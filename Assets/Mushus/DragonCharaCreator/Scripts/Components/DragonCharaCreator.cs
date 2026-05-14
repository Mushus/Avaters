using UnityEditor;
using Mushus.CharaCreatorV0;

namespace Mushus.DragonCharaCreator
{
    public class DragonCharaCreator : CharaCreator
    {
        public override Config GetConfig()
        {
            var colors = new ColorPalette[]
            {
                new ColorPalette(key: "main", name: "メインカラー", value: new UnityEngine.Color(0.81f, 0.34f, 0.48f)),
                new ColorPalette(key: "sub", name: "サブカラー", value: new UnityEngine.Color(0.95f, 0.89f, 0.79f)),
                new ColorPalette(key: "accent", name: "アクセントカラー", value: new UnityEngine.Color(0.18f, 0.20f, 0.32f)),
                new ColorPalette(key: "skin", name: "スキンカラー", value: new UnityEngine.Color(0.95f, 0.46f, 0.46f)),
                new ColorPalette(key: "outline", name: "アウトライン", value: new UnityEngine.Color(0.18f, 0.20f, 0.32f)),
                new ColorPalette(key: "claw", name: "ツメ", value: new UnityEngine.Color(0.91f, 0.91f, 0.93f)),
                new ColorPalette(key: "shade", name: "かげ", value: new UnityEngine.Color(0.58f, 0.20f, 0.32f)),
                new ColorPalette(key: "temporary", name: "色1", value: UnityEngine.Color.gray),
            };

            var loader = new AssetLoader("Assets/Mushus/DragonCharaCreator");

            var arm001 = new Part(
                key: "arm001",
                name: "うで001",
                models: new Model[]{
                    new Model("LeftUpperArm", loader.Model("Arm001Left.fbx")),
                    new Model("RightUpperArm", loader.Model("Arm001Right.fbx")),
                },
                textures: new LayerVariant[]{
                    new LayerVariant(
                        key: "base",
                        name: "ベースカラー",
                        layer: new Layer(texture: new Image(key:"base001", loader.BaseTexture(), colorKey: "main"))
                    ),
                    new LayerVariant(
                        key: "claw",
                        name: "ツメ",
                        layer: new Layer(texture: new Image(key:"claw001", loader.Texture("Arm001/Claw001.png"), colorKey: "claw"))
                    )
                }
            );

            var wing001 = new Part(
                key: "wing001",
                name: "はね001",
                model: new Model("Spine", loader.Model("Wing001.fbx")),
                textures: new LayerVariant[]{
                    new LayerVariant(
                        key: "base",
                        name: "ベースカラー",
                        layer: new Layer(texture: new Image(key:"base001", loader.BaseTexture(), colorKey: "main"))
                    ),
                    new LayerVariant(
                        key: "wingMemberane",
                        name: "よくまく",
                        layer: new Layer(texture: new Image(key:"WingMemberance001", loader.Texture("Wing001/WingMemberance001.png"), colorKey: "accent"))
                    )
                }
            );

            var tail001 = new Part(
                key: "tail001",
                name: "しっぽ001",
                model: new Model("Hips", loader.Model("Tail001.fbx")),
                textures: new LayerVariant[]{
                    new LayerVariant(
                        key: "base",
                        name: "ベースカラー",
                        layer: new Layer(texture: new Image(key:"base001", loader.BaseTexture(), colorKey: "main"))
                    ),
                    new LayerVariant(
                        key: "belly",
                        name: "おなか",
                        layer: new Layer(texture: new Image(key:"belly001", loader.Texture("Tail001/Belly001.png"), colorKey: "sub"))
                    )
                }
            );

            var horn001 = new Part(
                key: "horn001",
                name: "うねる角",
                model: new Model("Head", loader.Model("Horn001.fbx")),
                textures: new LayerVariant[]
                {
                    new LayerVariant(
                        key: "base",
                        name: "ベースカラー",
                        layer: new Layer(texture: new Image(key:"base001", loader.BaseTexture(), colorKey: "main"))
                    ),
                    new LayerVariant(
                        key: "horn",
                        name: "つの",
                        layer: new Layer(texture: new Image(key:"horn001", loader.Texture("Horn001/Horn001.png"), colorKey: "claw"))
                    )
                }
            );

            var horn002 = new Part(
                key: "horn002",
                name: "ねじれ角",
                model: new Model("Head", loader.Model("Horn002.fbx")),
                textures: new LayerVariant[]
                {
                    new LayerVariant(
                        key: "base",
                        name: "ベースカラー",
                        layer: new Layer(texture: new Image(key:"base001", loader.BaseTexture(), colorKey: "main"))
                    ),
                    new LayerVariant(
                        key: "horn",
                        name: "つの",
                        layer: new Layer(texture: new Image(key:"horn001", loader.Texture("Horn001/Horn001.png"), colorKey: "claw"))
                    )
                }
            );

            var horn003 = new Part(
                key: "horn003",
                name: "まっすぐ角",
                model: new Model("Head", loader.Model("Horn003.fbx")),
                textures: new LayerVariant[]
                {
                    new LayerVariant(
                        key: "base",
                        name: "ベースカラー",
                        layer: new Layer(texture: new Image(key:"base001", loader.BaseTexture(), colorKey: "main"))
                    ),
                    new LayerVariant(
                        key: "horn",
                        name: "つの",
                        layer: new Layer(texture: new Image(key:"horn001", loader.Texture("Horn001/Horn001.png"), colorKey: "claw"))
                    )
                }
            );

            var fillet001 = new Part(
                key: "fillet001",
                name: "ひれ001",
                model: new Model("Head", loader.Model("Fillet001.fbx")),
                textures: new LayerVariant[]
                {
                    new LayerVariant(
                        key: "base",
                        name: "ベースカラー",
                        layer: new Layer(texture: new Image(key:"base001", loader.BaseTexture(), colorKey: "main"))
                    ),
                    new LayerVariant(
                        key: "shade",
                        name: "かげ",
                        layer: new Layer(texture: new Image(key:"shade001", loader.Texture("Fillet001/AmbientOcclusion.png"), colorKey: "shade", blendMode: BlendMode.Multiply))
                    )
                }
            );

            var fillet002 = new Part(
                key: "fillet002",
                name: "ひれ002",
                model: new Model("Head", loader.Model("Fillet002.fbx")),
                textures: new LayerVariant[]
                {
                    new LayerVariant(
                        key: "base",
                        name: "ベースカラー",
                        layer: new Layer(texture: new Image(key:"base001", loader.BaseTexture(), colorKey: "main"))
                    ),
                    new LayerVariant(
                        key: "fillet",
                        name: "ヒレ膜",
                        layer: new Layer(texture: new Image(key:"fillet001", loader.Texture("Fillet002/Fillet001.png"), colorKey: "sub"))
                    ),
                    new LayerVariant(
                        key: "shade",
                        name: "かげ",
                        layer: new Layer(texture: new Image(key:"shade001", loader.Texture("Fillet002/AmbientOcclusion001.png"), colorKey: "shade", blendMode: BlendMode.Multiply))
                    )
                }
            );

            var fillet003 = new Part(
                key: "fillet003",
                name: "みみ",
                model: new Model("Head", loader.Model("Fillet003.fbx")),
                textures: new LayerVariant[]
                {
                    new LayerVariant(
                        key: "base",
                        name: "ベースカラー",
                        layer: new Layer(texture: new Image(key:"base001", loader.BaseTexture(), colorKey: "main"))
                    ),
                    new LayerVariant(
                        key: "fillet",
                        name: "内側",
                        layer: new Layer(texture: new Image(key:"fillet001", loader.Texture("Fillet003/Inner.png"), colorKey: "sub"))
                    ),
                    new LayerVariant(
                        key: "shade",
                        name: "かげ",
                        layer: new Layer(texture: new Image(key:"shade001", loader.Texture("Fillet003/AmbientOcclusion.png"), colorKey: "shade", blendMode: BlendMode.Multiply))
                    )
                }
            );

            var head001 = new Part(
                key: "head001",
                name: "あたま001",
                model: new Model("Head", loader.Model("Head001.fbx")),
                textureSize: TextureSize.Large,
                textures: new LayerVariant[]
                {
                    new LayerVariant(
                        key: "base",
                        name: "ベースカラー",
                        layer: new Layer(texture: new Image(key:"base001", loader.BaseTexture(), colorKey: "main"))
                    ),
                    new LayerVariant(
                        key: "facePattern",
                        name: "もよう",
                        variants: new Layer[]
                        {
                            Layer.None,
                            new Layer(name: "もよう1", texture: new Image(key:"facePattern001", loader.Texture("Head001/FacePattern001.png"), colorKey: "sub")),
                            new Layer(name: "もよう2", texture: new Image(key:"facePattern002", loader.Texture("Head001/FacePattern002.png"), colorKey: "sub")),
                            new Layer(name: "もよう3", texture: new Image(key:"facePattern003", loader.Texture("Head001/FacePattern003.png"), colorKey: "sub")),
                        }
                    ),
                    new LayerVariant(
                        key: "eye",
                        name: "目",
                        layer: new Layer(textures: new Image[]
                        {
                            new Image(key:"eye001base", name:"ベースカラー", image: loader.Texture("Head001/Eye001.png"), colorKey: "accent"),
                            new Image(key:"eyeOuter001", name:"目のかげ", image: loader.Texture("Head001/eyeOuter001.png"), color: new UnityEngine.Color(0.009f, 0.013f, 0.03f)),
                            new Image(key:"pupil001", name:"瞳", image: loader.Texture("Head001/Pupil001.png"), color: new UnityEngine.Color(0.1f, 0.1f, 0.1f)),
                        })
                    ),
                    new LayerVariant(
                        key: "sclera",
                        name: "白目",
                        layer: new Layer(texture: new Image(key: "sclera001", name: "白目", image: loader.Texture("Head001/Sclera001.png"), color: new UnityEngine.Color(0.90f, 0.90f, 0.95f)))
                    ),
                    new LayerVariant(
                        key: "eyeHighlight",
                        name: "目のハイライト",
                        layer: new Layer(texture: new Image(key:"eyeHighlight001", name:"ハイライト", image: loader.Texture("Head001/EyeHighlight001.png"), color: new UnityEngine.Color(1, 1, 1)))
                    ),
                    new LayerVariant(
                        key: "eyelash",
                        name: "まつげ",
                        layer: new Layer(texture: new Image(key:"eyelash001", loader.Texture("Head001/Eyelash001.png"), color: new UnityEngine.Color(0.16f, 0.12f, 0.20f)))
                    ),
                    new LayerVariant(
                        key: "mouth",
                        name: "くち",
                        layer: new Layer(texture: new Image(key:"mouth001", loader.Texture("Head001/Mouth001.png"), colorKey: "skin"))
                    ),
                    new LayerVariant(
                        key: "nose",
                        name: "はな",
                        layer: new Layer(texture: new Image(key:"nose001", loader.Texture("Head001/Nose001.png"), colorKey: "outline"))
                    ),
                    new LayerVariant(
                        key: "outline",
                        name: "アウトライン",
                        layer: new Layer(texture: new Image(key:"outline001", loader.Texture("Head001/Outline001.png"), colorKey: "outline"))
                    ),
                    new LayerVariant(
                        key: "shade",
                        name: "かげ",
                        layer: new Layer(texture: new Image(key: "shade001", loader.Texture("Head001/AmbientOcculusion001.png"), colorKey: "shade", blendMode: BlendMode.Multiply))
                    )
                },
                morphs: new Morph[]{
                    new Morph(key: "eye", name: "目", defaultOnly: true, blendShapes: new BlendShapeDefinition[]{
                        new BlendShapeDefinition(key: Constant.NoneKey, name: "ふつう"),
                        new BlendShapeDefinition(key: "CC_Eye1", name: "つりめ"),
                        new BlendShapeDefinition(key: "BLINK", name: "とじめ")
                    })
                },
                children: new PartVariant[]
                {
                    new PartVariant(
                        key: "horn",
                        name: "つの",
                        parts: new Part[]{ Part.None, horn001, horn002, horn003 }
                    ),
                    new PartVariant(
                        key: "fillet",
                        name: "ひれ",
                        parts: new Part[]{ Part.None, fillet001, fillet002, fillet003 }
                    ),
                }
            );

            var body001 = new Part(
                key: "body001",
                name: "からだ001",
                model: new Model("Hips", loader.Model("Body001.fbx")),
                textureSize: TextureSize.Large,
                textures: new LayerVariant[]
                {
                    new LayerVariant(
                        key: "base",
                        name: "ベースカラー",
                        layer: new Layer(texture: new Image(key:"base001", loader.BaseTexture(), colorKey: "main"))
                    ),
                    new LayerVariant(
                        key: "belly",
                        name: "おなか",
                        variants: new Layer[]{
                            Layer.None,
                            new Layer(name: "ひろめ", texture: new Image(key:"wide" ,image: loader.Texture("Body001/Belly001.png"), colorKey: "sub")),
                            new Layer(name: "せまめ", texture: new Image(key:"narrow" ,image: loader.Texture("Body001/Belly002.png"), colorKey: "sub")),
                            new Layer(name: "じゃばら", textures: new Image[]{
                                new Image(key: "bellows001", name: "色1", image: loader.Texture("Body001/Belly002_1.png")),
                                new Image(key: "bellows002", name: "色2", image: loader.Texture("Body001/Belly002_2.png")),
                            })
                        }
                    ),
                    new LayerVariant(
                        key: "toenail",
                        name: "あしヅメ",
                        layer: new Layer(texture: new Image(key:"toenail001", image: loader.Texture("Body001/Toenail001.png"), colorKey: "claw"))
                    ),
                    new LayerVariant(
                        key: "shade",
                        name: "かげ",
                        layer: new Layer(texture: new Image(key:"shade001", image: loader.Texture("Body001/AmbientOcclusion001.png"), colorKey: "shade", blendMode: BlendMode.Multiply))
                    )
                },
                children: new PartVariant[]
                {
                    new PartVariant(
                        key: "head",
                        name: "あたま",
                        parts: new Part[]{ Part.None, head001 }
                    ),
                    new PartVariant(
                        key: "tail",
                        name: "しっぽ",
                        parts: new Part[]{ Part.None, tail001 }
                    ),
                    new PartVariant(
                        key: "wing",
                        name: "はね",
                        parts: new Part[] { Part.None, wing001 }
                    ),
                    new PartVariant(
                        key: "arm",
                        name: "うで",
                        parts: new Part[] { Part.None, arm001 }
                    ),
                }
            );

            return new Config(
                sharder: new Surface(name: "Unlit/Texture"),
                colors: colors,
                parts: new PartVariant(
                    key: "body",
                    name: "からだ",
                    parts: new Part[] { Part.None, body001 }
                )
            );
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(DragonCharaCreator))]
    public class DragonCharaCreatorEditor : CharaCreatorEditor
    { }
#endif
}