---
name: distribution-package
description: Prepare and verify Products/<AvatarName> distribution deliverables: README, unitypackage, zip, src files, BOOTH images, and distribution metadata.
---

# Distribution Package

Use this skill when preparing avatar distribution files under `Products/<AvatarName>` or generating package artifacts from Unity.

## Source Standards

Read:

- `docs/distribution-standard.md`
- `docs/directory_structure.md`
- `docs/avatar-development-standard.md` when Unity assets are involved

## Expected Layout

```text
Products/<AvatarName>/
  <AvatarName>.zip
  <AvatarName>/
    README.txt
    <AvatarName>.unitypackage
    src/
  booth/
    1.png
    2.png
    Assets/
```

## Workflow

1. Check `git status --short` and preserve unrelated user edits.
2. Inspect `Products/<AvatarName>` and `Assets/Mushus/<AvatarName>`.
3. Confirm the distribution prefab exists under `Assets/Mushus/<AvatarName>/Prefabs/`.
4. Confirm `README.txt` includes avatar name, version/history, terms or terms URL, contents, and contact.
5. Generate or verify `.unitypackage` through the repository's Unity editor tooling when available:
   - `Mushus.DistributionTools.AvatarDistributionSettings`
   - `AvatarDistributionSettingsEditor`
6. Create or verify `<AvatarName>.zip` by compressing the inner `Products/<AvatarName>/<AvatarName>/` folder so extraction creates a single avatar folder.
7. Verify `src/` contains user-editable source data when expected.
8. Verify `booth/*.png` files are numbered in display order.
9. If Unity/editor scripts changed, run the standard compile/test flow.
10. Do not invent product files. Stop and report missing required inputs.

## Completion Standard

Finish with:

- Distribution folder audited/prepared.
- README, unitypackage, zip, src, and booth image status.
- Created/updated artifact paths.
- Any missing inputs or unresolved packaging risks.
- Compile/test status when Unity tooling or code changed.
