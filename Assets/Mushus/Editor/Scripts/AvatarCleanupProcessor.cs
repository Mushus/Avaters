using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Linq;
using System.Collections.Generic;
using VRC.Core;

public class AvatarCleanupProcessor : EditorWindow
{
    [MenuItem("Tools/Mushus/Run Avatar Cleanup")]
    public static void RunCleanup()
    {
        Debug.Log("Starting Avatar Cleanup...");

        // 1. Devシーン内のBlueprint IDをバックアップ
        var devScenes = AssetDatabase.FindAssets("t:Scene", new[] { "Assets/Mushus" })
            .Select(AssetDatabase.GUIDToAssetPath)
            .Where(p => p.Contains("Dev/"))
            .ToList();

        var sceneIdMap = new Dictionary<string, Dictionary<string, string>>();

        foreach (var scenePath in devScenes)
        {
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            var idMap = new Dictionary<string, string>();
            
            var pipelineManagers = Resources.FindObjectsOfTypeAll<PipelineManager>()
                .Where(p => p.gameObject.scene == scene)
                .ToList();

            foreach (var pm in pipelineManagers)
            {
                if (!string.IsNullOrEmpty(pm.blueprintId))
                {
                    string path = GetGameObjectPath(pm.gameObject);
                    idMap[path] = pm.blueprintId;
                    Debug.Log($"Backed up ID in {scenePath}: {path} -> {pm.blueprintId}");
                }
            }
            if (idMap.Count > 0)
            {
                sceneIdMap[scenePath] = idMap;
            }
        }

        // 2. 全プレハブのBlueprint IDを削除
        var prefabs = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Mushus" })
            .Select(AssetDatabase.GUIDToAssetPath)
            .ToList();

        int clearedCount = 0;
        foreach (var prefabPath in prefabs)
        {
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (go == null) continue;

            var pm = go.GetComponentInChildren<PipelineManager>(true);
            if (pm != null && !string.IsNullOrEmpty(pm.blueprintId))
            {
                // LoadPrefabContentsで編集する
                using (var editScope = new PrefabEditScope(prefabPath))
                {
                    var editPm = editScope.PrefabContents.GetComponentInChildren<PipelineManager>(true);
                    if (editPm != null)
                    {
                        editPm.blueprintId = "";
                        EditorUtility.SetDirty(editPm);
                        clearedCount++;
                        Debug.Log($"Cleared ID in Prefab: {prefabPath}");
                    }
                }
            }
        }
        Debug.Log($"Cleared Blueprint IDs from {clearedCount} prefabs.");

        // 3. DevシーンのBlueprint IDを復元（再アタッチ）
        int restoredCount = 0;
        foreach (var kvp in sceneIdMap)
        {
            string scenePath = kvp.Key;
            var idMap = kvp.Value;

            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            bool sceneModified = false;

            var pipelineManagers = Resources.FindObjectsOfTypeAll<PipelineManager>()
                .Where(p => p.gameObject.scene == scene)
                .ToList();

            foreach (var pm in pipelineManagers)
            {
                string path = GetGameObjectPath(pm.gameObject);
                if (idMap.TryGetValue(path, out string savedId))
                {
                    if (pm.blueprintId != savedId)
                    {
                        pm.blueprintId = savedId;
                        EditorUtility.SetDirty(pm);
                        
                        // Prefabインスタンスの場合、プロパティのオーバーライドとして記録する
                        if (PrefabUtility.IsPartOfPrefabInstance(pm))
                        {
                            PrefabUtility.RecordPrefabInstancePropertyModifications(pm);
                        }
                        
                        sceneModified = true;
                        restoredCount++;
                        Debug.Log($"Restored ID in {scenePath}: {path} -> {savedId}");
                    }
                }
            }

            if (sceneModified)
            {
                EditorSceneManager.SaveScene(scene);
            }
        }
        Debug.Log($"Restored Blueprint IDs to {restoredCount} objects in Dev scenes.");

        Debug.Log("Avatar Cleanup Phase 1 (Blueprint IDs) Completed.");
    }

    [MenuItem("Tools/Mushus/Fix Wrapper Prefabs")]
    public static void FixWrapperPrefabs()
    {
        string[] wrapperPaths = new[]
        {
            "Assets/Mushus/Catchy/Prefabs/AvaterPrefab.prefab",
            "Assets/Mushus/Catchy/Prefabs/AvaterQuestPrefab.prefab",
            "Assets/Mushus/Iris/Prefabs/AvaterPrefab.prefab",
            "Assets/Mushus/WhipLowPoly/Prefabs/AvaterPrefab.prefab"
        };

        foreach (var path in wrapperPaths)
        {
            if (string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(path)))
            {
                Debug.LogWarning($"Wrapper prefab not found: {path}");
                continue;
            }

            var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (go == null) continue;
            
            if (go.transform.childCount == 1)
            {
                var child = go.transform.GetChild(0);
                string newName = child.name;
                
                if (path.Contains("Quest")) newName += "Quest";

                string newPath = path.Replace("AvaterPrefab.prefab", newName + ".prefab").Replace("AvaterQuestPrefab.prefab", newName + ".prefab");
                
                GameObject wrapper = PrefabUtility.LoadPrefabContents(path);
                Transform childTransform = wrapper.transform.GetChild(0);
                
                GameObject tempObj = Instantiate(childTransform.gameObject);
                tempObj.name = newName;
                
                GameObject newPrefab = PrefabUtility.SaveAsPrefabAsset(tempObj, newPath);
                DestroyImmediate(tempObj);
                PrefabUtility.UnloadPrefabContents(wrapper);

                Debug.Log($"Created new clean prefab at {newPath}");
                
                // Devシーン上のインスタンスを置換する
                var devScenes = AssetDatabase.FindAssets("t:Scene", new[] { "Assets/Mushus" })
                    .Select(AssetDatabase.GUIDToAssetPath)
                    .Where(p => p.Contains("Dev/"))
                    .ToList();
                
                foreach (var scenePath in devScenes)
                {
                    var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                    bool sceneModified = false;
                    
                    var allObjects = Resources.FindObjectsOfTypeAll<GameObject>()
                        .Where(o => o.scene == scene)
                        .ToList();
                        
                    foreach (var obj in allObjects)
                    {
                        if (obj == null) continue;
                        if (!PrefabUtility.IsPartOfPrefabInstance(obj)) continue;
                        
                        string objPath = null;
                        try {
                            objPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(obj);
                        } catch {
                            continue;
                        }
                        
                        if (objPath == path)
                        {
                            var instanceRoot = PrefabUtility.GetOutermostPrefabInstanceRoot(obj);
                            if (instanceRoot != null && instanceRoot == obj)
                            {
                                // インスタンスを置換する
                                var newInstance = (GameObject)PrefabUtility.InstantiatePrefab(newPrefab, scene);
                                newInstance.transform.position = instanceRoot.transform.position;
                                newInstance.transform.rotation = instanceRoot.transform.rotation;
                                newInstance.transform.parent = instanceRoot.transform.parent;
                                
                                // PipelineManagerのblueprintIdをコピー
                                var oldPm = instanceRoot.GetComponentInChildren<PipelineManager>(true);
                                var newPm = newInstance.GetComponentInChildren<PipelineManager>(true);
                                if (oldPm != null && newPm != null)
                                {
                                    newPm.blueprintId = oldPm.blueprintId;
                                    PrefabUtility.RecordPrefabInstancePropertyModifications(newPm);
                                }
                                
                                DestroyImmediate(instanceRoot);
                                sceneModified = true;
                                Debug.Log($"Replaced wrapper prefab instance in {scenePath}");
                            }
                        }
                    }
                    if (sceneModified)
                    {
                        EditorSceneManager.SaveScene(scene);
                    }
                }

                // 古いものを削除
                AssetDatabase.DeleteAsset(path);
            }
        }

        Debug.Log("Wrapper Prefabs fixing completed. Please check Dev scenes for broken references.");
    }

    private static string GetGameObjectPath(GameObject obj)
    {
        string path = obj.name;
        Transform parent = obj.transform.parent;
        while (parent != null)
        {
            path = parent.name + "/" + path;
            parent = parent.parent;
        }
        return path;
    }

    private class PrefabEditScope : System.IDisposable
    {
        public GameObject PrefabContents { get; private set; }
        private string prefabPath;

        public PrefabEditScope(string path)
        {
            prefabPath = path;
            PrefabContents = PrefabUtility.LoadPrefabContents(prefabPath);
        }

        public void Dispose()
        {
            if (PrefabContents != null)
            {
                PrefabUtility.SaveAsPrefabAsset(PrefabContents, prefabPath);
                PrefabUtility.UnloadPrefabContents(PrefabContents);
            }
        }
    }
}
