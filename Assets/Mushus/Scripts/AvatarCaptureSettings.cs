using UnityEngine;
using System.Collections.Generic;

namespace Mushus.CaptureTools
{
    /// <summary>
    /// 開発・ブース素材作成用のキャプチャ設定。
    /// ユーザーがシーン内に配置したカメラの視点をそのまま撮影します。
    /// </summary>
    [AddComponentMenu("Mushus/Capture/Avatar Capture Settings")] 
    public class AvatarCaptureSettings : MonoBehaviour
    {
        public GameObject TargetAvatar;
        
        [Header("Output Settings")]
        [Tooltip("プロジェクト名（例: Windra）。空の場合はシーンのフォルダ名から推測されます。")]
        public string PackageName;

        [Header("Camera References")]
        public Camera FrontCamera;
        public Camera BackCamera;
        public Camera SideCamera;
        public Camera FaceCamera;

        [Header("Expression Animations")]
        public List<AnimationClip> ExpressionClips = new List<AnimationClip>();

        [Header("Marketing Image Settings")]
        public Color ThumbnailBackgroundColor = new Color(0.5f, 0.5f, 0.5f, 1f);
        public string ThumbnailFontPath = @"C:\Windows\Fonts\NotoSansJP-VF.ttf";
        public int ThumbnailFontSize = 48;
        public int ThumbnailLabelPadding = 30;
    }
}
