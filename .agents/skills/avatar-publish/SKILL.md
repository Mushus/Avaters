---
name: avatar-publish
description: Prepare, validate, and publish VRChat avatar sample scenes for this Unity avatar repository. Use when Codex is asked to set up a sample avatar, run preflight checks, prepare or automate Windows/Android/iOS VRChat uploads, open or drive the VRChat SDK Builder, click through SDK confirmation dialogs, or repeat the Betty/Windra-style avatar publishing workflow.
---

# Avatar Publish

## Workflow

Use this skill for avatar sample setup and VRChat publish preparation in `C:\Users\wyndf\Documents\unity\Avaters`.

1. Inspect the target avatar folders under `Assets/Mushus/<AvatarName>` and `Assets/Mushus/<AvatarName>Dev`.
2. Check the worktree with `git status --short`; preserve unrelated user edits.
3. Compile with `uloop compile` before making publish decisions.
4. Prepare the sample scene with `Mushus.EditorTools.AvatarPublishPipeline` when available.
5. Treat `Assets/Mushus/<AvatarName>/Publish/<AvatarName>.publish.json` as the source of truth for name, description, thumbnail, release status, and platform scene/prefab paths.
6. Read `Products/_publish_reports/<AvatarName>-publish-report.md` and fix local preflight issues.
7. If the user asks for automation priority, run the SDK multi-platform upload and drive the SDK Builder UI with helper methods instead of stopping at manual steps.
8. Open the VRChat SDK Builder for inspection/fallback if automation stalls.
9. Keep the human in the loop only when the user has not explicitly approved automated SDK clicks, login, confirmations, or final publish actions.

## Repository Conventions

- Distribution prefab: `Assets/Mushus/<AvatarName>/Prefabs/`.
- Dev/sample scene: `Assets/Mushus/<AvatarName>Dev/Scenes/`.
- Publish reports: `Products/_publish_reports/`.
- Data as Code profile: `Assets/Mushus/<AvatarName>/Publish/<AvatarName>.publish.json`.
- Optional long-form description: `Assets/Mushus/<AvatarName>/Publish/description.md`.
- Distribution settings component: `Mushus.DistributionTools.AvatarDistributionSettings`.
- Publish pipeline: `Assets/Mushus/Editor/Scripts/AvatarPublishPipeline.cs`.
- Existing command path pattern: `Mushus/Avatar Publish/Prepare <AvatarName> Sample`.

## Data as Code Profile

Use JSON because Unity `JsonUtility` can read it without extra packages:

```json
{
  "avatar": {
    "name": "WindraLowPoly",
    "description": "Windra low-poly sample avatar.",
    "releaseStatus": "private",
    "thumbnail": "Assets/Mushus/Windra/Textures/WindraLpTex.png"
  },
  "vrchat": {
    "avatarId": "",
    "tags": []
  },
  "platforms": {
    "windows": {
      "scene": "Assets/Mushus/WindraDev/Scenes/SampleScene.unity",
      "prefab": "Assets/Mushus/Windra/Prefabs/AvaterPrefab.prefab"
    },
    "android": {
      "scene": "Assets/Mushus/WindraDev/Scenes/SampleScene.unity",
      "prefab": "Assets/Mushus/Windra/Prefabs/AvaterPrefab.prefab"
    },
    "ios": {
      "scene": "Assets/Mushus/WindraDev/Scenes/SampleScene.unity",
      "prefab": "Assets/Mushus/Windra/Prefabs/AvaterPrefab.prefab"
    }
  }
}
```

When the user asks to change publish name, image, description, or platform targets, edit the profile first, then run the prepare command and inspect the generated report.

If `vrchat.avatarId` is set, the pipeline applies it to the avatar `PipelineManager` so platform variants update the intended avatar. Leave it empty for a new SDK-created avatar.

For a new avatar without a profile, create `Assets/Mushus/<AvatarName>/Publish/<AvatarName>.publish.json` first. Point all requested platforms at the intended dev scene and distribution prefab. Use `releaseStatus: "private"` unless the user explicitly asks for public.

## Unity Commands

Prefer direct static method calls through `uloop execute-dynamic-code` when menu paths contain spaces. If the method takes a string and shell quoting becomes unreliable, add a small per-avatar wrapper to `AvatarPublishPipeline.cs` and call that wrapper.

```powershell
$code = 'Mushus.EditorTools.AvatarPublishPipeline.PrepareWindraSample(); return 1;'
$argv = @('execute-dynamic-code','--code',$code)
& uloop @argv
```

For avatars with dedicated wrappers:

```powershell
$code = 'Mushus.EditorTools.AvatarPublishPipeline.PrepareBettySample(); return 1;'
$argv = @('execute-dynamic-code','--code',$code)
& uloop @argv
```

Validate compile state:

```powershell
uloop compile --force-recompile
uloop get-logs --log-type Error --max-count 20 --include-stack-trace
```

Open the SDK Builder:

```powershell
$code = 'Mushus.EditorTools.AvatarPublishPipeline.OpenSdkBuilder(); return 1;'
$argv = @('execute-dynamic-code','--code',$code)
& uloop @argv
```

Write a local SDK metadata mirror from the profile:

```powershell
$code = 'Mushus.EditorTools.AvatarPublishPipeline.WriteWindraSdkMetadata(); return 1;'
$argv = @('execute-dynamic-code','--code',$code)
& uloop @argv
```

## Automated SDK Upload

Use this path when the user prioritizes unattended or RPA-style upload.

1. Ensure `AvatarPublishPipeline.cs` has dedicated wrapper methods for the target avatar:
   - `Prepare<AvatarName>Sample`
   - `Validate<AvatarName>Sample`
   - `Write<AvatarName>SdkMetadata`
   - `ExperimentalUpload<AvatarName>MultiPlatform`
   - `Continue<AvatarName>MultiPlatformUpload`
2. Ensure SDK UI helpers exist:
   - `ConfirmVisibleSdkOk()` clicks visible SDK `OK` confirmation dialogs.
   - `ClickVisibleSdkBuildAndPublish()` clicks visible `Build & Publish` / `Multi-Platform Build & Publish` buttons.
   - `ClickVisibleSdkFinishedClose()` closes the SDK upload result panel. It must target the lower Build panel close button, not the per-platform overrides dialog close button.
   - `DumpSdkBuilderSnapshot()` writes `Products/_publish_reports/sdk-builder-snapshot.md` with visible SDK text, buttons, status, supported platforms, last updated, version, and MPB state.
   - `PumpVisibleSdkUploadUi()` writes the same snapshot and clicks exactly one useful visible SDK control in priority order: `OK`, `Build & Publish`, then finished-panel close.
3. Compile after adding wrappers/helpers.
4. Run `Prepare<AvatarName>Sample()` and read `Products/_publish_reports/<AvatarName>-publish-report.md`.
5. Run `ExperimentalUpload<AvatarName>MultiPlatform()`.
6. Prefer state-based waiting over visual inspection:
   - Run `PumpVisibleSdkUploadUi()` every 15-30 seconds while the SDK is active.
   - Read `Products/_publish_reports/sdk-builder-snapshot.md` after each pump.
   - Treat `Status: upload-finished` plus `Supported platforms: Android, iOS, Windows` as success.
   - After success, run one more pump to close the finished panel and confirm the Build panel returns to idle.
7. For a reusable loop, run `.agents/scripts/watch_avatar_upload.ps1 -IntervalSeconds 20 -MaxMinutes 20` after starting `ExperimentalUpload<AvatarName>MultiPlatform()`.
8. Use screenshots only as fallback when the snapshot reports `SDK window not found`, `unknown`, stale `Last updated`, repeated no-click cycles, or a uloop disconnect that does not recover.

Useful commands:

```powershell
$code = 'Mushus.EditorTools.AvatarPublishPipeline.ExperimentalUploadBettyMultiPlatform(); return 1;'
$argv = @('execute-dynamic-code','--code',$code,'--yield-to-foreground-requests')
& uloop @argv

$code = 'Mushus.EditorTools.AvatarPublishPipeline.ConfirmVisibleSdkOk(); return 1;'
$argv = @('execute-dynamic-code','--code',$code)
& uloop @argv

$code = 'Mushus.EditorTools.AvatarPublishPipeline.ClickVisibleSdkBuildAndPublish(); return 1;'
$argv = @('execute-dynamic-code','--code',$code)
& uloop @argv

$code = 'Mushus.EditorTools.AvatarPublishPipeline.ClickVisibleSdkFinishedClose(); return 1;'
$argv = @('execute-dynamic-code','--code',$code)
& uloop @argv

$code = 'Mushus.EditorTools.AvatarPublishPipeline.DumpSdkBuilderSnapshot(); return 1;'
$argv = @('execute-dynamic-code','--code',$code)
& uloop @argv

$code = 'Mushus.EditorTools.AvatarPublishPipeline.PumpVisibleSdkUploadUi(); return 1;'
$argv = @('execute-dynamic-code','--code',$code)
& uloop @argv

.agents/scripts/watch_avatar_upload.ps1 -IntervalSeconds 20 -MaxMinutes 20
```

Do not rely only on logs during SDK upload. The VRChat SDK often waits on UI state with no fresh console log. Prefer `sdk-builder-snapshot.md` for `Refreshing data`, `Uploading File`, `Avatar Built`, `Multi-Platform Upload Finished`, Supported Platforms, and Last Updated. Use screenshots only when the snapshot cannot classify the SDK state.

## Platform Rules

Read `references/platform-publish.md` when working on Windows/Android/iOS details.

Always confirm:

- Windows, Android, and iOS build support are installed.
- Mobile targets use VRChat mobile-compatible shaders.
- Expression Parameters and Expressions Menu are assigned, even if empty.
- The profile thumbnail resolves to a Unity `Texture2D`.
- Existing avatar IDs start with `avtr_` when present.
- The sample scene has exactly the intended avatar descriptor.
- The same avatar ID should be used for platform variants unless the user explicitly wants separate avatars.

## Completion Standard

Finish with:

- The prepared scene path.
- The target prefab path.
- The report path and issue count.
- Compile status.
- Upload status and final Supported Platforms text when automation was requested.
- Whether VRChat SDK Builder was opened.
- Any failed or unavailable verification steps.
