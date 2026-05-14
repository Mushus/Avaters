using UnityEngine;
using UnityEditor;

namespace Mushus.CharaCreatorV0
{
    public class AssetLoader
    {
        public readonly string PathPrefix;
        public AssetLoader(string pathPrefix)
        {
            this.PathPrefix = pathPrefix;
        }

        public Object[] Model(string path)
        {
            return AssetDatabase.LoadAllAssetsAtPath($"{PathPrefix}/Models/{path}");
        }

        public Texture Texture(string path)
        {
            return AssetDatabase.LoadAssetAtPath<Texture>($"{PathPrefix}/Textures/{path}");
        }

        public Texture SimpleTexture(Color color)
        {
            var texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, color);
            texture.Apply();
            return texture;
        }

        public Texture BaseTexture()
        {
            var texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            return texture;
        }
    }
}