# VRChat Multi-Platform Publish Notes

VRChat supports avatar uploads for Windows, Android, and iOS through the SDK. Windows is usually the full-fidelity target. Android and iOS should be treated as mobile targets and checked for shader, texture, and performance compatibility.

## Practical Sequence

1. Prepare the sample scene and descriptor.
2. Validate compile and preflight report.
3. Publish or update the Windows build.
4. Switch/upload Android using the mobile-compatible avatar or per-platform override.
5. Switch/upload iOS using the mobile-compatible avatar or per-platform override.
6. Verify the SDK shows the uploads under the intended avatar ID.

## Human Confirmation Gates

Do not silently click through final VRChat publish operations. Ask the user to confirm or take over when SDK login, Terms, visibility, content warnings, thumbnail confirmation, or final publish buttons are shown.

## Common Failure Points

- Build support module missing for Android or iOS.
- Non-mobile shader on Android/iOS.
- Descriptor lacks expression assets.
- Upload goes to the wrong platform after switching targets.
- A platform variant accidentally receives a different avatar ID.
- SDK control panel is unavailable because the project has compile errors.
