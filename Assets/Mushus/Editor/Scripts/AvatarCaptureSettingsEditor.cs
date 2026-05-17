using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using System.IO;
using UnityEngine.SceneManagement;

namespace Mushus.CaptureTools
{
    [CustomEditor(typeof(AvatarCaptureSettings))]
    public class AvatarCaptureSettingsEditor : Editor
    { 
        [MenuItem("GameObject/Mushus/Capture/Avatar Capture Settings", false, 0)]
        public static void CreateCaptureSettings(MenuCommand menuCommand)
        {
            GameObject obj = new GameObject("AvatarCaptureSettings");
            var settings = obj.AddComponent<AvatarCaptureSettings>();
            
            var descriptor = FindFirstObjectByType<VRCAvatarDescriptor>();
            if (descriptor != null)
            {
                settings.TargetAvatar = descriptor.gameObject;
                obj.name = $"{descriptor.gameObject.name}_CaptureSettings";
            }

            GameObjectUtility.SetParentAndAlign(obj, menuCommand.context as GameObject);
            Undo.RegisterCreatedObjectUndo(obj, "Create Capture Settings");

            SetupAllCamerasInternal(settings);
            AutoDetectPackageName(settings);

            Selection.activeObject = obj;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            var settings = (AvatarCaptureSettings)target;

            EditorGUILayout.PropertyField(serializedObject.FindProperty("TargetAvatar"));
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Output Settings", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("PackageName"));
            if (GUILayout.Button("Auto-detect", GUILayout.Width(80)))
            {
                AutoDetectPackageName(settings);
            }
            EditorGUILayout.EndHorizontal();

            // パスプレビュー
            string packageName = string.IsNullOrEmpty(settings.PackageName) ? "???" : settings.PackageName;
            EditorGUILayout.HelpBox(
                $"Previews: Assets/Mushus/{packageName}Dev/Previews/\n" +
                $"Icons: Assets/Mushus/{packageName}/Icons/", 
                MessageType.None);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Cameras (Orthographic & Transparent)", EditorStyles.boldLabel);
            
            if (settings.FrontCamera == null || settings.BackCamera == null || settings.SideCamera == null || settings.FaceCamera == null)
            {
                if (GUILayout.Button("Create Missing Cameras", GUILayout.Height(30)))
                {
                    SetupAllCamerasInternal(settings);
                }
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty("FrontCamera"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("BackCamera"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("SideCamera"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("FaceCamera"));
 
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Expression Animations", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("ExpressionClips"), true);
            if (GUILayout.Button("Auto-detect Clips from Animator"))
            {
                var clips = CaptureExecutor.DetectExpressionClips(settings.TargetAvatar);
                if (clips.Count > 0)
                {
                    settings.ExpressionClips = clips;
                    EditorUtility.SetDirty(settings);
                    Debug.Log($"[Capture] Detected {clips.Count} expression clips.");
                }
                else
                {
                    Debug.LogWarning("[Capture] No expression clips detected in FX controller.");
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Execution", EditorStyles.boldLabel);
            
            GUI.backgroundColor = new Color(0.7f, 1f, 0.7f);
            if (GUILayout.Button("Capture Previews (Transparent BG)", GUILayout.Height(40)))
            {
                CaptureExecutor.ExecuteThumbnailCapture(settings);
            }

            if (GUILayout.Button("Capture Expression Icons", GUILayout.Height(40)))
            {
                CaptureExecutor.ExecuteExpressionCapture(settings);
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Marketing Materials", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("ThumbnailBackgroundColor"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("ThumbnailFontPath"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("ThumbnailFontSize"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("ThumbnailLabelPadding"));

            if (GUILayout.Button("Generate Marketing Thumbnails", GUILayout.Height(40)))
            {
                AvatarThumbnailGenerator.GenerateMarketingImages(settings);
            }
            GUI.backgroundColor = Color.white;

            serializedObject.ApplyModifiedProperties();
        }

        private static void AutoDetectPackageName(AvatarCaptureSettings settings)
        {
            Undo.RecordObject(settings, "Auto Detect Package Name");
            
            // シーンのパスから推測 (Assets/Mushus/PackageName/...)
            string scenePath = SceneManager.GetActiveScene().path;
            if (!string.IsNullOrEmpty(scenePath) && scenePath.StartsWith("Assets/Mushus/"))
            {
                string relative = scenePath.Replace("Assets/Mushus/", "");
                string[] parts = relative.Split('/');
                if (parts.Length > 0)
                {
                    settings.PackageName = parts[0];
                    Debug.Log($"[Capture] Auto-detected PackageName: {settings.PackageName}");
                }
            }
            else if (settings.TargetAvatar != null)
            {
                // アバター名から (とりあえず)
                settings.PackageName = settings.TargetAvatar.name;
            }

            EditorUtility.SetDirty(settings);
        }

        private static void SetupAllCamerasInternal(AvatarCaptureSettings settings)
        {
            Undo.RecordObject(settings, "Setup All Cameras");

            float eyeHeight = 1.45f;
            Vector3 center = new Vector3(0, 0.8f, 0);
            
            if (settings.TargetAvatar != null)
            {
                var descriptor = settings.TargetAvatar.GetComponent<VRCAvatarDescriptor>();
                if (descriptor != null) eyeHeight = descriptor.ViewPosition.y;

                var renderers = settings.TargetAvatar.GetComponentsInChildren<Renderer>();
                if (renderers.Length > 0)
                {
                    Bounds b = renderers[0].bounds;
                    foreach (var r in renderers) b.Encapsulate(r.bounds);
                    center = b.center;
                }
            }

            Camera CreateCam(string name, Vector3 pos, Vector3 rot, float size)
            {
                GameObject obj = new GameObject(name);
                obj.transform.SetParent(settings.transform);
                obj.transform.localPosition = pos;
                obj.transform.localRotation = Quaternion.Euler(rot);
                var cam = obj.AddComponent<Camera>();
                
                cam.orthographic = true;
                cam.orthographicSize = size;
                cam.nearClipPlane = 0.01f;
                cam.farClipPlane = 100f;
                cam.cullingMask = -1;
                
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = new Color(0, 0, 0, 0);
                return cam;
            }

            if (settings.FrontCamera == null) settings.FrontCamera = CreateCam("FrontCamera", new Vector3(0, eyeHeight, 5.0f), new Vector3(0, 180, 0), 1.0f);
            if (settings.BackCamera == null) settings.BackCamera = CreateCam("BackCamera", new Vector3(0, center.y, -5.0f), new Vector3(0, 0, 0), 1.0f);
            if (settings.SideCamera == null) settings.SideCamera = CreateCam("SideCamera", new Vector3(-5.0f, center.y, 0), new Vector3(0, 90, 0), 1.0f);
            if (settings.FaceCamera == null) settings.FaceCamera = CreateCam("FaceCamera", new Vector3(0, eyeHeight, 2.0f), new Vector3(0, 180, 0), 0.15f);

            EditorUtility.SetDirty(settings);
        }
    }
}
