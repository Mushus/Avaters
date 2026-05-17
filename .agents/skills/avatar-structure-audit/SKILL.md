---
name: avatar-structure-audit
description: Audit or organize avatar package folders against this repository's Assets/Mushus development standard, including package/dev separation, prefab placement, references, materials, textures, and Products links.
---

# Avatar Structure Audit

Use this skill when reviewing, restoring, or organizing an avatar under `Assets/Mushus`.

## Source Standards

Read these before making decisions:

- `docs/directory_structure.md`
- `docs/avatar-development-standard.md`
- `docs/distribution-standard.md` when `Products/` is involved

## Workflow

1. Check `git status --short` and preserve unrelated user edits.
2. Pick exactly one avatar/package at a time unless the user explicitly asks for a batch audit.
3. Inspect:
   - `Assets/Mushus/<AvatarName>/`
   - `Assets/Mushus/<AvatarName>Dev/`
   - `Products/<AvatarName>/` when present
4. Confirm the standard package layout:
   - `Animations/`
   - `Controllers/`
   - `Expressions/`
   - `Materials/`
   - `Models/`
   - `Prefabs/`
   - `Scenes/`
   - `Scripts/`
   - `Shaders/`
   - `Textures/`
5. Confirm the Dev side contains sample/upload scenes, upload variants, and AAC/development assets only.
6. Verify distribution prefabs are under `Assets/Mushus/<AvatarName>/Prefabs/`.
7. Verify distribution prefabs do not reference `<AvatarName>Dev`.
8. Check for obvious missing scripts, missing materials, missing prefabs, old `Quest` naming that should remain stable, and `LP`/`LowPoly` products that should remain separate.
9. Move or rename assets only through Unity Editor APIs or together with their `.meta` files. Prefer Unity operations for reference safety.
10. After changes, run `uloop compile` and inspect errors. Run tests when code or editor automation changed.

## Completion Standard

Finish with:

- Avatar/package audited.
- Current package/dev/product paths.
- Issues found, grouped as fixed vs remaining.
- Any moved/created files.
- Compile/test status when changes were made.
