using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VRC.SDK3.Avatars.Components;

namespace Mushus.CaptureTools
{
    public static class CaptureExecutor
    {
        public static void ExecuteThumbnailCapture(AvatarCaptureSettings settings)
        {
            if (settings.TargetAvatar == null)
            {
                Debug.LogError("[Capture] TargetAvatar is not set.");
                return;
            }

            string packageName = string.IsNullOrEmpty(settings.PackageName) ? settings.TargetAvatar.name : settings.PackageName;
            string basePath = $"Assets/Mushus/{packageName}/Previews/";

            try
            {
                // 背景透明でキャプチャ
                if (settings.FrontCamera != null) SaveCapture(settings.FrontCamera, basePath + "01_Front.png", true);
                if (settings.BackCamera != null) SaveCapture(settings.BackCamera, basePath + "02_Back.png", true);
                if (settings.SideCamera != null) SaveCapture(settings.SideCamera, basePath + "03_Side.png", true);
                if (settings.FaceCamera != null) SaveCapture(settings.FaceCamera, basePath + "04_FaceUp.png", true);

                AssetDatabase.Refresh();
                Debug.Log($"[Capture Success] All views processed with transparent BG for: {packageName}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[Capture Error] Thumbnail capture failed: {e}");
            }
        }

        public static void ExecuteExpressionCapture(AvatarCaptureSettings settings)
        {
            if (settings.TargetAvatar == null || settings.FaceCamera == null || settings.ExpressionClips.Count == 0)
            {
                Debug.LogError("[Capture] TargetAvatar, FaceCamera, or ExpressionClips are missing.");
                return;
            }

            string packageName = string.IsNullOrEmpty(settings.PackageName) ? settings.TargetAvatar.name : settings.PackageName;
            string basePackageName = packageName.Replace("Dev", "");
            string iconPath = $"Assets/Mushus/{basePackageName}/Icons/";
            string previewPath = $"Assets/Mushus/{packageName}/Previews/";
            string oldListPath = previewPath + "Expression_List.png";

            try 
            {
                // 古いリスト画像が残っている場合は削除して混乱を防ぐ
                string oldListFullPath = Path.Combine(Application.dataPath, oldListPath.Replace("Assets/", ""));
                if (File.Exists(oldListFullPath))
                {
                    File.Delete(oldListFullPath);
                    if (File.Exists(oldListFullPath + ".meta")) File.Delete(oldListFullPath + ".meta");
                    Debug.Log($"[Capture] Deleted legacy list file: {oldListPath}");
                }

                // 無表情（アニメーションなし）の状態をキャプチャ
                AnimationMode.StopAnimationMode();
                SaveExpressionIcon(settings, null, iconPath + "None.png");
                SaveExpressionIcon(settings, null, previewPath + "Expression_None.png");

                foreach (var clip in settings.ExpressionClips)
                {
                    if (clip == null) continue;
                    SaveExpressionIcon(settings, clip, iconPath + $"{clip.name}.png");
                    SaveExpressionIcon(settings, clip, previewPath + $"Expression_{clip.name}.png");
                }

                AssetDatabase.Refresh();
                Debug.Log($"[Capture Success] Expression icons processed for: {packageName} (Saved to Icons and Previews)");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[Capture Error] Expression capture failed: {e}");
            }
            finally
            {
                AnimationMode.StopAnimationMode();
            }
        }

        private static void SaveExpressionIcon(AvatarCaptureSettings settings, AnimationClip clip, string assetPath)
        {
            GameObject clone = Object.Instantiate(settings.TargetAvatar, settings.TargetAvatar.transform.parent);
            clone.hideFlags = HideFlags.HideAndDontSave;
            
            try {
                var animator = settings.TargetAvatar.GetComponent<Animator>();
                var cloneAnimator = clone.GetComponent<Animator>();
                settings.TargetAvatar.SetActive(false);

                if (clip != null)
                {
                    AnimationMode.StartAnimationMode();
                    AnimationMode.BeginSampling();
                    AnimationMode.SampleAnimationClip(clone, clip, clip.length);
                    AnimationMode.EndSampling();
                }

                if (animator != null && animator.isHuman && cloneAnimator.isHuman)
                {
                    var srcHandler = new HumanPoseHandler(animator.avatar, animator.transform);
                    var dstHandler = new HumanPoseHandler(cloneAnimator.avatar, cloneAnimator.transform);
                    HumanPose pose = new HumanPose();
                    srcHandler.GetHumanPose(ref pose);
                    dstHandler.SetHumanPose(ref pose);
                }

                Texture2D tex = CaptureCore.Capture(settings.FaceCamera, 256, 256, true);
                CaptureCore.ApplyRoundedCorners(tex, 15f);
                SaveTex(tex, assetPath);
                Object.DestroyImmediate(tex);
            }
            finally {
                Object.DestroyImmediate(clone);
                settings.TargetAvatar.SetActive(true);
            }
        }

        public static List<AnimationClip> DetectExpressionClips(GameObject targetAvatar)
        {
            List<AnimationClip> clips = new List<AnimationClip>();
            if (targetAvatar == null) return clips;

            var descriptor = targetAvatar.GetComponent<VRCAvatarDescriptor>();
            if (descriptor == null) return clips;

            // FXレイヤーのコントローラーを取得
            var fxLayer = descriptor.baseAnimationLayers.FirstOrDefault(l => l.type == VRCAvatarDescriptor.AnimLayerType.FX);
            if (fxLayer.animatorController == null) return clips;

            var controller = fxLayer.animatorController as UnityEditor.Animations.AnimatorController;
            if (controller == null) return clips;

            // 表情に関連しそうなキーワード
            string[] keywords = { "smile", "eye", "face", "expression", "surprise", "angry", "sorrow", "joy", "wink", "mouth", "blink" };

            // コントローラー内の全アニメーションクリップをスキャン
            foreach (var clip in controller.animationClips)
            {
                if (clip == null) continue;
                string nameLower = clip.name.ToLower();
                if (keywords.Any(k => nameLower.Contains(k)))
                {
                    if (!clips.Contains(clip)) clips.Add(clip);
                }
            }

            return clips;
        }

        private static void SaveCapture(Camera cam, string assetPath, bool transparent)
        {
            // 正方形 (512x512) でキャプチャ
            Texture2D tex = CaptureCore.Capture(cam, 512, 512, transparent);
            SaveTex(tex, assetPath);
            Object.DestroyImmediate(tex);
        }

        private static void SaveTex(Texture2D tex, string assetPath)
        {
            byte[] bytes = tex.EncodeToPNG();
            string fullPath = Path.Combine(Application.dataPath, assetPath.Replace("Assets/", ""));
            fullPath = fullPath.Replace("\\", "/");
            
            string dir = Path.GetDirectoryName(fullPath);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            
            File.WriteAllBytes(fullPath, bytes);
            Debug.Log($"[Capture] File Saved (Transparent): {fullPath}");
        }
    }
}
