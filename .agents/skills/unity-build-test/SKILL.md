---
name: unity-build-test
description: Run the repository standard verification loop after code or Unity asset automation changes: compile, inspect compiler errors, run Unity tests, inspect failures, and report actionable results.
---

# Unity Build/Test

Use this skill in `C:\Users\wyndf\Documents\unity\Avaters` whenever code changes, editor automation changes, or prefab/scene operations need repository-standard verification.

## Workflow

1. Check `git status --short` and preserve unrelated user edits.
2. Compile the Unity project:

```powershell
uloop compile --force-recompile
```

3. Inspect compiler errors:

```powershell
uloop get-logs --log-type Error --max-count 50 --include-stack-trace
```

4. If compiler errors are present, identify the files and exact messages, then fix the relevant code before continuing.
5. Run Unity tests. Prefer EditMode first unless the task specifically changes PlayMode behavior:

```powershell
uloop run-tests --test-mode EditMode
```

6. If tests fail and `XmlPath` is returned, read the NUnit XML and summarize failing test names, messages, and stack traces.
7. For PlayMode/gameplay/UI behavior, run PlayMode tests as a separate single-flight run:

```powershell
uloop run-tests --test-mode PlayMode
```

Do not run multiple `uloop run-tests` commands at the same time.

## Completion Standard

Finish with:

- Compile success/failure and error/warning counts.
- Whether Unity Console has compiler errors.
- Test mode(s), pass/fail counts, and failing test names if any.
- Any verification that could not be completed and why.
