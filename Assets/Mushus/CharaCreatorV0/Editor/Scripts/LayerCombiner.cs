using UnityEngine;
using UnityEditor;
using System.IO;

namespace Mushus.CharaCreatorV0
{
    internal class CombineLayer
    {
        internal readonly int BlendMode;
        internal readonly Texture Texture;
        internal readonly Color Color;
        internal readonly float Opacity = 1;
        internal readonly Rect Offset = new Rect(0, 0, 1, 1);

        internal CombineLayer(int blendMode = 0, Texture texture = null, Color? color = null, float opacity = 1, Rect? offset = null)
        {
            this.BlendMode = blendMode;
            this.Texture = texture;
            this.Color = color ?? Color.white;
            this.Opacity = opacity;
            this.Offset = offset ?? new Rect(0, 0, 1, 1);
        }
    }

    internal static class LayerCombiner
    {
        internal static Shader normalSharder
        {
            get
            {
                return Shader.Find("Mushus/CharaCreatorV0/ImageBlendNormal");
            }
        }

        internal static Texture2D Combine(Vector2Int size, CombineLayer[] layers)
        {
            var width = size.x;
            var height = size.y;

            var combinedTexture = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB, 1);

            var dstTexture = new Texture2D(width, height, TextureFormat.ARGB32, false, false);
            Color fillColor = Color.clear;
            Color[] fillPixels = new Color[dstTexture.width * dstTexture.height];

            for (int i = 0; i < fillPixels.Length; i++)
            {
                fillPixels[i] = fillColor;
            }

            dstTexture.SetPixels(fillPixels);
            dstTexture.Apply();

            for (int i = 0; i < layers.Length; i++)
            {
                CombineLayer layer = layers[i];
                Texture texture = layer.Texture;
                if (texture == null)
                {
                    continue;
                }

                var mat = new Material(normalSharder);
                mat.SetTexture("_DstTex", dstTexture);
                mat.SetColor("_Color", layer.Color);
                mat.SetInt("_BlendMode", layer.BlendMode);
                mat.SetFloat("_Opacity", layer.Opacity);
                mat.SetVector("_Offset", new Vector4(layer.Offset.x, layer.Offset.y, layer.Offset.width, layer.Offset.height));

                Graphics.Blit(texture, combinedTexture, mat);

                var oldRender = RenderTexture.active;
                RenderTexture.active = combinedTexture;

                dstTexture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                dstTexture.Apply();

                RenderTexture.active = oldRender;
            }

            return dstTexture;
        }

        internal static void ExportTexture2D(Texture2D texture, string path)
        {
            var bytes = texture.EncodeToPNG();
            File.WriteAllBytes(path, bytes);
            AssetDatabase.Refresh();
        }
    }
}