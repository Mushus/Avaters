using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;

public class AnalyzePrefabs {
    [MenuItem("Tools/Analyze Prefabs")]
    public static void Run() {
        var prefabs = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Mushus" })
            .Select(AssetDatabase.GUIDToAssetPath)
            .ToList();

        var results = new List<string>();
        foreach(var p in prefabs) {
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(p);
            if(go == null) continue;
            
            var pipelineManager = go.GetComponentInChildren<VRC.Core.PipelineManager>(true);
            string blueprintId = pipelineManager != null ? pipelineManager.blueprintId : "";
            
            bool isWrapper = false;
            if(go.name.ToLower().Contains("avatarprefab") || go.name.ToLower().Contains("avaterprefab")) {
                isWrapper = true;
            } else if (go.GetComponent<VRC.SDK3.Avatars.Components.VRCAvatarDescriptor>() == null && go.transform.childCount == 1) {
                var child = go.transform.GetChild(0);
                if(child.GetComponent<VRC.SDK3.Avatars.Components.VRCAvatarDescriptor>() != null) {
                    isWrapper = true;
                }
            }
            
            if(!string.IsNullOrEmpty(blueprintId) || isWrapper) {
                results.Add($"{p} | ID: {blueprintId} | Root: {go.name} | isWrapper: {isWrapper}");
            }
        }
        System.IO.File.WriteAllLines("prefab_analysis.txt", results);
        Debug.Log("Analysis complete");
    }
}
