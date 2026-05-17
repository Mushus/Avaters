using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using Mushus.DistributionTools;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;
using VRC.Core;
using VRC.SDKBase;
using VRC.SDK3A.Editor;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using VRC.SDKBase.Editor;
using VRC.SDKBase.Editor.Api;

namespace Mushus.EditorTools
{
    public static class AvatarPublishPipeline
    {
        private const string DefaultAvatarName = "Windra";
        private const string RedDragonAvatarName = "RedDragon";
        private const string BettyAvatarName = "Betty";
        private const string WhipLowPolyAvatarName = "WhipLowPoly";
        private const string ReportDirectory = "Products/_publish_reports";
        private const string PublishFolderName = "Publish";

        [MenuItem("Mushus/Avatar Publish/Prepare Windra Sample")]
        public static void PrepareWindraSample()
        {
            PrepareSample(DefaultAvatarName);
        }

        [MenuItem("Mushus/Avatar Publish/Prepare RedDragon Sample")]
        public static void PrepareRedDragonSample()
        {
            PrepareSample(RedDragonAvatarName);
        }

        [MenuItem("Mushus/Avatar Publish/Prepare Betty Sample")]
        public static void PrepareBettySample()
        {
            PrepareSample(BettyAvatarName);
        }

        [MenuItem("Mushus/Avatar Publish/Prepare WhipLowPoly Sample")]
        public static void PrepareWhipLowPolySample()
        {
            PrepareSample(WhipLowPolyAvatarName);
        }

        [MenuItem("Mushus/Avatar Publish/Validate Windra Sample")]
        public static void ValidateWindraSample()
        {
            ValidateSample(DefaultAvatarName, true);
        }

        [MenuItem("Mushus/Avatar Publish/Validate RedDragon Sample")]
        public static void ValidateRedDragonSample()
        {
            ValidateSample(RedDragonAvatarName, true);
        }

        [MenuItem("Mushus/Avatar Publish/Validate Betty Sample")]
        public static void ValidateBettySample()
        {
            ValidateSample(BettyAvatarName, true);
        }

        [MenuItem("Mushus/Avatar Publish/Validate WhipLowPoly Sample")]
        public static void ValidateWhipLowPolySample()
        {
            ValidateSample(WhipLowPolyAvatarName, true);
        }

        [MenuItem("Mushus/Avatar Publish/Write Windra SDK Metadata")]
        public static void WriteWindraSdkMetadata()
        {
            WriteSdkMetadata(DefaultAvatarName);
        }

        [MenuItem("Mushus/Avatar Publish/Write RedDragon SDK Metadata")]
        public static void WriteRedDragonSdkMetadata()
        {
            WriteSdkMetadata(RedDragonAvatarName);
        }

        [MenuItem("Mushus/Avatar Publish/Write Betty SDK Metadata")]
        public static void WriteBettySdkMetadata()
        {
            WriteSdkMetadata(BettyAvatarName);
        }

        [MenuItem("Mushus/Avatar Publish/Write WhipLowPoly SDK Metadata")]
        public static void WriteWhipLowPolySdkMetadata()
        {
            WriteSdkMetadata(WhipLowPolyAvatarName);
        }

        [MenuItem("Mushus/Avatar Publish/Experimental Upload Windra Multi-Platform")]
        public static async void ExperimentalUploadWindraMultiPlatform()
        {
            await ExperimentalUploadMultiPlatform(DefaultAvatarName);
        }

        [MenuItem("Mushus/Avatar Publish/Continue Windra Multi-Platform Upload")]
        public static async void ContinueWindraMultiPlatformUpload()
        {
            await ExperimentalUploadMultiPlatform(DefaultAvatarName, false);
        }

        [MenuItem("Mushus/Avatar Publish/Experimental Upload RedDragon Multi-Platform")]
        public static async void ExperimentalUploadRedDragonMultiPlatform()
        {
            await ExperimentalUploadMultiPlatform(RedDragonAvatarName);
        }

        [MenuItem("Mushus/Avatar Publish/Continue RedDragon Multi-Platform Upload")]
        public static async void ContinueRedDragonMultiPlatformUpload()
        {
            await ExperimentalUploadMultiPlatform(RedDragonAvatarName, false);
        }

        [MenuItem("Mushus/Avatar Publish/Experimental Upload Betty Multi-Platform")]
        public static async void ExperimentalUploadBettyMultiPlatform()
        {
            await ExperimentalUploadMultiPlatform(BettyAvatarName);
        }

        [MenuItem("Mushus/Avatar Publish/Continue Betty Multi-Platform Upload")]
        public static async void ContinueBettyMultiPlatformUpload()
        {
            await ExperimentalUploadMultiPlatform(BettyAvatarName, false);
        }

        [MenuItem("Mushus/Avatar Publish/Experimental Upload WhipLowPoly Multi-Platform")]
        public static async void ExperimentalUploadWhipLowPolyMultiPlatform()
        {
            await ExperimentalUploadMultiPlatform(WhipLowPolyAvatarName);
        }

        [MenuItem("Mushus/Avatar Publish/Continue WhipLowPoly Multi-Platform Upload")]
        public static async void ContinueWhipLowPolyMultiPlatformUpload()
        {
            await ExperimentalUploadMultiPlatform(WhipLowPolyAvatarName, false);
        }

        [MenuItem("Mushus/Avatar Publish/Open VRChat SDK Builder")]
        public static void OpenSdkBuilder()
        {
            if (!EditorApplication.ExecuteMenuItem("VRChat SDK/Show Control Panel"))
            {
                EditorUtility.DisplayDialog(
                    "VRChat SDK",
                    "Could not open the VRChat SDK control panel. Confirm that the VRChat SDK is installed and compiled.",
                    "OK");
            }
        }

        [MenuItem("Mushus/Avatar Publish/Confirm Visible SDK OK")]
        public static void ConfirmVisibleSdkOk()
        {
            ClickVisibleSdkButton(button => button.text == "OK");
        }

        [MenuItem("Mushus/Avatar Publish/Click Visible SDK Build And Publish")]
        public static void ClickVisibleSdkBuildAndPublish()
        {
            ClickVisibleSdkButton(button => button.text.Contains("Build") && button.text.Contains("Publish"));
        }

        [MenuItem("Mushus/Avatar Publish/Click Visible SDK Finished Close")]
        public static void ClickVisibleSdkFinishedClose()
        {
            ClickVisibleSdkFinishedCloseButton();
        }

        [MenuItem("Mushus/Avatar Publish/Pump Visible SDK Upload UI")]
        public static void PumpVisibleSdkUploadUi()
        {
            var snapshot = CaptureSdkBuilderSnapshot();
            WriteSdkBuilderSnapshot(snapshot);

            var clicked =
                ClickVisibleSdkButton(button => button.text == "OK") ||
                ClickVisibleSdkButton(button => button.text.Contains("Build") && button.text.Contains("Publish")) ||
                ClickVisibleSdkFinishedCloseButton();

            Debug.Log($"SDK upload UI pump: clicked={clicked}; status={snapshot.Status}; platforms={snapshot.SupportedPlatforms}; updated={snapshot.LastUpdated}");
        }

        [MenuItem("Mushus/Avatar Publish/Dump Multi-Platform Build State")]
        public static void DumpMultiPlatformBuildState()
        {
            Debug.Log($"VRChat SDK MPB state: {GetMultiPlatformBuildState()}");
        }

        [MenuItem("Mushus/Avatar Publish/Dump SDK Builder Snapshot")]
        public static void DumpSdkBuilderSnapshot()
        {
            var snapshot = CaptureSdkBuilderSnapshot();
            var path = WriteSdkBuilderSnapshot(snapshot);
            Debug.Log($"SDK Builder snapshot: {path}; status={snapshot.Status}; platforms={snapshot.SupportedPlatforms}; updated={snapshot.LastUpdated}");
        }

        [MenuItem("Mushus/Avatar Publish/Dump VRChat SDK Publish API")]
        public static void DumpVrChatSdkPublishApi()
        {
            Directory.CreateDirectory(ReportDirectory);
            var path = $"{ReportDirectory}/vrchat-sdk-publish-api.txt";
            var keywords = new[] { "Build", "Publish", "Upload", "Avatar", "Thumbnail", "Blueprint", "Pipeline" };
            var lines = AppDomain.CurrentDomain.GetAssemblies()
                .Where(assembly => assembly.GetName().Name.Contains("VRC") || assembly.GetName().Name.Contains("VRChat"))
                .SelectMany(GetTypesSafe)
                .Where(type => type.FullName != null && keywords.Any(keyword => type.FullName.Contains(keyword)))
                .OrderBy(type => type.FullName)
                .SelectMany(TypeSummary)
                .ToList();

            File.WriteAllLines(path, lines);
            AssetDatabase.Refresh();
            Debug.Log($"VRChat SDK publish API dump: {path}");
        }

        [MenuItem("Mushus/Avatar Publish/Dump VRChat SDK Publish Signatures")]
        public static void DumpVrChatSdkPublishSignatures()
        {
            Directory.CreateDirectory(ReportDirectory);
            var path = $"{ReportDirectory}/vrchat-sdk-publish-signatures.txt";
            var targetTypeNames = new[]
            {
                "VRC.SDK3A.Editor.IVRCSdkAvatarBuilderApi",
                "VRC.SDK3A.Editor.VRCSdkControlPanelAvatarBuilder",
                "VRC.SDKBase.Editor.Api.VRCApi",
                "VRC.SDKBase.Editor.Api.VRCAvatar",
                "VRC.Core.ApiAvatar",
                "VRC.Core.PipelineManager",
                "VRC.SDKBase.Editor.VRC_SdkBuilder"
            };

            var allTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(GetTypesSafe).ToList();
            var lines = new List<string>();
            foreach (var typeName in targetTypeNames)
            {
                var type = allTypes.FirstOrDefault(candidate => candidate.FullName == typeName);
                if (type == null)
                {
                    lines.Add($"MISSING {typeName}");
                    continue;
                }

                lines.Add($"TYPE {type.Assembly.GetName().Name} {type.FullName}");
                foreach (var constructor in type.GetConstructors(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance))
                {
                    lines.Add($"  CTOR {FormatParameters(constructor.GetParameters())}");
                }

                foreach (var method in type.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly)
                             .OrderBy(method => method.Name))
                {
                    lines.Add($"  METHOD {(method.IsPublic ? "public" : "nonpublic")} {(method.IsStatic ? "static" : "instance")} {method.ReturnType.FullName} {method.Name}({FormatParameters(method.GetParameters())})");
                }

                foreach (var property in type.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly)
                             .OrderBy(property => property.Name))
                {
                    lines.Add($"  PROPERTY {property.PropertyType.FullName} {property.Name}");
                }

                foreach (var field in type.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly)
                             .OrderBy(field => field.Name))
                {
                    lines.Add($"  FIELD {(field.IsPublic ? "public" : "nonpublic")} {(field.IsStatic ? "static" : "instance")} {field.FieldType.FullName} {field.Name}");
                }

                lines.Add("");
            }

            File.WriteAllLines(path, lines);
            AssetDatabase.Refresh();
            Debug.Log($"VRChat SDK publish signatures dump: {path}");
        }

        public static void PrepareSample(string avatarName)
        {
            var profile = LoadProfile(avatarName);
            var scenePath = GetSampleScenePath(avatarName, profile);
            var prefabPath = GetPrefabPath(avatarName, profile);

            if (string.IsNullOrEmpty(scenePath))
            {
                Debug.LogError($"Sample scene not found for {avatarName}.");
                return;
            }

            if (string.IsNullOrEmpty(prefabPath))
            {
                Debug.LogError($"Prefab not found for {avatarName}.");
                return;
            }

            var scene = EditorSceneManager.OpenScene(scenePath);
            var descriptor = UnityEngine.Object.FindObjectsByType<VRCAvatarDescriptor>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None)
                .FirstOrDefault();

            if (descriptor == null)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                var instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                if (instance == null)
                {
                    Debug.LogError($"Failed to instantiate prefab: {prefabPath}");
                    return;
                }

                descriptor = instance.GetComponent<VRCAvatarDescriptor>();
            }

            if (descriptor == null)
            {
                Debug.LogError($"VRCAvatarDescriptor not found on prefab: {prefabPath}");
                return;
            }

            if (descriptor.transform.parent != null)
            {
                var prefabRoot = PrefabUtility.GetOutermostPrefabInstanceRoot(descriptor.gameObject);
                if (prefabRoot != null)
                {
                    PrefabUtility.UnpackPrefabInstance(prefabRoot, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                }

                descriptor.transform.SetParent(null, true);
            }

            ApplyProfileToPipelineManager(descriptor.gameObject, profile);
            EnsureExpressions(avatarName, descriptor);
            EnsureDistributionSettings(descriptor.gameObject);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            var reportPath = WriteReport(avatarName, descriptor, profile);
            Debug.Log($"Prepared {avatarName} sample scene: {scenePath}");
            Debug.Log($"Publish report: {reportPath}");
        }

        public static void WriteSdkMetadata(string avatarName)
        {
            var profile = LoadProfile(avatarName);
            Directory.CreateDirectory(ReportDirectory);
            var metadata = CreateSdkAvatarMetadata(profile);
            var path = $"{ReportDirectory}/{avatarName}-sdk-metadata.md";
            var lines = new[]
            {
                $"# {avatarName} SDK Metadata",
                "",
                $"ID: `{metadata.ID}`",
                $"Name: `{metadata.Name}`",
                $"Description: {metadata.Description}",
                $"ReleaseStatus: `{metadata.ReleaseStatus}`",
                $"Thumbnail path: `{profile.avatar.thumbnail}`",
                "",
                "This file mirrors the VRCAvatar metadata object that the SDK BuildAndUpload API expects."
            };

            File.WriteAllLines(path, lines);
            AssetDatabase.Refresh();
            Debug.Log($"SDK metadata written: {path}");
        }

        public static async System.Threading.Tasks.Task ExperimentalUploadMultiPlatform(string avatarName, bool resetMultiPlatformState = true)
        {
            PrepareSample(avatarName);

            var profile = LoadProfile(avatarName);
            var scenePath = GetSampleScenePath(avatarName, profile);
            EditorSceneManager.OpenScene(scenePath);

            var descriptor = UnityEngine.Object.FindObjectsByType<VRCAvatarDescriptor>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None)
                .FirstOrDefault();

            if (descriptor == null)
            {
                Debug.LogError($"VRCAvatarDescriptor not found in scene: {scenePath}");
                return;
            }

            var metadata = CreateSdkAvatarMetadata(profile);
            var pipelineManager = descriptor.GetComponent<PipelineManager>();
            if (string.IsNullOrWhiteSpace(metadata.ID) && pipelineManager != null && !string.IsNullOrWhiteSpace(pipelineManager.blueprintId))
            {
                metadata.ID = pipelineManager.blueprintId;
                Debug.Log($"Using existing PipelineManager blueprint ID for {avatarName}: {metadata.ID}");
            }

            var thumbnailPath = string.IsNullOrWhiteSpace(profile.avatar.thumbnail)
                ? null
                : Path.GetFullPath(profile.avatar.thumbnail);

            Debug.Log($"Starting experimental multi-platform upload for {avatarName}. Avatar ID: {metadata.ID}. Thumbnail: {thumbnailPath}");

            if (resetMultiPlatformState)
            {
                ClearMultiPlatformBuildState();
            }

            SetMultiPlatformContentIdentifier(descriptor.gameObject);
            PrepareSdkSessionState(profile, thumbnailPath);
            EditorApplication.ExecuteMenuItem("VRChat SDK/Show Control Panel");
            await System.Threading.Tasks.Task.Delay(1500);

            if (!VRCSdkControlPanel.TryGetBuilder<IVRCSdkAvatarBuilderApi>(out var builder))
            {
                Debug.LogError("Could not get initialized VRChat avatar builder from the SDK Control Panel.");
                return;
            }

            await builder.BuildAndUploadMultiPlatform(descriptor.gameObject, metadata, thumbnailPath, CancellationToken.None);

            Debug.Log($"Experimental multi-platform upload finished for {avatarName}.");
        }

        public static void ValidateSample(string avatarName, bool showDialog)
        {
            var profile = LoadProfile(avatarName);
            var scenePath = GetSampleScenePath(avatarName, profile);
            if (string.IsNullOrEmpty(scenePath))
            {
                Debug.LogError($"Sample scene not found for {avatarName}.");
                return;
            }

            EditorSceneManager.OpenScene(scenePath);
            var descriptor = UnityEngine.Object.FindObjectsByType<VRCAvatarDescriptor>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None)
                .FirstOrDefault();

            if (descriptor == null)
            {
                Debug.LogError($"VRCAvatarDescriptor not found in scene: {scenePath}");
                return;
            }

            var reportPath = WriteReport(avatarName, descriptor, profile);
            var issues = CollectIssues(avatarName, descriptor, profile);
            foreach (var issue in issues)
            {
                Debug.LogWarning(issue);
            }

            Debug.Log($"Validated {avatarName}. Issues: {issues.Count}. Report: {reportPath}");

            if (showDialog)
            {
                EditorUtility.DisplayDialog(
                    "Avatar Publish Validation",
                    issues.Count == 0
                        ? $"Validation passed.\n\nReport:\n{reportPath}"
                        : $"Validation completed with {issues.Count} issue(s). Check Console and report.\n\nReport:\n{reportPath}",
                    "OK");
            }
        }

        private static void EnsureDistributionSettings(GameObject avatarRoot)
        {
            var settings = UnityEngine.Object.FindFirstObjectByType<AvatarDistributionSettings>();
            if (settings == null)
            {
                var holder = new GameObject("DistributionSettings");
                settings = holder.AddComponent<AvatarDistributionSettings>();
            }

            settings.TargetAvatar = avatarRoot;
            settings.CurrentSpec = AvatarSpecCollector.Collect(avatarRoot);
            EditorUtility.SetDirty(settings);
        }

        private static void ApplyProfileToPipelineManager(GameObject avatarRoot, PublishProfile profile)
        {
            if (string.IsNullOrWhiteSpace(profile.vrchat.avatarId))
            {
                return;
            }

            var pipelineManager = avatarRoot.GetComponent<PipelineManager>();
            if (pipelineManager == null)
            {
                pipelineManager = avatarRoot.AddComponent<PipelineManager>();
            }

            pipelineManager.blueprintId = profile.vrchat.avatarId;
            EditorUtility.SetDirty(pipelineManager);
        }

        private static VRCAvatar CreateSdkAvatarMetadata(PublishProfile profile)
        {
            return new VRCAvatar
            {
                ID = profile.vrchat.avatarId,
                Name = profile.avatar.name,
                Description = profile.avatar.description,
                ReleaseStatus = profile.avatar.releaseStatus,
                ThumbnailImageUrl = profile.avatar.thumbnail,
                Tags = profile.vrchat.tags ?? new List<string>()
            };
        }

        private static void PrepareSdkSessionState(PublishProfile profile, string thumbnailPath)
        {
            AvatarBuilderSessionState.AvatarName = profile.avatar.name;
            AvatarBuilderSessionState.AvatarDesc = profile.avatar.description;
            AvatarBuilderSessionState.AvatarReleaseStatus = profile.avatar.releaseStatus;
            AvatarBuilderSessionState.AvatarThumbPath = thumbnailPath;
            AvatarBuilderSessionState.AvatarTags = string.Join("|", profile.vrchat.tags ?? new List<string>());
            AvatarBuilderSessionState.AvatarPlatforms = new List<BuildTarget>
            {
                BuildTarget.StandaloneWindows64,
                BuildTarget.Android,
                BuildTarget.iOS
            };
            VRCSettings.SDKAvatarBuildType = "BuildAndPublish";
            VRCSettings.ActiveWindowPanel = 1;
        }

        private static void ClearMultiPlatformBuildState()
        {
            var mpbType = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(GetTypesSafe)
                .FirstOrDefault(type => type.FullName == "VRC.SDKBase.VRCMultiPlatformBuild");
            var clearMethod = mpbType?.GetMethod(
                "ClearMPBState",
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            clearMethod?.Invoke(null, null);

            var flags = System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic;
            var contentProperty = mpbType?.GetProperty("MPBContentIdentifier", flags);
            if (contentProperty?.CanWrite == true)
            {
                contentProperty.SetValue(null, string.Empty);
            }

            var contentField = mpbType?.GetField("MPBContentIdentifier", flags);
            contentField?.SetValue(null, string.Empty);
        }

        private static string GetMultiPlatformBuildState()
        {
            var mpbType = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(GetTypesSafe)
                .FirstOrDefault(type => type.FullName == "VRC.SDKBase.VRCMultiPlatformBuild");
            if (mpbType == null)
            {
                return "VRCMultiPlatformBuild type not found";
            }

            var flags = System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic;
            var names = new[]
            {
                "MPB",
                "MPBContentIdentifier",
                "MPBState",
                "MPBBuiltCount",
                "MPBNextPlatform",
                "MPBInitialPlatform",
                "MPBPlatformsList"
            };

            return string.Join("; ", names.Select(name =>
            {
                var value = mpbType.GetProperty(name, flags)?.GetValue(null);
                return $"{name}={FormatStateValue(value)}";
            }));
        }

        private static string FormatStateValue(object value)
        {
            if (value == null)
            {
                return "<null>";
            }

            if (value is System.Collections.IEnumerable enumerable && value is not string)
            {
                return string.Join(",", enumerable.Cast<object>());
            }

            return value.ToString();
        }

        private static SdkBuilderSnapshot CaptureSdkBuilderSnapshot()
        {
            var window = VRCSdkControlPanel.window ?? EditorWindow.focusedWindow;
            var snapshot = new SdkBuilderSnapshot
            {
                CapturedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                WindowFound = window != null,
                MultiPlatformBuildState = GetMultiPlatformBuildState()
            };

            if (window == null)
            {
                snapshot.Status = "SDK window not found";
                return snapshot;
            }

            var root = window.rootVisualElement;
            snapshot.VisibleTexts = root.Query<VisualElement>()
                .ToList()
                .Where(IsVisible)
                .Select(GetElementText)
                .Where(text => !string.IsNullOrWhiteSpace(text))
                .Select(text => text.Trim())
                .Distinct()
                .Take(200)
                .ToList();

            snapshot.VisibleButtons = root.Query<Button>()
                .ToList()
                .Where(IsVisible)
                .Select(button => string.IsNullOrWhiteSpace(button.text) ? $"<{button.name}>" : button.text.Trim())
                .Where(text => !string.IsNullOrWhiteSpace(text))
                .Distinct()
                .ToList();

            snapshot.Status = InferSdkBuilderStatus(snapshot.VisibleTexts);
            snapshot.SupportedPlatforms = FindValueAfterLabel(snapshot.VisibleTexts, "Supported Platforms");
            snapshot.LastUpdated = FindValueAfterLabel(snapshot.VisibleTexts, "Last Updated");
            snapshot.Version = FindValueAfterLabel(snapshot.VisibleTexts, "Version");
            return snapshot;
        }

        private static string WriteSdkBuilderSnapshot(SdkBuilderSnapshot snapshot)
        {
            Directory.CreateDirectory(ReportDirectory);
            var path = $"{ReportDirectory}/sdk-builder-snapshot.md";
            var lines = new List<string>
            {
                "# VRChat SDK Builder Snapshot",
                "",
                $"Captured at: `{snapshot.CapturedAt}`",
                $"Window found: `{snapshot.WindowFound}`",
                $"Status: `{snapshot.Status}`",
                $"Supported platforms: `{snapshot.SupportedPlatforms}`",
                $"Last updated: `{snapshot.LastUpdated}`",
                $"Version: `{snapshot.Version}`",
                $"MPB state: `{snapshot.MultiPlatformBuildState}`",
                "",
                "## Visible Buttons",
                ""
            };

            lines.AddRange(snapshot.VisibleButtons.Select(button => "- " + button));
            lines.Add("");
            lines.Add("## Visible Text");
            lines.Add("");
            lines.AddRange(snapshot.VisibleTexts.Select(text => "- " + text.Replace("\n", "\\n")));
            File.WriteAllLines(path, lines);
            AssetDatabase.Refresh();
            return path;
        }

        private static string InferSdkBuilderStatus(List<string> visibleTexts)
        {
            if (visibleTexts.Any(text => text.Contains("Multi-Platform Upload Finished")))
            {
                return "upload-finished";
            }

            if (visibleTexts.Any(text => text.Contains("Uploading File")))
            {
                return "uploading-file";
            }

            if (visibleTexts.Any(text => text.Contains("Avatar Built")))
            {
                return "avatar-built";
            }

            if (visibleTexts.Any(text => text.Contains("Refreshing data")))
            {
                return "refreshing-data";
            }

            if (visibleTexts.Any(text => text.Contains("Build") && text.Contains("Publish")))
            {
                return "ready-or-waiting-for-build";
            }

            return "unknown";
        }

        private static string FindValueAfterLabel(List<string> visibleTexts, string label)
        {
            var index = visibleTexts.FindIndex(text => text == label || text.StartsWith(label + "\n"));
            if (index < 0)
            {
                return "";
            }

            var sameText = visibleTexts[index];
            if (sameText.StartsWith(label + "\n"))
            {
                return sameText.Substring(label.Length).Trim();
            }

            return index + 1 < visibleTexts.Count ? visibleTexts[index + 1] : "";
        }

        private static string GetElementText(VisualElement element)
        {
            if (element is TextElement textElement)
            {
                return textElement.text;
            }

            return "";
        }

        private static bool IsVisible(VisualElement element)
        {
            return element.resolvedStyle.display != DisplayStyle.None &&
                   element.resolvedStyle.visibility == Visibility.Visible &&
                   element.worldBound.width > 0f &&
                   element.worldBound.height > 0f;
        }

        private static void SetMultiPlatformContentIdentifier(GameObject avatarRoot)
        {
            var transform = avatarRoot.transform;
            var hierarchyPath = transform.name + $"[{transform.GetSiblingIndex()}]";
            while (transform.parent != null)
            {
                transform = transform.parent;
                hierarchyPath = transform.name + "/*/" + hierarchyPath;
            }

            var mpbType = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(GetTypesSafe)
                .FirstOrDefault(type => type.FullName == "VRC.SDKBase.VRCMultiPlatformBuild");
            var property = mpbType?.GetProperty(
                "MPBContentIdentifier",
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            property?.SetValue(null, hierarchyPath);
            Debug.Log($"Prepared VRChat SDK multi-platform content identifier: {hierarchyPath}");
        }

        private static bool ClickVisibleSdkButton(Func<Button, bool> predicate)
        {
            var window = EditorWindow.focusedWindow ?? VRCSdkControlPanel.window;
            if (window == null)
            {
                Debug.LogWarning("No focused EditorWindow or VRChat SDK window found.");
                return false;
            }

            var button = window.rootVisualElement.Query<Button>()
                .ToList()
                .FirstOrDefault(candidate =>
                    !string.IsNullOrWhiteSpace(candidate.text) &&
                    candidate.resolvedStyle.display != DisplayStyle.None &&
                    candidate.enabledInHierarchy &&
                    predicate(candidate));

            if (button == null)
            {
                Debug.LogWarning("No matching visible SDK button found.");
                return false;
            }

            var invoke = button.clickable?.GetType().GetMethod(
                "Invoke",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
            invoke?.Invoke(button.clickable, new object[] { null });
            Debug.Log($"Clicked visible SDK button: {button.text}");
            return true;
        }

        private static bool ClickVisibleSdkFinishedCloseButton()
        {
            var window = EditorWindow.focusedWindow ?? VRCSdkControlPanel.window;
            if (window == null)
            {
                Debug.LogWarning("No focused EditorWindow or VRChat SDK window found.");
                return false;
            }

            var buttons = window.rootVisualElement.Query<Button>()
                .ToList()
                .Where(button => button.resolvedStyle.display != DisplayStyle.None && button.enabledInHierarchy)
                .ToList();
            foreach (var button in buttons)
            {
                Debug.Log($"Visible SDK button: text='{button.text}' name='{button.name}' classes='{string.Join(",", button.GetClasses())}'");
            }

            var closeButton = buttons
                .Where(button => string.IsNullOrWhiteSpace(button.text) || button.text.Trim() == "x" || button.text.Trim() == "X")
                .Where(button => button.worldBound.y > 850f)
                .OrderByDescending(button => button.worldBound.x)
                .ThenBy(button => button.worldBound.y)
                .FirstOrDefault();

            if (closeButton == null)
            {
                Debug.LogWarning("No visible SDK finished close button found.");
                return false;
            }

            ClickSdkButton(closeButton);
            return true;
        }

        private static void ClickSdkButton(Button button)
        {
            var invoke = button.clickable?.GetType().GetMethod(
                "Invoke",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
            invoke?.Invoke(button.clickable, new object[] { null });
            Debug.Log($"Clicked visible SDK button: {button.text}");
        }

        private static void EnsureExpressions(string avatarName, VRCAvatarDescriptor descriptor)
        {
            var expressionsRoot = $"Assets/Mushus/{avatarName}/Expressions";
            EnsureFolder("Assets/Mushus", avatarName, "Expressions");

            if (descriptor.expressionParameters == null)
            {
                var parametersPath = $"{expressionsRoot}/{avatarName}ExpressionParameters.asset";
                var parameters = AssetDatabase.LoadAssetAtPath<VRCExpressionParameters>(parametersPath);
                if (parameters == null)
                {
                    parameters = ScriptableObject.CreateInstance<VRCExpressionParameters>();
                    parameters.parameters = new VRCExpressionParameters.Parameter[0];
                    AssetDatabase.CreateAsset(parameters, parametersPath);
                }

                descriptor.expressionParameters = parameters;
            }

            if (descriptor.expressionsMenu == null)
            {
                var menuPath = $"{expressionsRoot}/{avatarName}ExpressionsMenu.asset";
                var menu = AssetDatabase.LoadAssetAtPath<VRCExpressionsMenu>(menuPath);
                if (menu == null)
                {
                    menu = ScriptableObject.CreateInstance<VRCExpressionsMenu>();
                    AssetDatabase.CreateAsset(menu, menuPath);
                }

                descriptor.expressionsMenu = menu;
            }

            EditorUtility.SetDirty(descriptor);
        }

        private static void EnsureFolder(string parent, string avatarName, string child)
        {
            var avatarRoot = $"{parent}/{avatarName}";
            var target = $"{avatarRoot}/{child}";

            if (!AssetDatabase.IsValidFolder(target))
            {
                AssetDatabase.CreateFolder(avatarRoot, child);
            }
        }

        private static string WriteReport(string avatarName, VRCAvatarDescriptor descriptor, PublishProfile profile)
        {
            Directory.CreateDirectory(ReportDirectory);

            var spec = AvatarSpecCollector.Collect(descriptor.gameObject);
            var issues = CollectIssues(avatarName, descriptor, profile);
            var path = $"{ReportDirectory}/{avatarName}-publish-report.md";

            var lines = new List<string>
            {
                $"# {avatarName} Publish Report",
                "",
                $"Profile: `{GetProfilePath(avatarName)}`",
                $"Avatar name: `{profile.avatar.name}`",
                $"Description: {profile.avatar.description}",
                $"Thumbnail: `{profile.avatar.thumbnail}`",
                $"Release status: `{profile.avatar.releaseStatus}`",
                $"VRChat avatar ID: `{profile.vrchat.avatarId}`",
                $"Scene: `{GetSampleScenePath(avatarName, profile)}`",
                $"Prefab: `{GetPrefabPath(avatarName, profile)}`",
                $"Avatar object: `{descriptor.gameObject.name}`",
                $"Descriptor asset: `{AssetDatabase.GetAssetPath(PrefabUtility.GetCorrespondingObjectFromSource(descriptor) ?? descriptor)}`",
                "",
                "## Data as Code Platforms",
                "",
                $"- Windows scene: `{profile.platforms.windows.scene}`",
                $"- Windows prefab: `{profile.platforms.windows.prefab}`",
                $"- Android scene: `{profile.platforms.android.scene}`",
                $"- Android prefab: `{profile.platforms.android.prefab}`",
                $"- iOS scene: `{profile.platforms.ios.scene}`",
                $"- iOS prefab: `{profile.platforms.ios.prefab}`",
                "",
                "## Platform Support",
                "",
                $"- Windows: {BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64)}",
                $"- Android: {BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Android, BuildTarget.Android)}",
                $"- iOS: {BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.iOS, BuildTarget.iOS)}",
                $"- Active build target: {EditorUserBuildSettings.activeBuildTarget}",
                "",
                "## Avatar Spec",
                "",
                $"- Polygons: {spec.PolyCount:N0}",
                $"- Meshes: {spec.MeshCount}",
                $"- Materials: {spec.MaterialCount}",
                $"- PhysBones: {spec.PhysBoneCount}",
                $"- Eye Look: {spec.HasEyeLook}",
                $"- Expression Parameters: {spec.ExpressionParameters}",
                $"- Expression Menu Controls: {spec.ExpressionMenuCount}",
                $"- Shaders: {string.Join(", ", spec.Shaders)}",
                "",
                "## Issues",
                ""
            };

            if (issues.Count == 0)
            {
                lines.Add("- None detected by the local preflight.");
            }
            else
            {
                lines.AddRange(issues.Select(issue => "- " + issue));
            }

            File.WriteAllLines(path, lines);
            AssetDatabase.Refresh();
            return path;
        }

        private static List<string> CollectIssues(string avatarName, VRCAvatarDescriptor descriptor, PublishProfile profile)
        {
            var issues = new List<string>();
            var prefabPath = GetPrefabPath(avatarName, profile);

            if (string.IsNullOrEmpty(prefabPath))
            {
                issues.Add($"Distribution prefab is missing under Assets/Mushus/{avatarName}/Prefabs.");
            }

            if (descriptor.ViewPosition == Vector3.zero)
            {
                issues.Add("ViewPosition is zero.");
            }

            if (descriptor.expressionParameters == null)
            {
                issues.Add("Expression Parameters are not assigned.");
            }

            if (descriptor.expressionsMenu == null)
            {
                issues.Add("Expressions Menu is not assigned.");
            }

            if (string.IsNullOrWhiteSpace(profile.avatar.name))
            {
                issues.Add("Publish profile avatar.name is empty.");
            }

            if (string.IsNullOrWhiteSpace(profile.avatar.description))
            {
                issues.Add("Publish profile avatar.description is empty.");
            }

            if (!string.IsNullOrWhiteSpace(profile.vrchat.avatarId) && !profile.vrchat.avatarId.StartsWith("avtr_"))
            {
                issues.Add($"Publish profile vrchat.avatarId should start with avtr_: {profile.vrchat.avatarId}");
            }

            if (!string.IsNullOrWhiteSpace(profile.avatar.thumbnail) &&
                AssetDatabase.LoadAssetAtPath<Texture2D>(profile.avatar.thumbnail) == null)
            {
                issues.Add($"Publish profile thumbnail does not resolve to a Texture2D: {profile.avatar.thumbnail}");
            }

            AddPlatformIssue(issues, "windows", profile.platforms.windows);
            AddPlatformIssue(issues, "android", profile.platforms.android);
            AddPlatformIssue(issues, "ios", profile.platforms.ios);

            var shaders = descriptor.GetComponentsInChildren<Renderer>(true)
                .SelectMany(renderer => renderer.sharedMaterials)
                .Where(material => material != null && material.shader != null)
                .Select(material => material.shader.name)
                .Distinct()
                .ToList();

            if (shaders.Any(shader => !shader.StartsWith("VRChat/Mobile")))
            {
                issues.Add("Non-mobile shader detected. Android/iOS uploads should use VRChat mobile-compatible shaders.");
            }

            if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Android, BuildTarget.Android))
            {
                issues.Add("Android build support is not installed for this Unity editor.");
            }

            if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.iOS, BuildTarget.iOS))
            {
                issues.Add("iOS build support is not installed for this Unity editor.");
            }

            return issues;
        }

        private class SdkBuilderSnapshot
        {
            public string CapturedAt = "";
            public bool WindowFound;
            public string Status = "";
            public string SupportedPlatforms = "";
            public string LastUpdated = "";
            public string Version = "";
            public string MultiPlatformBuildState = "";
            public List<string> VisibleButtons = new List<string>();
            public List<string> VisibleTexts = new List<string>();
        }

        private static void AddPlatformIssue(List<string> issues, string platformName, PublishPlatform platform)
        {
            if (platform == null)
            {
                issues.Add($"Publish profile platforms.{platformName} is missing.");
                return;
            }

            if (string.IsNullOrWhiteSpace(platform.scene) || AssetDatabase.LoadAssetAtPath<SceneAsset>(platform.scene) == null)
            {
                issues.Add($"Publish profile platforms.{platformName}.scene is missing or invalid: {platform.scene}");
            }

            if (string.IsNullOrWhiteSpace(platform.prefab) || AssetDatabase.LoadAssetAtPath<GameObject>(platform.prefab) == null)
            {
                issues.Add($"Publish profile platforms.{platformName}.prefab is missing or invalid: {platform.prefab}");
            }
        }

        private static IEnumerable<Type> GetTypesSafe(System.Reflection.Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch
            {
                return new Type[0];
            }
        }

        private static IEnumerable<string> TypeSummary(Type type)
        {
            yield return $"TYPE {type.Assembly.GetName().Name} {type.FullName}";

            foreach (var method in type.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly)
                         .Where(method => method.Name.Contains("Build") || method.Name.Contains("Publish") || method.Name.Contains("Upload") || method.Name.Contains("Thumbnail") || method.Name.Contains("Blueprint")))
            {
                yield return $"  METHOD {method.Name}";
            }

            foreach (var field in type.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly)
                         .Where(field => field.Name.Contains("Build") || field.Name.Contains("Publish") || field.Name.Contains("Upload") || field.Name.Contains("Thumbnail") || field.Name.Contains("Blueprint")))
            {
                yield return $"  FIELD {field.Name}";
            }

            foreach (var property in type.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly)
                         .Where(property => property.Name.Contains("Build") || property.Name.Contains("Publish") || property.Name.Contains("Upload") || property.Name.Contains("Thumbnail") || property.Name.Contains("Blueprint")))
            {
                yield return $"  PROPERTY {property.Name}";
            }
        }

        private static string FormatParameters(System.Reflection.ParameterInfo[] parameters)
        {
            return string.Join(", ", parameters.Select(parameter => $"{parameter.ParameterType.FullName} {parameter.Name}"));
        }

        private static string GetSampleScenePath(string avatarName, PublishProfile profile)
        {
            if (!string.IsNullOrWhiteSpace(profile.platforms.windows.scene))
            {
                return profile.platforms.windows.scene;
            }

            return GetSampleScenePath(avatarName);
        }

        private static string GetSampleScenePath(string avatarName)
        {
            var devRoot = $"Assets/Mushus/{avatarName}Dev/Scenes";
            if (!Directory.Exists(devRoot))
            {
                return null;
            }

            return Directory.GetFiles(devRoot, "*.unity")
                .Select(path => path.Replace("\\", "/"))
                .OrderBy(path => Path.GetFileName(path) == "SampleScene.unity" ? 0 : 1)
                .FirstOrDefault();
        }

        private static string GetPrefabPath(string avatarName, PublishProfile profile)
        {
            if (!string.IsNullOrWhiteSpace(profile.platforms.windows.prefab))
            {
                return profile.platforms.windows.prefab;
            }

            return GetPrefabPath(avatarName);
        }

        private static string GetPrefabPath(string avatarName)
        {
            var prefabRoot = $"Assets/Mushus/{avatarName}/Prefabs";
            if (!Directory.Exists(prefabRoot))
            {
                return null;
            }

            return Directory.GetFiles(prefabRoot, "*.prefab")
                .Select(path => path.Replace("\\", "/"))
                .OrderBy(path => Path.GetFileNameWithoutExtension(path).Contains(avatarName) ? 0 : 1)
                .FirstOrDefault();
        }

        private static string GetSampleAvatarObjectName(string avatarName)
        {
            return avatarName == DefaultAvatarName ? "WindraLowPoly" : avatarName + "Sample";
        }

        private static PublishProfile LoadProfile(string avatarName)
        {
            var path = GetProfilePath(avatarName);
            if (!File.Exists(path))
            {
                return PublishProfile.CreateDefault(avatarName);
            }

            var profile = JsonUtility.FromJson<PublishProfile>(File.ReadAllText(path));
            return profile == null ? PublishProfile.CreateDefault(avatarName) : profile.WithDefaults(avatarName);
        }

        private static string GetProfilePath(string avatarName)
        {
            return $"Assets/Mushus/{avatarName}/{PublishFolderName}/{avatarName}.publish.json";
        }

        [System.Serializable]
        private class PublishProfile
        {
            public PublishAvatar avatar = new PublishAvatar();
            public PublishVrChat vrchat = new PublishVrChat();
            public PublishPlatforms platforms = new PublishPlatforms();

            public static PublishProfile CreateDefault(string avatarName)
            {
                return new PublishProfile().WithDefaults(avatarName);
            }

            public PublishProfile WithDefaults(string avatarName)
            {
                avatar = avatar ?? new PublishAvatar();
                vrchat = vrchat ?? new PublishVrChat();
                platforms = platforms ?? new PublishPlatforms();
                platforms.windows = platforms.windows ?? new PublishPlatform();
                platforms.android = platforms.android ?? new PublishPlatform();
                platforms.ios = platforms.ios ?? new PublishPlatform();

                if (string.IsNullOrWhiteSpace(avatar.name))
                {
                    avatar.name = GetSampleAvatarObjectName(avatarName);
                }

                if (string.IsNullOrWhiteSpace(avatar.releaseStatus))
                {
                    avatar.releaseStatus = "private";
                }

                return this;
            }
        }

        [System.Serializable]
        private class PublishAvatar
        {
            public string name;
            public string description;
            public string releaseStatus;
            public string thumbnail;
        }

        [System.Serializable]
        private class PublishVrChat
        {
            public string avatarId;
            public List<string> tags = new List<string>();
        }

        [System.Serializable]
        private class PublishPlatforms
        {
            public PublishPlatform windows = new PublishPlatform();
            public PublishPlatform android = new PublishPlatform();
            public PublishPlatform ios = new PublishPlatform();
        }

        [System.Serializable]
        private class PublishPlatform
        {
            public string scene;
            public string prefab;
        }
    }
}
