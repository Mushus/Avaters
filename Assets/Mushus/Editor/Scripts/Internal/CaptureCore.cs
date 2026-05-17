using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace Mushus.CaptureTools
{
    public static class CaptureCore
    {
        /// <summary>
        /// 指定したカメラの内容をTexture2Dとしてキャプチャします。
        /// </summary>
        public static Texture2D Capture(Camera camera, int width, int height, bool transparent = false)
        {
            RenderTexture rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            RenderTexture prev = camera.targetTexture;
            RenderTexture prevActive = RenderTexture.active;

            camera.targetTexture = rt;
            
            // 背景をクリア (透明にする)
            var oldClearFlags = camera.clearFlags;
            var oldBgColor = camera.backgroundColor;
            if (transparent)
            {
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = Color.clear;
            }
            
            camera.Render();
            
            if (transparent)
            {
                camera.clearFlags = oldClearFlags;
                camera.backgroundColor = oldBgColor;
            }

            RenderTexture.active = rt;
            Texture2D screenShot = new Texture2D(width, height, TextureFormat.ARGB32, false);
            screenShot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            screenShot.Apply();

            camera.targetTexture = prev;
            RenderTexture.active = prevActive;
            Object.DestroyImmediate(rt);

            return screenShot;
        }

        /// <summary>
        /// 画像に角丸加工を施します。
        /// </summary>
        public static void ApplyRoundedCorners(Texture2D tex, float radiusPercent)
        {
            int w = tex.width;
            int h = tex.height;
            float r = Mathf.Min(w, h) * (radiusPercent / 100f);
            Color[] pixels = tex.GetPixels();

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float dx = 0, dy = 0;
                    bool inCorner = false;

                    if (x < r && y < r) { dx = r - x; dy = r - y; inCorner = true; }
                    else if (x > w - r && y < r) { dx = x - (w - r); dy = r - y; inCorner = true; }
                    else if (x < r && y > h - r) { dx = r - x; dy = y - (h - r); inCorner = true; }
                    else if (x > w - r && y > h - r) { dx = x - (w - r); dy = y - (h - r); inCorner = true; }

                    if (inCorner)
                    {
                        float dist = Mathf.Sqrt(dx * dx + dy * dy);
                        if (dist > r)
                        {
                            pixels[y * w + x] = Color.clear;
                        }
                        else if (dist > r - 1f)
                        {
                            // アンチエイリアス（簡易）
                            float alpha = 1f - (dist - (r - 1f));
                            Color c = pixels[y * w + x];
                            c.a *= alpha;
                            pixels[y * w + x] = c;
                        }
                    }
                }
            }
            tex.SetPixels(pixels);
            tex.Apply();
        }

        /// <summary>
        /// 複数の画像をタイル状に並べた1枚の画像を生成します。
        /// </summary>
        public static Texture2D CreateTile(List<Texture2D> textures, int columns, int padding = 10, string title = "")
        {
            if (textures == null || textures.Count == 0) return null;

            int count = textures.Count;
            int rows = Mathf.CeilToInt((float)count / columns);
            int cellW = textures[0].width;
            int cellH = textures[0].height;

            int totalW = columns * cellW + (columns + 1) * padding;
            int totalH = rows * cellH + (rows + 1) * padding;
            
            // タイトル用の余白
            int titleHeight = string.IsNullOrEmpty(title) ? 0 : 100;
            totalH += titleHeight;

            Texture2D combined = new Texture2D(totalW, totalH, TextureFormat.ARGB32, false);
            // 背景を塗りつぶし（グレーなど）
            Color[] bg = new Color[totalW * totalH];
            for (int i = 0; i < bg.Length; i++) bg[i] = new Color(0.15f, 0.15f, 0.15f, 1f);
            combined.SetPixels(bg);

            for (int i = 0; i < count; i++)
            {
                int col = i % columns;
                int row = i / columns;
                int x = padding + col * (cellW + padding);
                int y = totalH - titleHeight - padding - (row + 1) * cellH - row * padding;

                combined.SetPixels(x, y, cellW, cellH, textures[i].GetPixels());
            }

            combined.Apply();
            return combined;
        }

        /// <summary>
        /// 背景色を合成します。
        /// </summary>
        public static void CompositeBackground(Texture2D tex, Color bgColor)
        {
            Color[] pixels = tex.GetPixels();
            for (int i = 0; i < pixels.Length; i++)
            {
                float alpha = pixels[i].a;
                pixels[i].r = Mathf.Lerp(bgColor.r, pixels[i].r, alpha);
                pixels[i].g = Mathf.Lerp(bgColor.g, pixels[i].g, alpha);
                pixels[i].b = Mathf.Lerp(bgColor.b, pixels[i].b, alpha);
                pixels[i].a = 1.0f;
            }
            tex.SetPixels(pixels);
            tex.Apply();
        }

        /// <summary>
        /// 指定したフォントでテキストを描画します。
        /// </summary>
        public static void DrawText(Texture2D tex, string text, string fontPath, int fontSize, Color color, TextAnchor anchor, int padding = 20)
        {
            if (tex == null) return;
            if (!File.Exists(fontPath))
            {
                Debug.LogWarning($"Font not found: {fontPath}.");
                return;
            }

            // フォントの読み込み
            Font font = new Font(fontPath);
            if (font == null) return;
            
            // ダイナミックフォントのテクスチャを強制的に生成
            font.RequestCharactersInTexture(text, fontSize, FontStyle.Normal);
            
            // 文字情報を取得して、テクスチャが更新されたか確認
            bool allFound = true;
            foreach (char c in text)
            {
                if (!font.GetCharacterInfo(c, out _, fontSize)) allFound = false;
            }
            
            if (!allFound)
            {
                // まだテクスチャが生成されていない可能性があるため、少し多めに文字を要求
                font.RequestCharactersInTexture(text + " !\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ", fontSize);
            }
            
            // フォントテクスチャの生成を確認
            Material fontMat = font.material;
            if (fontMat == null || fontMat.mainTexture == null)
            {
                Debug.LogWarning("Font material or texture is null.");
                Object.DestroyImmediate(font);
                return;
            }

            // フォントテクスチャを読み取り可能な Texture2D にコピー
            Texture2D fontTexSrc = fontMat.mainTexture as Texture2D;
            RenderTexture tempRT = RenderTexture.GetTemporary(fontTexSrc.width, fontTexSrc.height, 0, RenderTextureFormat.ARGB32);
            Graphics.Blit(fontTexSrc, tempRT);
            RenderTexture prevRT = RenderTexture.active;
            RenderTexture.active = tempRT;
            Texture2D fontTexReadable = new Texture2D(fontTexSrc.width, fontTexSrc.height, TextureFormat.ARGB32, false);
            fontTexReadable.ReadPixels(new Rect(0, 0, fontTexSrc.width, fontTexSrc.height), 0, 0);
            fontTexReadable.Apply();
            RenderTexture.active = prevRT;
            RenderTexture.ReleaseTemporary(tempRT);

            // テキストのサイズを計算
            int totalWidth = 0;
            int totalHeight = fontSize;
            foreach (char c in text)
            {
                if (font.GetCharacterInfo(c, out CharacterInfo info, fontSize))
                {
                    totalWidth += info.advance;
                }
            }

            // 描画開始位置を計算
            int startX = padding;
            int startY = padding; // 下端からの距離 (Texture2D は左下が 0,0)

            if (anchor == TextAnchor.LowerRight || anchor == TextAnchor.MiddleRight || anchor == TextAnchor.UpperRight)
                startX = tex.width - totalWidth - padding;
            else if (anchor == TextAnchor.LowerCenter || anchor == TextAnchor.MiddleCenter || anchor == TextAnchor.UpperCenter)
                startX = (tex.width - totalWidth) / 2;

            if (anchor == TextAnchor.UpperLeft || anchor == TextAnchor.UpperCenter || anchor == TextAnchor.UpperRight)
                startY = tex.height - totalHeight - padding;
            else if (anchor == TextAnchor.MiddleLeft || anchor == TextAnchor.MiddleCenter || anchor == TextAnchor.MiddleRight)
                startY = (tex.height - totalHeight) / 2;

            // 各文字を描画
            int currentX = startX;
            foreach (char c in text)
            {
                if (font.GetCharacterInfo(c, out CharacterInfo info, fontSize))
                {
                    // テクスチャ上のピクセル座標を計算
                    int glyphX = (int)(info.uvBottomLeft.x * fontTexReadable.width);
                    int glyphY = (int)(info.uvBottomLeft.y * fontTexReadable.height);
                    int glyphW = info.glyphWidth;
                    int glyphH = info.glyphHeight;

                    // 文字の描画位置 (ベースラインを考慮)
                    int drawX = currentX + info.minX;
                    int drawY = startY + (fontSize + info.minY); // fontSize でオフセットしてベースライン調整

                    for (int y = 0; y < glyphH; y++)
                    {
                        for (int x = 0; x < glyphW; x++)
                        {
                            // フォントテクスチャからピクセル取得
                            // 0.5f を足すことでピクセルの中心をサンプリングする
                            float u = Mathf.Lerp(info.uvBottomLeft.x, info.uvTopRight.x, (x + 0.5f) / glyphW);
                            float v = Mathf.Lerp(info.uvBottomLeft.y, info.uvTopRight.y, (y + 0.5f) / glyphH);
                            
                            Color fontPixel = fontTexReadable.GetPixelBilinear(u, v);

                            int targetX = drawX + x;
                            int targetY = drawY + y;

                            if (targetX >= 0 && targetX < tex.width && targetY >= 0 && targetY < tex.height)
                            {
                                Color targetPixel = tex.GetPixel(targetX, targetY);
                                // アルファ値として、Red または Alpha の最大値を使用（フォントテクスチャの仕様に合わせる）
                                float alpha = Mathf.Max(fontPixel.r, fontPixel.a) * color.a;
                                
                                // アルファ値が非常に小さい場合はスキップ（ノイズ防止）
                                if (alpha < 0.01f) continue;

                                Color blended = Color.Lerp(targetPixel, color, alpha);
                                tex.SetPixel(targetX, targetY, blended);
                            }
                        }
                    }
                    currentX += info.advance;
                }
            }

            tex.Apply();
            Object.DestroyImmediate(fontTexReadable);
            Object.DestroyImmediate(font);
        }

        /// <summary>
        /// ファイルからテクスチャを読み込みます。
        /// </summary>
        public static Texture2D LoadTexture(string path)
        {
            if (!File.Exists(path)) return null;
            byte[] bytes = File.ReadAllBytes(path);
            Texture2D tex = new Texture2D(2, 2);
            tex.LoadImage(bytes);
            return tex;
        }

        /// <summary>
        /// 指定したパスに画像を保存します。
        /// </summary>
        public static void SaveTexture(Texture2D tex, string path)
        {
            byte[] bytes = tex.EncodeToPNG();
            string dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            File.WriteAllBytes(path, bytes);
            Debug.Log($"Saved image to: {path}");
        }
    }
}
