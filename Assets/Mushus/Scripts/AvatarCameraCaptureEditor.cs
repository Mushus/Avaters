#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System;
using System.IO;

[CustomEditor(typeof(AvatarCameraCapture))]
public class AvatarCameraCaptureEditor : Editor
{
    SerializedProperty outputPath;
    SerializedProperty width;
    SerializedProperty height;
    SerializedProperty transparentBackground;
    SerializedProperty targetAvatar;
    SerializedProperty animationClip;
    SerializedProperty normalizedTime;
    SerializedProperty layerIndex;

    void OnEnable() 
    {
        outputPath = serializedObject.FindProperty("outputPath");
        width = serializedObject.FindProperty("width");
        height = serializedObject.FindProperty("height");
        transparentBackground = serializedObject.FindProperty("transparentBackground");
        targetAvatar = serializedObject.FindProperty("targetAvatar");
        animationClip = serializedObject.FindProperty("animationClip");
        normalizedTime = serializedObject.FindProperty("normalizedTime");
        layerIndex = serializedObject.FindProperty("layerIndex");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Capture Settings
        EditorGUILayout.LabelField("Capture Settings", EditorStyles.boldLabel);

        // 保存パス行
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(outputPath, new GUIContent("Output Path"));
        if (GUILayout.Button("参照...", GUILayout.Width(70)))
        {
            var t = (AvatarCameraCapture)target;
            var suggestedDir = Path.GetDirectoryName(t.BuildDefaultPath());
            var currentDir = string.IsNullOrEmpty(outputPath.stringValue)
                ? suggestedDir
                : Path.GetDirectoryName(t.BuildSavePath());

            var picked = EditorUtility.OpenFolderPanel("保存先フォルダを選択", currentDir, "");
            if (!string.IsNullOrEmpty(picked))
            {
                // 可能ならプロジェクト相対へ変換して保存
                var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, "..")) + Path.DirectorySeparatorChar;
                var pickedFull = Path.GetFullPath(picked);
                if (pickedFull.StartsWith(projectRoot, StringComparison.OrdinalIgnoreCase))
                {
                    var rel = pickedFull.Substring(projectRoot.Length).Replace('\\', '/');
                    outputPath.stringValue = rel; // 例: "Assets/Screenshots"
                }
                else
                {
                    outputPath.stringValue = pickedFull; // 絶対パス（フォルダ）
                }
            }
        }
        if (GUILayout.Button("既定", GUILayout.Width(50)))
        {
            var t = (AvatarCameraCapture)target;
            outputPath.stringValue = Path.GetDirectoryName(t.BuildDefaultPath());
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.IntSlider(width, 8, 8192, new GUIContent("Width"));
        EditorGUILayout.IntSlider(height, 8, 8192, new GUIContent("Height"));
        EditorGUILayout.PropertyField(transparentBackground, new GUIContent("Transparent Background"));

        EditorGUILayout.Space(8);

        // Avatar Animation Settings
        EditorGUILayout.LabelField("Avatar Animation Settings", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(targetAvatar, new GUIContent("Target Avatar"));
        EditorGUILayout.PropertyField(animationClip, new GUIContent("Animation Clip"));

        // アニメーション情報の表示
        if (animationClip.objectReferenceValue != null)
        {
            var clip = animationClip.objectReferenceValue as AnimationClip;
            EditorGUILayout.HelpBox($"Animation Length: {clip.length:F2} seconds", MessageType.Info);

            // 正規化時間スライダー
            EditorGUILayout.PropertyField(normalizedTime, new GUIContent("Normalized Time"));

            // 実際の時間を表示
            float actualTime = clip.length * normalizedTime.floatValue;
            EditorGUILayout.LabelField("Actual Time", $"{actualTime:F3} seconds");

            // クイック選択ボタン
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Start (0%)"))
                normalizedTime.floatValue = 0f;
            if (GUILayout.Button("25%"))
                normalizedTime.floatValue = 0.25f;
            if (GUILayout.Button("50%"))
                normalizedTime.floatValue = 0.5f;
            if (GUILayout.Button("75%"))
                normalizedTime.floatValue = 0.75f;
            if (GUILayout.Button("End (100%)"))
                normalizedTime.floatValue = 1f;
            EditorGUILayout.EndHorizontal();
        }
        else
        {
            EditorGUILayout.HelpBox("Animation Clipを設定してください。アニメーションなしでも撮影できますが、ポーズは現在の状態になります。", MessageType.Warning);
        }

        EditorGUILayout.PropertyField(layerIndex, new GUIContent("Layer Index"));

        // プレビュー & 撮影ボタン
        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Preview & Capture", EditorStyles.boldLabel);

        using (new EditorGUI.DisabledScope(targetAvatar.objectReferenceValue == null || animationClip.objectReferenceValue == null))
        {
            EditorGUILayout.BeginHorizontal();

            // プレビュー開始ボタン
            if (GUILayout.Button("🎬 プレビュー", GUILayout.Height(28)))
            {
                var t = (AvatarCameraCapture)target;
                t.PreviewAnimation();
            }

            // プレビュー停止ボタン
            if (GUILayout.Button("⏹ 停止", GUILayout.Height(28)))
            {
                var t = (AvatarCameraCapture)target;
                t.StopPreview();
            }

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.Space(4);

        using (new EditorGUI.DisabledScope(EditorApplication.isCompiling || targetAvatar.objectReferenceValue == null))
        {
            if (GUILayout.Button("📸 撮影 (PNG保存)", GUILayout.Height(32)))
            {
                var t = (AvatarCameraCapture)target;
                var ok = t.CaptureNow();
                if (ok)
                {
                    var abs = t.BuildSavePath();
                    EditorUtility.RevealInFinder(abs);
                }
            }
        }

        // 使い方ヘルプ
        EditorGUILayout.Space(8);
        EditorGUILayout.HelpBox(
            "使い方:\n" +
            "1. シーン上でアバターを任意のポーズに配置（このポーズが保持されます）\n" +
            "2. Target Avatarにアニメーションを適用したいGameObjectを設定\n" +
            "3. Animation Clipに再生したいアニメーション（表情など）を設定\n" +
            "4. Normalized Timeでアニメーションのどの時点で撮影するかを指定\n" +
            "5. 「プレビュー」ボタンでアニメーションを確認\n" +
            "6. カメラ位置を調整（必要に応じて）\n" +
            "7. 「撮影」ボタンで画像を保存\n" +
            "8. 「停止」ボタンでプレビューを終了して元の姿勢に戻す\n\n" +
            "新機能: Humanoidアバターのボーンポーズは自動的に保持されます。\n" +
            "アニメーションの表情・マテリアル変更のみが適用されます。",
            MessageType.Info);

        serializedObject.ApplyModifiedProperties();
    }

    [MenuItem("GameObject/Camera/Attach AvatarCameraCapture", priority = 11)]
    static void AttachToSelected()
    {
        var go = Selection.activeGameObject;
        if (go == null)
        {
            EditorUtility.DisplayDialog("AvatarCameraCapture", "カメラを選択してください。", "OK");
            return;
        }

        var cam = go.GetComponent<Camera>();
        if (cam == null)
        {
            EditorUtility.DisplayDialog("AvatarCameraCapture", "選択オブジェクトに Camera コンポーネントがありません。", "OK");
            return;
        }

        if (go.GetComponent<AvatarCameraCapture>() == null)
        {
            Undo.AddComponent<AvatarCameraCapture>(go);
        }
        Selection.activeGameObject = go;
    }

    [MenuItem("GameObject/Camera/Attach AvatarCameraCapture", true)]
    static bool AttachToSelected_Validate()
    {
        return Selection.activeGameObject != null && Selection.activeGameObject.GetComponent<Camera>() != null;
    }
}
#endif
