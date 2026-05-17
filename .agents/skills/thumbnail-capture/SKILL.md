---
name: thumbnail-capture
description: Generate, process, and verify avatar capture images, BOOTH thumbnails, and expression tile images using the repository's Unity capture tooling and local image scripts.
---

# Thumbnail/Capture

Use this skill for avatar screenshots, BOOTH thumbnail images, marketing thumbnails, expression tiles, and local image processing.

## Repository Tools

- `Assets/Mushus/Editor/Scripts/AvatarThumbnailGenerator.cs`
- `Assets/Mushus/Editor/Scripts/AvatarCaptureSettingsEditor.cs`
- `Assets/Mushus/Editor/Scripts/Internal/CaptureExecutor.cs`
- `.agents/scripts/process_thumbnail.py`
- `scripts/tile-images.js`

## Workflow

1. Check `git status --short` and preserve unrelated user edits.
2. Inspect the target avatar's capture settings and BOOTH folder:
   - `Assets/Mushus/<AvatarName>`
   - `Products/<AvatarName>/booth`
3. Use Unity capture/generation tooling when a scene, avatar camera, or `AvatarCaptureSettings` is involved.
4. Use `.agents/scripts/process_thumbnail.py` for the repository's standardized thumbnail processing.
5. Use `scripts/tile-images.js` only for tile composition tasks that match the existing script.
6. Verify generated files exist, have expected dimensions, and are named in BOOTH display order (`1.png`, `2.png`, ...).
7. If Unity editor scripts or capture settings code changed, run `uloop compile` and inspect errors.
8. If image generation depends on PlayMode or scene state, capture a screenshot or generated output path for review.

## Completion Standard

Finish with:

- Input avatar/package and source images used.
- Generated/updated image paths.
- Dimensions and naming/order checks.
- Compile status when Unity tooling changed.
- Any images that require manual visual approval.
