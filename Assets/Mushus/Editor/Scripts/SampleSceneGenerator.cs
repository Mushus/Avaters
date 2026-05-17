using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
#if VRC_SDK_VRCSDK3
using VRC.Core;
#endif

namespace Mushus.EditorTools.Scripts
{
    public class SampleSceneGenerator : UnityEditor.Editor
    {
        private static readonly string[] ExcludedAvatars = { "Windra", "RedDragon" };
        private static readonly string MushusRoot = "Assets/Mushus";

        [MenuItem("Mushus/Tools/Generate All Sample Scenes")]
        public static void GenerateAllSampleScenes()
        {

            var directories = Directory.GetDirectories(MushusRoot);
            int processedCount = 0;

            foreach (var dir in directories)
            {
                string dirName = Path.GetFileName(dir);

                // 無視するフォルダの判定
                if (dirName.EndsWith("Dev") || 
                    dirName.EndsWith("V0") || 
                    dirName.Contains("Backup") ||
                    dirName == "Editor" || 
                    dirName == "Capture" || 
                    dirName == "Scripts" || 
                    dirName == "Icons" || 
                    dirName == "ScenesDev")
                {
                    continue;
                }

                // 除外指定されたアバターの判定
                bool isExcluded = false;
                foreach (var excluded in ExcludedAvatars)
                {
                    if (dirName == excluded)
                    {
                        isExcluded = true;
                        break;
                    }
                }
                if (isExcluded)
                {
                    Debug.Log($"[SampleSceneGenerator] {dirName} は作業中のためスキップします。");
                    continue;
                }

                if (ProcessAvatar(dirName))
                {
                    processedCount++;
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[SampleSceneGenerator] 完了しました。処理したアバター数: {processedCount}");
        }

        private static bool ProcessAvatar(string avatarName)
        {
            string prefabDirPath = $"{MushusRoot}/{avatarName}/Prefabs";
            if (!Directory.Exists(prefabDirPath))
            {
                return false;
            }

            // 配布用Prefabを検索
            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { prefabDirPath });
            if (prefabGuids.Length == 0)
            {
                return false;
            }

            List<GameObject> prefabs = new List<GameObject>();
            foreach (string guid in prefabGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null)
                {
                    prefabs.Add(prefab);
                    ClearBlueprintIdFromAsset(prefab);
                }
            }

            if (prefabs.Count == 0) return false;

            // 新規シーンを作成 (デフォルトのMain CameraとDirectional Lightが含まれる)
            Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            // Prefabをシーンに配置
            float xOffset = 0f;
            foreach (var prefab in prefabs)
            {
                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                instance.transform.position = new Vector3(xOffset, 0, 0);
                xOffset += 1.5f; // 複数ある場合は横にずらす

                // シーン上のインスタンスからもBlueprint IDをクリア
                ClearBlueprintIdFromInstance(instance);
            }

            // Scenesフォルダが存在しなければ作成
            string scenesDirPath = $"{MushusRoot}/{avatarName}/Scenes";
            if (!Directory.Exists(scenesDirPath))
            {
                Directory.CreateDirectory(scenesDirPath);
            }

            // シーンを保存
            string scenePath = $"{scenesDirPath}/SampleScene.unity";
            EditorSceneManager.SaveScene(newScene, scenePath);
            Debug.Log($"[SampleSceneGenerator] 生成完了: {scenePath}");

            return true;
        }

        private static void ClearBlueprintIdFromAsset(GameObject prefab)
        {
#if VRC_SDK_VRCSDK3
            // PrefabのルートにあるPipelineManagerを探す
            var pipelineManager = prefab.GetComponent<PipelineManager>();
            if (pipelineManager != null && !string.IsNullOrEmpty(pipelineManager.blueprintId))
            {
                pipelineManager.blueprintId = "";
                EditorUtility.SetDirty(prefab);
                Debug.Log($"[SampleSceneGenerator] {prefab.name} アセットの Blueprint ID をクリアしました。");
            }
#endif
        }

        private static void ClearBlueprintIdFromInstance(GameObject instance)
        {
#if VRC_SDK_VRCSDK3
            var pipelineManager = instance.GetComponent<PipelineManager>();
            if (pipelineManager != null && !string.IsNullOrEmpty(pipelineManager.blueprintId))
            {
                pipelineManager.blueprintId = "";
                EditorUtility.SetDirty(instance);
            }
#endif
        }
    }
}
