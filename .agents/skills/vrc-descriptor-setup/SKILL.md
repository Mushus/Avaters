---
name: vrc-descriptor-setup
description: Attach or repair current VRChat SDK3 avatar descriptors in Unity scenes or prefabs. Use when an avatar GameObject has a missing old SDK descriptor, serialized fields like ViewPosition/lipSync but no live VRCAvatarDescriptor, a distribution prefab needs a descriptor before saving, or Codex needs to convert legacy VRC descriptor data to the current SDK component.
---

# VRC Descriptor Setup

## Workflow

Use Unity through `uloop execute-dynamic-code`; do not edit `.unity` or `.prefab` YAML by hand for descriptor repair.

1. Open the target scene or prefab in Unity.
2. Find the avatar root by name or by the FBX/prefab instance root.
3. Count missing scripts with `GameObjectUtility.GetMonoBehavioursWithMissingScriptCount`.
4. If the missing script is an old VRC descriptor, record legacy values from YAML first, especially:
   - `ViewPosition`
   - `Animations`
   - `ScaleIPD`
   - `lipSync`
   - `VisemeSkinnedMesh`, `MouthOpenBlendShapeName`, `VisemeBlendShapes`
   - `portraitCameraPositionOffset`, `portraitCameraRotationOffset`
   - expressions menu/parameters and eye look settings, if present
5. Remove only the missing script component(s) on the avatar root and children.
6. Add `VRC.SDK3.Avatars.Components.VRCAvatarDescriptor`.
7. Restore the recorded values through `SerializedObject` properties.
8. Save the scene or prefab, then run `uloop compile` and check Unity Console errors.

Prefer converting to the current SDK3 descriptor over restoring an old SDK package. A live `VRC.Core.PipelineManager` can remain attached; only remove missing scripts.

## Project Notes

In this project, the current SDK3 avatar descriptor serializes as:

```yaml
m_Script: {fileID: 542108242, guid: 67cc4cb7839cd3741b63733d5adf0442, type: 3}
```

The old missing Betty descriptor used:

```yaml
m_Script: {fileID: -1122756102, guid: f78c4655b33cb5741983dc02e08899cf, type: 3}
```

Treat that as legacy SDK data to migrate, not as a reason to reinstall the old SDK.

## Dynamic Code Pattern

Use this shape and adapt paths, object names, and legacy values. In PowerShell, `uloop --%` is often the least fragile way to pass quoted C#.

```powershell
uloop --% execute-dynamic-code --code "using UnityEngine; using UnityEditor; using UnityEditor.SceneManagement; using VRC.SDK3.Avatars.Components; using System.Linq; string scenePath=\"Assets/Mushus/BettyDev/Scenes/betty.unity\"; string prefabPath=\"Assets/Mushus/Betty/Prefabs/Betty.prefab\"; AssetDatabase.Refresh(); EditorSceneManager.OpenScene(scenePath,OpenSceneMode.Single); var roots=UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects(); var target=roots.SelectMany(r=>r.GetComponentsInChildren<Transform>(true)).Select(t=>t.gameObject).FirstOrDefault(go=>go.name==\"256fes\"||go.name==\"Betty\"); if(target==null) return \"target not found\"; int removed=0; foreach(var go in target.GetComponentsInChildren<Transform>(true).Select(t=>t.gameObject)){ removed+=GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go); } var descriptor=target.GetComponent<VRCAvatarDescriptor>(); if(descriptor==null) descriptor=target.AddComponent<VRCAvatarDescriptor>(); var so=new SerializedObject(descriptor); var view=so.FindProperty(\"ViewPosition\"); if(view!=null) view.vector3Value=new Vector3(0f,1.3f,0.2f); var anim=so.FindProperty(\"Animations\"); if(anim!=null) anim.intValue=1; var lip=so.FindProperty(\"lipSync\"); if(lip!=null) lip.intValue=0; so.ApplyModifiedPropertiesWithoutUndo(); bool prefabSuccess=false; PrefabUtility.SaveAsPrefabAssetAndConnect(target,prefabPath,InteractionMode.AutomatedAction,out prefabSuccess); EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene()); AssetDatabase.SaveAssets(); AssetDatabase.Refresh(); return \"removed=\"+removed+\"; prefabSuccess=\"+prefabSuccess;"
```

For prefab-only edits, use `PrefabUtility.EditPrefabContentsScope(prefabPath)` instead of opening a scene.

## Verification

After repair:

```powershell
uloop compile
uloop get-logs --log-type Error --max-count 50
rg -n "f78c4655b33cb5741983dc02e08899cf|VRCAvatarDescriptor|ViewPosition|lipSync" Assets/Mushus/<Package> Assets/Mushus/<Package>Dev -g "*.prefab" -g "*.unity"
```

Expected result:

- compile: `ErrorCount: 0`, `WarningCount: 0`
- console: no errors
- no legacy missing descriptor GUID remains for the repaired package
- prefab or scene contains current descriptor fields and expected `ViewPosition` / `lipSync`
