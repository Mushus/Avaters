using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Mushus.DistributionTools
{
    [CustomEditor(typeof(AvatarDistributionSettings))]
    public class AvatarDistributionSettingsEditor : Editor
    {
        [MenuItem("GameObject/Mushus/Avatar Distribution Settings", false, 0)]
        public static void CreateDistributionSettings(MenuCommand menuCommand)
        {
            GameObject obj = new GameObject("AvatarDistributionSettings");
            var settings = obj.AddComponent<AvatarDistributionSettings>();
            
            // シーン内のアバターを自動検出して設定
            var descriptor = FindFirstObjectByType<VRC.SDK3.Avatars.Components.VRCAvatarDescriptor>();
            if (descriptor != null)
            {
                settings.TargetAvatar = descriptor.gameObject;
                obj.name = $"{descriptor.gameObject.name}_DistributionSettings";
            }

            GameObjectUtility.SetParentAndAlign(obj, menuCommand.context as GameObject);
            Undo.RegisterCreatedObjectUndo(obj, "Create Distribution Settings");
            Selection.activeObject = obj;
        }

        public override void OnInspectorGUI()
        {
            var settings = (AvatarDistributionSettings)target;

            // 基本設定の描画
            serializedObject.Update();
            DrawDefaultInspector();
            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Distribution Controls", EditorStyles.boldLabel);

            if (settings.TargetAvatar == null)
            {
                EditorGUILayout.HelpBox("Target Avatar Prefab を設定してください。", MessageType.Warning);
                return;
            }

            // 解析ボタン
            if (GUILayout.Button("1. Analyze Avatar", GUILayout.Height(30)))
            {
                settings.CurrentSpec = AvatarSpecCollector.Collect(settings.TargetAvatar);
                EditorUtility.SetDirty(settings);
            }

            if (settings.CurrentSpec != null && !string.IsNullOrEmpty(settings.CurrentSpec.Name))
            {
                DrawSpecPreview(settings.CurrentSpec);

                EditorGUILayout.Space();

                // パッケージ生成ボタン
                GUI.backgroundColor = new Color(0.7f, 1f, 0.7f);
                if (GUILayout.Button("2. Generate Distribution Package", GUILayout.Height(40)))
                {
                    GeneratePackage(settings);
                }
                GUI.backgroundColor = Color.white;

                // Zip化ボタン
                if (GUILayout.Button("3. Create Zip Archive", GUILayout.Height(30)))
                {
                    CreateZipArchive(settings);
                }
            }
        }

        private void DrawSpecPreview(AvatarSpec spec)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label($"Analysis Result: {spec.Name}", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Polygons", spec.PolyCount.ToString("N0"));
            EditorGUILayout.LabelField("Materials", spec.MaterialCount.ToString());
            EditorGUILayout.LabelField("PhysBones", spec.PhysBoneCount.ToString());

            EditorGUILayout.Space();
            GUILayout.Label("LipSync Shapes:", EditorStyles.miniBoldLabel);
            EditorGUILayout.LabelField(spec.LipSyncShapes != null ? string.Join(", ", spec.LipSyncShapes) : "None");

            EditorGUILayout.Space();
            GUILayout.Label("Key Face Shapes (Top 10):", EditorStyles.miniBoldLabel);
            if (spec.FaceShapes != null)
            {
                string faceDisplay = string.Join(", ", spec.FaceShapes.Take(10));
                if (spec.FaceShapes.Count > 10) faceDisplay += "...";
                EditorGUILayout.LabelField(faceDisplay);
            }
            else
            {
                EditorGUILayout.LabelField("None");
            }

            EditorGUILayout.EndVertical();
        }

        private void GeneratePackage(AvatarDistributionSettings settings)
        {
            string avatarName = settings.TargetAvatar.name;
            string rootPath = $"Products/{avatarName}";
            string distPath = $"{rootPath}/{avatarName}";
            string srcPath = $"{distPath}/src";

            // フォルダ作成
            if (Directory.Exists(distPath)) Directory.Delete(distPath, true);
            Directory.CreateDirectory(distPath);
            Directory.CreateDirectory(srcPath);

            // 1. UnityPackageの書き出し
            string packagePath = $"{distPath}/{avatarName}.unitypackage";
            string mushusRoot = "Assets/Mushus/" + avatarName;

            if (Directory.Exists(mushusRoot))
            {
                AssetDatabase.ExportPackage(mushusRoot, packagePath, ExportPackageOptions.Recurse);
                Debug.Log($"Exported UnityPackage: {packagePath}");
            }
            else
            {
                Debug.LogWarning($"Source directory not found: {mushusRoot}. Skipping UnityPackage export.");
            }

            // 2. README.txt 生成
            string readmeContent = GenerateReadme(settings);
            File.WriteAllText($"{distPath}/README.txt", readmeContent);

            // 3. booth_info.md 生成
            string boothInfo = GenerateBoothInfo(settings);
            File.WriteAllText($"{rootPath}/booth_info.md", boothInfo);

            AssetDatabase.Refresh();
            Debug.Log($"[{avatarName}] Distribution package generation completed!");
        }

        private void CreateZipArchive(AvatarDistributionSettings settings)
        {
            string avatarName = settings.TargetAvatar.name;
            string rootPath = Path.GetFullPath($"Products/{avatarName}");
            string sourcePath = Path.Combine(rootPath, avatarName);
            string zipPath = Path.Combine(rootPath, $"{avatarName}.zip");

            if (!Directory.Exists(sourcePath))
            {
                Debug.LogError($"Source directory for zip does not exist: {sourcePath}");
                return;
            }

            if (File.Exists(zipPath)) File.Delete(zipPath);

            // PowerShellを使ってzip化
            string command = $"Compress-Archive -Path '{sourcePath}' -DestinationPath '{zipPath}' -Force";
            System.Diagnostics.Process.Start("powershell.exe", $"-Command \"{command}\"");

            Debug.Log($"[{avatarName}] Zip archive creation started: {zipPath}");
        }

        private string GenerateReadme(AvatarDistributionSettings settings)
        {
            var spec = settings.CurrentSpec;
            return $"■ {settings.TargetAvatar.name} 配布パッケージ\n\n" +
                   $"▼ 利用規約\n{settings.LicenseText}\n\n" +
                   $"▼ オンライン説明書\n{settings.DescriptionUrl}\n\n" +
                   $"▼ アバタースペック\n" +
                   $"・ポリゴン数: {spec.PolyCount:N0}\n" +
                   $"・マテリアル数: {spec.MaterialCount}\n" +
                   $"・PhysBoneコンポーネント数: {spec.PhysBoneCount}\n" +
                   $"・リップシンク対応: {spec.LipSyncShapes.Count}種類\n" +
                   $"・表情シェイプキー: {spec.FaceShapes.Count}種類\n" +
                   $"・使用シェーダー: {string.Join(", ", spec.Shaders)}\n";
        }

        private string GenerateBoothInfo(AvatarDistributionSettings settings)
        {
            var spec = settings.CurrentSpec;
            return $"## {settings.TargetAvatar.name} アセット詳細\n\n" +
                   $"### スペック情報\n" +
                   $"- ポリゴン数: {spec.PolyCount:N0}\n" +
                   $"- マテリアル数: {spec.MaterialCount}\n" +
                   $"- 使用シェーダー: {string.Join(", ", spec.Shaders)}\n" +
                   $"- PhysBone数: {spec.PhysBoneCount}\n" +
                   $"- リップシンク対応: {spec.LipSyncShapes.Count}種類\n" +
                   $"- 表情用シェイプキー: {spec.FaceShapes.Count}種類\n\n" +
                   $"### リップシンク詳細\n" +
                   $"{string.Join(", ", spec.LipSyncShapes)}\n\n" +
                   $"### 表情シェイプキー（一部抜粋）\n" +
                   $"{string.Join(", ", spec.FaceShapes.Take(20))}{(spec.FaceShapes.Count > 20 ? " ...など" : "")}\n\n" +
                   $"### オンライン説明書\n{settings.DescriptionUrl}\n\n" +
                   $"### 利用規約\n{settings.LicenseText}\n";
        }
    }
}
