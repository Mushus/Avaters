#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System;
using System.IO;

[CustomEditor(typeof(CameraCapture))]
public class CameraCaptureEditor : Editor
{
    SerializedProperty outputPath;
    SerializedProperty width;
    SerializedProperty height;
    SerializedProperty transparentBackground;

    void OnEnable()
    {
        outputPath = serializedObject.FindProperty("outputPath");
        width = serializedObject.FindProperty("width");
        height = serializedObject.FindProperty("height");
        transparentBackground = serializedObject.FindProperty("transparentBackground");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField("Capture Settings", EditorStyles.boldLabel);

        // 保存パス行
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(outputPath, new GUIContent("Output Path"));
        if (GUILayout.Button("参照...", GUILayout.Width(70)))
        {
            var t = (CameraCapture)target;
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
            var t = (CameraCapture)target;
            outputPath.stringValue = Path.GetDirectoryName(t.BuildDefaultPath());
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.IntSlider(width, 8, 8192, new GUIContent("Width"));
        EditorGUILayout.IntSlider(height, 8, 8192, new GUIContent("Height"));
        EditorGUILayout.PropertyField(transparentBackground, new GUIContent("Transparent Background"));

        EditorGUILayout.Space(8);
        using (new EditorGUI.DisabledScope(EditorApplication.isCompiling))
        {
            if (GUILayout.Button("📸 撮影 (PNG保存)", GUILayout.Height(32)))
            {
                var t = (CameraCapture)target;
                var ok = t.CaptureNow();
                if (ok)
                {
                    var abs = t.BuildSavePath();
                    EditorUtility.RevealInFinder(abs);
                }
            }
        }

        serializedObject.ApplyModifiedProperties();
    }

    [MenuItem("GameObject/Camera/Attach CameraCapture", priority = 10)]
    static void AttachToSelected()
    {
        var go = Selection.activeGameObject;
        if (go == null) { EditorUtility.DisplayDialog("CameraCapture", "カメラを選択してください。", "OK"); return; }

        var cam = go.GetComponent<Camera>();
        if (cam == null) { EditorUtility.DisplayDialog("CameraCapture", "選択オブジェクトに Camera コンポーネントがありません。", "OK"); return; }

        if (go.GetComponent<CameraCapture>() == null)
        {
            Undo.AddComponent<CameraCapture>(go);
        }
        Selection.activeGameObject = go;
    }

    [MenuItem("GameObject/Camera/Attach CameraCapture", true)]
    static bool AttachToSelected_Validate()
    {
        return Selection.activeGameObject != null && Selection.activeGameObject.GetComponent<Camera>() != null;
    }
}
#endif
