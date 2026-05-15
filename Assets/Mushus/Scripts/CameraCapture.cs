using System;
using System.IO;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
[RequireComponent(typeof(Camera))]
public class CameraCapture : MonoBehaviour
{
    [Tooltip("保存先フォルダ。絶対パス or プロジェクト相対(Assets/...)。空なら既定パスを使用。ファイル名はこのコンポーネントを持つオブジェクト名.pngになります。")]
    public string outputPath = "";

    [Min(8)] public int width = 1920;
    [Min(8)] public int height = 1080;

    [Tooltip("SolidColor + A=0で背景透明を試みます（パイプラインや設定により非対応の場合あり）。")]
    public bool transparentBackground = false;

    Camera _cam;

    void OnEnable()
    {
        _cam = GetComponent<Camera>();
    }

    /// <summary>
    /// 既定の保存パス（プロジェクト内 Screenshots/Camera_YYYYMMDD_HHMMSS.png の絶対パス）
    /// </summary>
    public string BuildDefaultPath()
    {
        var root = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        var dir = Path.Combine(root, "Screenshots");
        var name = $"Camera_{gameObject.name}_{DateTime.Now:yyyyMMdd_HHmmss}.png";
        return Path.Combine(dir, name);
    }

    /// <summary>
    /// 指定パス（ディレクトリ/ファイル）とカメラ名から保存パスを構築
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
            var fileName = $"{SanitizeFileName(gameObject != null ? gameObject.name : "Camera")}.png";
            return Path.Combine(absBasePath, fileName);
        }

        return absBasePath;
    }

    private static string SanitizeFileName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return "Camera";
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
    /// 現在のカメラを PNG で保存。成功時は true を返す。
    /// </summary>
    public bool CaptureNow(string overridePath = null)
    {
        if (_cam == null) _cam = GetComponent<Camera>();
        if (_cam == null) { Debug.LogError("CameraCapture: カメラが見つかりません。"); return false; }

        int w = Mathf.Max(8, width);
        int h = Mathf.Max(8, height);

        var absPath = BuildSavePath(overridePath);
        var dir = Path.GetDirectoryName(absPath);
        if (string.IsNullOrEmpty(dir)) { Debug.LogError("CameraCapture: 保存先ディレクトリが不正です。"); return false; }
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

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

            Debug.Log($"CameraCapture: 保存しました → {absPath}");

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
            Debug.LogError($"CameraCapture: 保存に失敗しました。\n{e}");
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
}
