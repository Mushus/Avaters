using UnityEngine;

namespace Mushus.DistributionTools
{
    /// <summary>
    /// 各アバターの配布用設定をシーン内に保持するためのコンポーネント。
    /// </summary>
    public class AvatarDistributionSettings : MonoBehaviour
    {
        [Header("Target Avatar")]
        [Tooltip("配布対象のアバタープレハブを設定してください。")]
        public GameObject TargetAvatar;

        [Header("Documentation & License")]
        [Tooltip("オンライン説明書のURL。")]
        public string DescriptionUrl = "https://example.com/avatar-docs";
        
        [Tooltip("同梱するREADMEに記載するライセンス文。")]
        [TextArea(5, 10)]
        public string LicenseText = "本アバターは共通利用規約（VN3ライセンス等）を適用しています。詳細は同梱の規約ファイルまたはリンク先をご確認ください。";

        [Header("Analysis Result (Read Only)")]
        public AvatarSpec CurrentSpec;
    }
}
