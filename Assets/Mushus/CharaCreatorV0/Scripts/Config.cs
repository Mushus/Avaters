using GameObject = UnityEngine.GameObject;
using Object = UnityEngine.Object;
using Transform = UnityEngine.Transform;
using SkinnedMeshRenderer = UnityEngine.SkinnedMeshRenderer;
using System.Linq;

namespace Mushus.CharaCreatorV0
{
    public class Config
    {
        public readonly ColorPalette[] Colors;
        public readonly Surface Sharder;
        public readonly PartVariant Parts;
        public Config(PartVariant parts, ColorPalette[] colors, Surface sharder = null)
        {
            this.Parts = parts;
            this.Colors = colors != null ? colors : new ColorPalette[] { };
            this.Sharder = sharder != null ? sharder : new Surface();
        }
    }

    public class Surface
    {
        public readonly string Name;
        public Surface(string name = "Standard")
        {
            this.Name = name;
        }
    }

    public enum TextureSize
    {
        Small = 0,
        Medium = 1,
        Large = 2
    }

    public class Part
    {
        public readonly string Key;
        public readonly string Name;
        public readonly PartVariant[] Children;
        public readonly Model[] Models;
        public readonly LayerVariant[] Textures;
        public readonly TextureSize TextureSize;
        public readonly Morph[] Morphs;
        public Part(string key, string name = null, Model[] models = null, Model model = null, LayerVariant[] textures = null, TextureSize textureSize = TextureSize.Medium, PartVariant[] children = null, Morph[] morphs = null)
        {
            Key = key;
            Name = name ?? key;
            Models = models ?? (model != null ? new Model[] { model } : null);
            Textures = textures ?? null;
            TextureSize = textureSize;
            Children = children ?? new PartVariant[] { };
            Morphs = morphs ?? new Morph[] { };
        }

        public static Part None = new Part(key: "_none", name: "--- なし ---");
        public static Part[] List(params Part[] parts)
        {
            return parts;
        }
    }

    public class PartVariant
    {
        public readonly string Key;
        public readonly string Name;
        public readonly bool isShowHirarchy;
        public readonly Part[] parts;

        public PartVariant(string key, string name = null, bool isShowHirarchy = true, Part[] parts = null)
        {
            this.Key = key;
            this.Name = name ?? key;
            this.isShowHirarchy = isShowHirarchy;
            this.parts = parts ?? new Part[] { Part.None };
        }
    }

    public class Model
    {
        public readonly string RootBone;
        public readonly Transform Armature;
        public readonly GameObject[] Meshes;
        public Model(string rootBone = "Hips", Object[] assets = null)
        {
            RootBone = rootBone;
            var _assets = assets ?? new Object[] { };
            Armature = _assets.FirstOrDefault(a => a is Transform && a.name == Constant.ArmatureName) as Transform;
            Meshes = _assets
                .Where(a => a is GameObject)
                .Where(a => (a as GameObject).GetComponent<SkinnedMeshRenderer>() != null)
                .Select(a => a as GameObject)
                .ToArray();
        }
    }

    public class Layer
    {
        public readonly string Key;
        public readonly string Name;
        public readonly Image[] Textures;
        public Layer(string name = null, Image[] textures = null, Image texture = null)
        {
            this.Name = name;
            this.Textures = textures ?? (texture != null ? new Image[] { texture } : new Image[] { });
        }

        public static Layer None = new Layer(name: "--- なし ---");
    }

    public class LayerVariant
    {
        public readonly string Key;
        public readonly string Name;
        public readonly Layer[] Variants;
        public LayerVariant(string key, string name = null, Layer[] variants = null, Layer layer = null)
        {
            this.Key = key;
            this.Name = name ?? key;
            this.Variants = variants ?? (layer != null ? new Layer[] { layer } : new Layer[] { });
        }
    }

    /** 画像の合成モード */
    public enum BlendMode
    {
        /** 通常 */
        Normal = 0,
        /** 乗算 */
        Multiply = 1,
        /** 加算 */
        Additive = 2,
        /** 減算 */
        Screen = 3,
        /** オーバーレイ */
        Overlay = 4,
        /** 比較（暗） */
        Darken = 5,
        /** 比較（明） */
        Lighten = 6,
        /** 覆い焼き */
        ColorDodge = 7,
        /** 焼き込み */
        ColorBurn = 8,
        /** ハードライト */
        HardLight = 9,
        /** ソフトライト */
        SoftLight = 10,
        /** 差の絶対値 */
        Difference = 11,
        /** 除外 */
        Exclusion = 12,
    }

    /** テクスチャの設定 */
    public class Image
    {
        /** テクスチャのキー */
        public readonly string Key;
        /** テクスチャの名前 */
        public readonly string Name;
        /** テクスチャの画像 */
        public readonly UnityEngine.Texture Texture;
        /** テクスチャの色のキー */
        public readonly string ColorKey;
        /** テクスチャの色 */
        public readonly UnityEngine.Color Color;
        /** 画像の合成モード */
        public readonly BlendMode BlendMode;
        public Image(string key, UnityEngine.Texture image = null, string name = null, string colorKey = "", UnityEngine.Color? color = null, BlendMode blendMode = BlendMode.Normal)
        {
            this.Key = key;
            this.Name = name ?? key;
            this.Texture = image;
            this.ColorKey = colorKey;
            this.Color = color != null ? color.Value : UnityEngine.Color.white;
            this.BlendMode = blendMode;
        }
    }

    public class ColorPalette
    {
        public readonly string Key;
        public readonly string Name;
        public readonly UnityEngine.Color Value;
        public ColorPalette(string key, UnityEngine.Color value, string name = null)
        {
            this.Key = key;
            this.Name = name ?? key;
            this.Value = value;
        }
    }

    public class Morph
    {
        public readonly string Key;
        public readonly string Name;
        public readonly BlendShapeDefinition[] BlendShapes;
        public readonly bool DefaultOnly;
        public Morph(string key, string name, BlendShapeDefinition[] blendShapes, bool defaultOnly = false)
        {
            this.Key = key;
            this.Name = name;
            this.BlendShapes = blendShapes;
            this.DefaultOnly = defaultOnly;
        }

        public Morph(string key, string name, BlendShapeDefinition blendShapeName, bool defaultOnly = false)
        {
            this.Key = key;
            this.Name = name;
            this.BlendShapes = new BlendShapeDefinition[] { blendShapeName };
            this.DefaultOnly = defaultOnly;
        }
    }

    public class BlendShapeDefinition
    {
        public readonly string Key;
        public readonly string Name;
        public BlendShapeDefinition(string key, string name)
        {
            this.Key = key;
            this.Name = name;
        }

        public static BlendShapeDefinition None = new BlendShapeDefinition(key: "_none", name: "--- なし ---");
    }

    public static class ConfigExtension
    {
        public static int Size(this TextureSize textureSize)
        {
            return 1 << (int)textureSize;
        }
    }
}