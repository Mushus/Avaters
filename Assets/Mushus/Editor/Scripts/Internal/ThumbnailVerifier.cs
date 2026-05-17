using UnityEngine;
using UnityEditor;
using System.IO;

namespace Mushus.CaptureTools
{
    public static class ThumbnailVerifier
    {
        [MenuItem("Mushus/Debug/Verify DrawText Performance")]
        public static void Verify()
        {
            int size = 512;
            Texture2D testTex = new Texture2D(size, size, TextureFormat.ARGB32, false);
            
            // 背景をグレーに
            Color[] bg = new Color[size * size];
            for(int i=0; i<bg.Length; i++) bg[i] = new Color(0.5f, 0.5f, 0.5f, 1.0f);
            testTex.SetPixels(bg);
            testTex.Apply();

            string fontPath = @"C:\Windows\Fonts\NotoSansJP-VF.ttf";
            if (!File.Exists(fontPath)) fontPath = @"C:\Windows\Fonts\arial.ttf"; // フォールバック

            Debug.Log($"[Verifier] Using font: {fontPath}");

            // 描画テスト
            CaptureCore.DrawText(testTex, "TEST DRAW", fontPath, 64, Color.white, TextAnchor.MiddleCenter, 0);
            CaptureCore.DrawText(testTex, "LowerRight", fontPath, 32, Color.cyan, TextAnchor.LowerRight, 20);

            byte[] png = testTex.EncodeToPNG();
            string path = Path.Combine(Application.dataPath, "thumbnail_verify_test.png");
            File.WriteAllBytes(path, png);
            AssetDatabase.ImportAsset("Assets/thumbnail_verify_test.png");
            
            Debug.Log($"[Verifier] Test image saved to: Assets/thumbnail_verify_test.png. Please check if text is visible.");
            
            Object.DestroyImmediate(testTex);
        }
    }
}
