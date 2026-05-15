using System;
using System.IO;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
#endif

[ExecuteAlways]
[RequireComponent(typeof(Camera))]
public class AvatarCameraCapture : MonoBehaviour
{
#if UNITY_EDITOR
    // プレビュー用の一時クローン管理
    private GameObject _previewClone;
    private PlayableGraph _previewGraph;
    private bool _previewOriginalActive = true;
#endif

    [Header("Capture Settings")]
    [Tooltip("保存先フォルダ。絶対パス or プロジェクト相対(Assets/...)。空なら既定パスを使用。ファイル名はこのコンポーネントを持つオブジェクト名.pngになります。")]
    public string outputPath = "";

    [Min(8)] public int width = 1920;
    [Min(8)] public int height = 1080;

    [Tooltip("SolidColor + A=0で背景透明を試みます（パイプラインや設定により非対応の場合あり）。")]
    public bool transparentBackground = false;

    [Header("Avatar Animation Settings")]
    [Tooltip("アニメーションを適用する対象のGameObject（通常はアバターのルート）")]
    public GameObject targetAvatar;

    [Tooltip("撮影時に再生するアニメーションクリップ")]
    public AnimationClip animationClip;

    [Tooltip("アニメーションの正規化時間 (0.0 ~ 1.0)")]
    [Range(0f, 1f)]
    public float normalizedTime = 0f;

    [Tooltip("レイヤーインデックス（通常は0でBase Layer）")]
    public int layerIndex = 0;

    Camera _cam;

    void OnEnable()
    {
        _cam = GetComponent<Camera>();
    }

    /// <summary>
    /// 既定の保存パス（プロジェクト内 Screenshots/AvatarCamera_YYYYMMDD_HHMMSS.png の絶対パス）
    /// </summary>
    public string BuildDefaultPath()
    {
        var root = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        var dir = Path.Combine(root, "Screenshots");
        var captureObjectName = gameObject != null ? gameObject.name : "AvatarCamera";
        var animName = animationClip != null ? animationClip.name : "NoAnim";
        var name = $"AvatarCamera_{captureObjectName}_{animName}_{DateTime.Now:yyyyMMdd_HHmmss}.png";
        return Path.Combine(dir, name);
    }

    /// <summary>
    /// 指定パス（ディレクトリ/ファイル）とアバター名から保存パスを構築
    /// </summary>
    public string BuildSavePath(string overridePath = null)
    {
        var basePath = string.IsNullOrWhiteSpace(overridePath) ? outputPath : overridePath;

        if (string.IsNullOrWhiteSpace(basePath))
        {
            return BuildDefaultPath();
        }

        var absBasePath = ToAbsolutePath(basePath);
        if (IsDirectoryPath(absBasePath))
        {
            var fileName = $"{SanitizeFileName(gameObject != null ? gameObject.name : "AvatarCamera")}.png";
            return Path.Combine(absBasePath, fileName);
        }

        return absBasePath;
    }

    private static string SanitizeFileName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return "Avatar";
        foreach (var c in Path.GetInvalidFileNameChars())
        {
            name = name.Replace(c, '_');
        }
        return name;
    }

    private static bool IsDirectoryPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return false;
        if (Directory.Exists(path)) return true;
        return string.IsNullOrEmpty(Path.GetExtension(path));
    }

    /// <summary>
    /// 指定パスを絶対パスへ正規化（Assets/ 相対にも対応）
    /// </summary>
    public string ToAbsolutePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return BuildDefaultPath();

        // Assets 相対 → 絶対
        if (path.Replace('\\', '/').StartsWith("Assets/", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(path, "Assets", StringComparison.OrdinalIgnoreCase))
        {
            var root = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            return Path.GetFullPath(Path.Combine(root, path));
        }

        // 既に絶対パス
        return Path.GetFullPath(path);
    }

    /// <summary>
    /// アバターにアニメーションを適用して撮影
    /// </summary>
    public bool CaptureNow(string overridePath = null)
    {
        if (_cam == null) _cam = GetComponent<Camera>();
        if (_cam == null)
        {
            Debug.LogError("AvatarCameraCapture: カメラが見つかりません。");
            return false;
        }

        if (targetAvatar == null)
        {
            Debug.LogError("AvatarCameraCapture: Target Avatarが設定されていません。");
            return false;
        }

        Animator animator = targetAvatar.GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("AvatarCameraCapture: Target AvatarにAnimatorコンポーネントがありません。");
            return false;
        }

        int w = Mathf.Max(8, width);
        int h = Mathf.Max(8, height);

        var absPath = BuildSavePath(overridePath);
        var dir = Path.GetDirectoryName(absPath);
        if (string.IsNullOrEmpty(dir))
        {
            Debug.LogError("AvatarCameraCapture: 保存先ディレクトリが不正です。");
            return false;
        }
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

        bool success = false;

#if UNITY_EDITOR
        // エディタモードでアニメーションを適用
        success = CaptureWithAnimationInEditor(animator, absPath, w, h);
#else
        // ランタイムでは通常のAnimator再生を使用
        success = CaptureWithAnimationRuntime(animator, absPath, w, h);
#endif

        return success;
    }

#if UNITY_EDITOR
    /// <summary>
    /// エディタモードでアニメーションをサンプリングして撮影（ポーズ保持機能付き）
    /// </summary>
    private bool CaptureWithAnimationInEditor(Animator animator, string absPath, int w, int h)
    {
        if (animationClip == null)
        {
            Debug.LogWarning("AvatarCameraCapture: Animation Clipが設定されていません。アニメーションなしで撮影します。");
            return CaptureCamera(absPath, w, h);
        }

        // Humanoidでない場合はフォールバック
        if (!animator.isHuman)
        {
            Debug.LogWarning("AvatarCameraCapture: AnimatorがHumanoidではありません。ポーズ保持機能は使用できません。");
            return CaptureWithAnimationSimple(animator, absPath, w, h);
        }

        return CaptureWithPlayableClone(animator, absPath, w, h);
    }

    /// <summary>
    /// 非Humanoidアバター用のフォールバック処理
    /// </summary>
    private bool CaptureWithAnimationSimple(Animator animator, string absPath, int w, int h)
    {
        bool wasInAnimationMode = AnimationMode.InAnimationMode();

        try
        {
            if (!wasInAnimationMode)
            {
                AnimationMode.StartAnimationMode();
            }

            AnimationMode.BeginSampling();
            float time = animationClip.length * normalizedTime;
            AnimationMode.SampleAnimationClip(targetAvatar, animationClip, time);
            AnimationMode.EndSampling();

            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();

            return CaptureCamera(absPath, w, h);
        }
        catch (Exception e)
        {
            Debug.LogError($"AvatarCameraCapture: アニメーション適用中にエラーが発生しました。\n{e}");
            return false;
        }
        finally
        {
            if (!wasInAnimationMode && AnimationMode.InAnimationMode())
            {
                AnimationMode.StopAnimationMode();
            }
        }
    }

    /// <summary>
    /// アニメーションをプレビュー（撮影なし・ポーズ保持機能付き）
    /// </summary>
    public void PreviewAnimation()
    {
        if (targetAvatar == null || animationClip == null)
        {
            Debug.LogWarning("AvatarCameraCapture: Target AvatarまたはAnimation Clipが設定されていません。");
            return;
        }

        try
        {
            StopPreviewClone();

            if (!StartPreviewClone())
            {
                Debug.LogError("AvatarCameraCapture: プレビュー用のクローン作成に失敗しました。");
                return;
            }

            float time = animationClip.length * normalizedTime;
            Debug.Log($"AvatarCameraCapture: アニメーションをプレビューしました (Time: {time:F3}s)");
        }
        catch (Exception e)
        {
            Debug.LogError($"AvatarCameraCapture: プレビュー中にエラーが発生しました。\n{e}");
            StopPreviewClone();
        }
    }

    /// <summary>
    /// プレビューを終了して元の姿勢に戻す
    /// </summary>
    public void StopPreview()
    {
        StopPreviewClone();
        Debug.Log("AvatarCameraCapture: プレビューを停止し、元の姿勢に戻しました。");
    }

    /// <summary>
    /// プレビュー用に一時クローンを作成し、指定時間のアニメーションを適用
    /// </summary>
    private bool StartPreviewClone()
    {
        if (targetAvatar == null || animationClip == null) return false;

        StopPreviewClone(); // 念のため前回のクローンを破棄

        GameObject clone = null;
        PlayableGraph graph = default;

        _previewOriginalActive = targetAvatar.activeSelf;

        try
        {
            clone = Instantiate(targetAvatar, targetAvatar.transform.parent);
            clone.name = targetAvatar.name + "_PreviewTemp";
#if UNITY_EDITOR
            clone.hideFlags = HideFlags.HideAndDontSave;
#endif
            clone.transform.SetPositionAndRotation(targetAvatar.transform.position, targetAvatar.transform.rotation);
            clone.transform.localScale = targetAvatar.transform.localScale;

            targetAvatar.SetActive(false);

            var cloneAnimator = clone.GetComponent<Animator>();
            if (cloneAnimator == null)
            {
                Debug.LogError("AvatarCameraCapture: プレビュークローンにAnimatorが見つかりません。");
                StopPreviewClone();
                return false;
            }

            graph = PlayableGraph.Create("AvatarPreviewGraph");
            graph.SetTimeUpdateMode(DirectorUpdateMode.Manual);

            var clipPlayable = AnimationClipPlayable.Create(graph, animationClip);
            clipPlayable.SetApplyFootIK(false);

            var output = AnimationPlayableOutput.Create(graph, "Animation", cloneAnimator);
            output.SetSourcePlayable(clipPlayable);
            output.SetWeight(1f);

            cloneAnimator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            graph.Play();
            clipPlayable.SetTime(animationClip.length * normalizedTime);
            graph.Evaluate(0f);

            // ボーンは元のポーズを維持（表情のみ反映）
            CopyHumanoidPose(targetAvatar.GetComponent<Animator>(), cloneAnimator);

            _previewClone = clone;
            _previewGraph = graph;

#if UNITY_EDITOR
            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
#endif
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"AvatarCameraCapture: プレビュークローン作成中にエラーが発生しました。\n{e}");
            if (graph.IsValid()) graph.Destroy();
            if (clone != null)
            {
                if (Application.isPlaying)
                    Destroy(clone);
                else
                    DestroyImmediate(clone);
            }
            targetAvatar.SetActive(_previewOriginalActive);
            return false;
        }
    }

    /// <summary>
    /// プレビュー用クローンとPlayableGraphを破棄し、元の表示に戻す
    /// </summary>
    private void StopPreviewClone()
    {
        if (_previewGraph.IsValid()) _previewGraph.Destroy();
        _previewGraph = default;

        if (_previewClone != null)
        {
            if (Application.isPlaying)
                Destroy(_previewClone);
            else
                DestroyImmediate(_previewClone);
            _previewClone = null;
        }

        if (targetAvatar != null)
        {
            targetAvatar.SetActive(_previewOriginalActive);
        }
    }
#endif

    /// <summary>
    /// ランタイムでアニメーターを使用して撮影
    /// </summary>
    private bool CaptureWithAnimationRuntime(Animator animator, string absPath, int w, int h)
    {
        if (animationClip == null)
        {
            Debug.LogWarning("AvatarCameraCapture: Animation Clipが設定されていません。アニメーションなしで撮影します。");
            return CaptureCamera(absPath, w, h);
        }

        // AnimatorControllerが必要
        RuntimeAnimatorController controller = animator.runtimeAnimatorController;
        if (controller == null)
        {
            Debug.LogError("AvatarCameraCapture: AnimatorにRuntimeAnimatorControllerが設定されていません。");
            return false;
        }

        try
        {
            return CaptureWithPlayableClone(animator, absPath, w, h);
        }
        catch (Exception e)
        {
            Debug.LogError($"AvatarCameraCapture: アニメーション再生中にエラーが発生しました。\n{e}");
            return false;
        }
    }

    /// <summary>
    /// カメラでキャプチャを実行
    /// </summary>
    private bool CaptureCamera(string absPath, int w, int h)
    {
        // 退避
        var prevTarget = _cam.targetTexture;
        var prevActive = RenderTexture.active;
        var prevFlags = _cam.clearFlags;
        var prevBG = _cam.backgroundColor;

        // 透明背景を試みる（SRPやポストプロセスによっては効かない場合あり）
        if (transparentBackground)
        {
            _cam.clearFlags = CameraClearFlags.SolidColor;
            _cam.backgroundColor = new Color(0, 0, 0, 0);
        }

        RenderTexture rt = null;
        Texture2D tex = null;
        try
        {
            rt = new RenderTexture(w, h, 24, RenderTextureFormat.ARGB32);
            _cam.targetTexture = rt;
            _cam.Render();

            RenderTexture.active = rt;
            tex = new Texture2D(w, h, TextureFormat.RGBA32, false, false);
            tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
            tex.Apply(false, false);

            var png = ImageConversion.EncodeToPNG(tex);
            File.WriteAllBytes(absPath, png);

            Debug.Log($"AvatarCameraCapture: 保存しました → {absPath}");

#if UNITY_EDITOR
            // プロジェクト配下ならアセットDBを更新
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, "..")) + Path.DirectorySeparatorChar;
            if (absPath.StartsWith(projectRoot, StringComparison.OrdinalIgnoreCase))
            {
                AssetDatabase.Refresh();
            }
#endif

            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"AvatarCameraCapture: 保存に失敗しました。\n{e}");
            return false;
        }
        finally
        {
            _cam.targetTexture = prevTarget;
            RenderTexture.active = prevActive;

            if (transparentBackground)
            {
                _cam.clearFlags = prevFlags;
                _cam.backgroundColor = prevBG;
            }

            if (rt != null) rt.Release();
            if (tex != null) DestroyImmediate(tex);
        }
    }

    /// <summary>
    /// Humanoidのボーンポーズをソースからデスティネーションへコピー（表情やBlendShapeは触らない）
    /// </summary>
    private void CopyHumanoidPose(Animator source, Animator destination)
    {
        if (source == null || destination == null) return;
        if (!source.isHuman || !destination.isHuman) return;

        try
        {
            var srcHandler = new HumanPoseHandler(source.avatar, source.transform);
            var dstHandler = new HumanPoseHandler(destination.avatar, destination.transform);

            HumanPose pose = new HumanPose();
            srcHandler.GetHumanPose(ref pose);
            dstHandler.SetHumanPose(ref pose);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"AvatarCameraCapture: Humanoidポーズのコピーに失敗しました。\n{e}");
        }
    }

    /// <summary>
    /// 撮影専用の一時クローンにアニメーションを適用してキャプチャ（元アバターを触らない）
    /// </summary>
    private bool CaptureWithPlayableClone(Animator animator, string absPath, int w, int h)
    {
        GameObject clone = null;
        PlayableGraph graph = default;
        bool originalActive = targetAvatar.activeSelf;

        try
        {
            // 一時クローンを作成し、元の表示をオフにする
            clone = Instantiate(targetAvatar, targetAvatar.transform.parent);
            clone.name = targetAvatar.name + "_CaptureTemp";
#if UNITY_EDITOR
            clone.hideFlags = HideFlags.HideAndDontSave;
#endif
            clone.transform.SetPositionAndRotation(targetAvatar.transform.position, targetAvatar.transform.rotation);
            clone.transform.localScale = targetAvatar.transform.localScale;
            targetAvatar.SetActive(false);

            var cloneAnimator = clone.GetComponent<Animator>();
            if (cloneAnimator == null)
            {
                Debug.LogError("AvatarCameraCapture: クローンにAnimatorが見つかりません。");
                return false;
            }

            // PlayableGraphで指定時間のポーズを適用
            graph = PlayableGraph.Create("AvatarCaptureGraph");
            graph.SetTimeUpdateMode(DirectorUpdateMode.Manual);

            var clipPlayable = AnimationClipPlayable.Create(graph, animationClip);
            clipPlayable.SetApplyFootIK(false);

            var output = AnimationPlayableOutput.Create(graph, "Animation", cloneAnimator);
            output.SetSourcePlayable(clipPlayable);
            output.SetWeight(1f);

            cloneAnimator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            graph.Play();
            clipPlayable.SetTime(animationClip.length * normalizedTime);
            graph.Evaluate(0f); // 指定時刻で一度評価してポーズを確定

            // ボーンは元のポーズを維持（表情のみ反映）
            CopyHumanoidPose(animator, cloneAnimator);

            // キャプチャ実行
            return CaptureCamera(absPath, w, h);
        }
        catch (Exception e)
        {
            Debug.LogError($"AvatarCameraCapture: 撮影用クローンでエラーが発生しました。\n{e}");
            return false;
        }
        finally
        {
            if (graph.IsValid()) graph.Destroy();
            if (clone != null)
            {
                if (Application.isPlaying)
                    Destroy(clone);
                else
                    DestroyImmediate(clone);
            }

            targetAvatar.SetActive(originalActive);
        }
    }
}
