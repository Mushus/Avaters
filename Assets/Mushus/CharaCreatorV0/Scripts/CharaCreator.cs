using UnityEngine;
using UnityEditor;
using System;
using System.Linq;

namespace Mushus.CharaCreatorV0
{
    public abstract class CharaCreator : MonoBehaviour
    {
        private Debounce _previewFuncion = new Debounce();
        private Config _configCache;
        public abstract Config GetConfig();
        public Config Config
        {
            get
            {
                if (_configCache == null)
                {
                    _configCache = GetConfig();
                }
                return _configCache;
            }
        }
        [SerializeField] public ColorProperty[] Colors;
        [SerializeField] public PartProperty[] Parts;
        [SerializeField] public LayerProperty[] Layers;
        [SerializeField] public TextureProperty[] Textures;
        [SerializeField] public MorphProperty[] Morphs;


        void Start()
        {

        }

        void Update()
        {

        }

        void Reset()
        {
            var config = Config;
            Colors = config.Colors.Select(color => new ColorProperty()
            {
                Key = color.Key,
                Color = color.Value,
            }).ToArray();

            Parts = new PartProperty[] { };
            Layers = new LayerProperty[] { };
            Textures = new TextureProperty[] { };
            Morphs = new MorphProperty[] { };
        }

        void OnValidate()
        {
            _previewFuncion.Run(Preview, 0.1f);
        }

        void Awake()
        {
            var config = Config;
            Colors = config.Colors.Select(color =>
            {
                var colorProp = Colors.FirstOrDefault(c => c.Key == color.Key);
                return colorProp ?? new ColorProperty()
                {
                    Key = color.Key,
                    Color = color.Value,
                };
            }).ToArray();

            if (Parts == null)
            {
                Parts = new PartProperty[] { };
            }
            if (Layers == null)
            {
                Layers = new LayerProperty[] { };
            }
            if (Textures == null)
            {
                Textures = new TextureProperty[] { };
            }
        }

        public Color? GetDefaultColor(string colorKey)
        {
            var colorProp = Colors.FirstOrDefault(c => c.Key == colorKey);
            return colorProp != null ? colorProp.Color as Color? : null;
        }

        internal LayerProperty FindLayer(string path)
        {
            var layerProp = Layers.FirstOrDefault(l => l.path == path);
            if (layerProp != null) return layerProp;
            return new LayerProperty()
            {
                path = path,
                index = 0,
            };
        }

        internal MorphProperty FindMorph(string key)
        {
            var morphProp = Morphs.FirstOrDefault(m => m.Key == key);
            if (morphProp != null) return morphProp;
            return new MorphProperty()
            {
                Key = key,
                Index = 0,
                Value = 0,
            };
        }

        internal void Preview()
        {
            ModelBuilder.Preview(this);
        }
    }

#if UNITY_EDITOR
    public abstract class CharaCreatorEditor : Editor
    {
        private const string _partsControlPrefix = "part:";
        private CharaCreator _component;
        private Config _config;

        /** プロパティ */
        private SerializedProperty _colors;
        private SerializedProperty _parts;
        private SerializedProperty _layers;
        private SerializedProperty _textures;
        private SerializedProperty _morphs;

        private string _focusPartPath = "";

        public CharaCreatorEditor()
        {
        }

        void OnEnable()
        {
            _component = target as CharaCreator;
            _config = _component.GetConfig();
            _colors = serializedObject.FindProperty(nameof(CharaCreator.Colors));
            _parts = serializedObject.FindProperty(nameof(CharaCreator.Parts));
            _layers = serializedObject.FindProperty(nameof(CharaCreator.Layers));
            _textures = serializedObject.FindProperty(nameof(CharaCreator.Textures));
            _morphs = serializedObject.FindProperty(nameof(CharaCreator.Morphs));

            serializedObject.ApplyModifiedProperties();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            CustomGUI.Heading("基本設定");
            using (CustomGUI.Group())
            {
                foreach (var color in _config.Colors)
                {
                    var colorProp = FindColorProperty(color.Key);
                    var colorValueProp = colorProp.FindPropertyRelative(nameof(ColorProperty.Color));
                    colorValueProp.colorValue = EditorGUILayout.ColorField(color.Name, colorValueProp.colorValue);
                }
            }

            CustomGUI.Heading("パーツ");
            using (CustomGUI.Group())
            {
                PartSelect(_config.Parts);
            }

            CustomGUI.SpaceM();
            CustomGUI.Heading("パーツ詳細");

            var focusPart = GetFocusPart();
            // TODO: partinfoのPathを消す
            var partPropInfo = GetPartPropertyByPath(focusPart);
            if (partPropInfo != null)
            {

                PartDetail(focusPart, partPropInfo.def);
            }

            CustomGUI.Heading("出力");
            using (CustomGUI.Group())
            {
                if (GUILayout.Button("書き出し"))
                {
                    ModelBuilder.Build(_component);
                }
            }
            serializedObject.ApplyModifiedProperties();
        }

        private void PartSelect(PartVariant variant, string path = null)
        {
            var variantPath = path != null ? $"{path}/{variant.Key}" : variant.Key;
            var partsPath = $"{_partsControlPrefix}{variantPath}";
            var parts = variant.parts;
            var partProperty = FindPartProperty(variantPath);
            var indexProp = partProperty.FindPropertyRelative(nameof(PartProperty.index));
            var displayOptions = parts.Select(part => part.Name).ToArray();

            GUI.SetNextControlName(partsPath);
            indexProp.intValue = EditorGUILayout.Popup(variant.Name, indexProp.intValue, displayOptions);
            if (parts.Length <= indexProp.intValue)
            {
                indexProp.intValue = 0;
            }

            using (CustomGUI.Indent())
            {
                var partDef = parts[indexProp.intValue];
                foreach (var childVariant in partDef.Children)
                {
                    PartSelect(childVariant, variantPath);
                }
            }
        }

        private void PartDetail(string partPath, Part partDef)
        {
            if (partDef.Morphs != null && partDef.Morphs.Length > 0)
            {
                EditorGUILayout.LabelField("見た目:");
                using (CustomGUI.Group())
                {
                    for (var i = 0; i < partDef.Morphs.Length; i++)
                    {
                        if (i != 0) CustomGUI.SpaceM();
                        var morph = partDef.Morphs[i];
                        MorphDetail(partPath, morph);
                    }
                }
                CustomGUI.SpaceM();
            }

            if (partDef.Textures != null)
            {
                EditorGUILayout.LabelField("色や模様:");
                using (CustomGUI.Group())
                {
                    var hasSibling = partDef.Textures.Length > 1;
                    for (var i = 0; i < partDef.Textures.Length; i++)
                    {
                        if (i != 0) CustomGUI.SpaceM();

                        var layerVariant = partDef.Textures[i];
                        VariantDetail(partPath, layerVariant, hasSibling);
                    }
                }
                CustomGUI.SpaceM();
            }
        }

        private void VariantDetail(string partPath, LayerVariant layerVariant, bool hasSibling)
        {
            var layerProp = FindLayerProperty(partPath, layerVariant.Key);
            var index = layerProp.FindPropertyRelative(nameof(LayerProperty.index));
            var layerDef = layerVariant.Variants[index.intValue];
            // なにかの問題でインデックスが選択範囲外のときは元に戻す
            if (layerVariant.Variants.Length <= index.intValue)
            {
                index.intValue = 0;
            }

            bool hasTextureSibling;
            string label = null;
            var indent = 0;
            if (layerVariant.Variants.Length > 1)
            {
                // バリアントが複数ある場合はポップアップがラベル代わり
                var displayOptions = layerVariant.Variants.Select(texture => texture.Name).ToArray();
                index.intValue = EditorGUILayout.Popup(layerVariant.Name, index.intValue, displayOptions);
                indent = 1;
                // ラベルを表示するのでテクスチャの数で兄弟がいるかどうかがわかる
                hasTextureSibling = layerDef.Textures.Length > 1;
            }
            else if (layerDef.Textures.Length > 1)
            {
                // テクスチャが複数ある場合はラベルを表示
                EditorGUILayout.LabelField(layerVariant.Name);
                indent = 1;
                // テクスチャが１以上あるので兄弟がいる
                hasTextureSibling = true;
            }
            else
            {
                // ラベルを表示しないかわりに子にラベル表示を委託
                label = layerVariant.Name;
                // ラベルがないので親と子供をあわせて兄弟の状態がわかる
                hasTextureSibling = hasSibling || layerDef.Textures.Length > 1;
            }

            using (CustomGUI.Indent(indent)) foreach (var textureDef in layerDef.Textures)
                {
                    TextureDetail(partPath, textureDef, label, hasTextureSibling);
                }
        }

        private void TextureDetail(string partPath, Image textureDef, string label, bool hasSibling)
        {
            bool hasFormSibiling;
            var indent = 0;
            if (!IsOnlyColorForm(textureDef))
            {
                EditorGUILayout.LabelField(label ?? textureDef.Name);
                if (label != null) label = textureDef.Name;
                indent = 1;
                // カラーフォーム以外もあるので兄弟がいる
                hasFormSibiling = false;
            }
            else
            {
                if (label == null) label = textureDef.Name;
                // カラーフォームしかなくラベルもないので、兄弟がいるかどうかは親の状態による
                hasFormSibiling = hasSibling;
            }

            using (CustomGUI.Indent(indent))
            {
                var textureProp = FindTextureProperty(partPath, textureDef.Key);
                var isSelectedColor = textureProp.FindPropertyRelative(nameof(TextureProperty.IsSelectedColor));
                var color = textureProp.FindPropertyRelative(nameof(TextureProperty.color));

                var defaultColor = GetDefaultColor(textureDef.ColorKey) ?? (textureDef.Color != null ? (textureDef.Color) : Color.red);
                (color.colorValue, isSelectedColor.boolValue) = ColorField(!hasFormSibiling || label == null ? "カラー" : label, color.colorValue, isSelectedColor.boolValue, defaultColor);
            }
        }

        private void MorphDetail(string partPath, Morph morph)
        {
            var morphProp = FindMorphProperty(partPath, morph.Key);
            var keyProp = morphProp.FindPropertyRelative(nameof(MorphProperty.Key));

            if (morph.BlendShapes.Length == 1)
            {
                EditorGUILayout.LabelField(morph.Name);
            }
            else
            {
                var IndexProp = morphProp.FindPropertyRelative(nameof(MorphProperty.Index));
                if (morph.BlendShapes.Length <= IndexProp.intValue) IndexProp.intValue = 0;

                var displayOptions = morph.BlendShapes.Select(m => m.Name).ToArray();
                IndexProp.intValue = EditorGUILayout.Popup(morph.Name, IndexProp.intValue, displayOptions);
            }

            using (CustomGUI.Indent())
            {
                var valueProp = morphProp.FindPropertyRelative(nameof(MorphProperty.Value));
                valueProp.floatValue = EditorGUILayout.Slider("値", valueProp.floatValue, 0, 1);
            }
        }

        private bool IsOnlyColorForm(Image textureDef)
        {
            return true;
        }

        private (Color, bool) ColorField(string label, Color color, bool isSelected, Color defaultColor)
        {
            var rect = EditorGUILayout.GetControlRect();
            var labelWidth = EditorGUIUtility.labelWidth - EditorGUI.indentLevel * Constant.IndentPerLevel;
            var labelRect = new Rect(rect.x, rect.y, labelWidth, rect.height);
            EditorGUI.LabelField(labelRect, label);
            if (isSelected)
            {
                const int buttonWidth = 24;
                var colorRect = new Rect(labelRect.xMax, rect.y, rect.width - labelWidth - buttonWidth, rect.height);
                var buttonRect = new Rect(colorRect.xMax, rect.y, buttonWidth, rect.height);
                EditorGUI.LabelField(labelRect, label);
                color = EditorGUI.ColorField(colorRect, color);
                if (GUI.Button(buttonRect, "x"))
                {
                    isSelected = false;
                }
            }
            else
            {
                var buttonX = EditorGUIUtility.labelWidth + 23;
                var buttonW = rect.width - EditorGUIUtility.labelWidth - 2;
                var buttonRect = new Rect(buttonX, rect.y, buttonW, rect.height);
                var guiColor = GUI.color;
                GUI.color = defaultColor;
                if (GUI.Button(buttonRect, "色選択", CustomGUI.WhiteBox))
                {
                    isSelected = true;
                    color = defaultColor;
                }
                GUI.color = guiColor;
            }
            return (color, isSelected);
        }

        private SerializedProperty FindColorProperty(string key)
        {
            var index = Array.FindIndex(_component.Colors, c => c.Key == key);
            if (index >= 0)
            {
                return _colors.GetArrayElementAtIndex(index);
            }

            var defaultColor = _config.Colors.FirstOrDefault(c => c.Key == key).Value;

            var insertIndex = _colors.arraySize;
            _colors.InsertArrayElementAtIndex(insertIndex);

            var childProperty = _colors.GetArrayElementAtIndex(insertIndex);
            childProperty.FindPropertyRelative(nameof(ColorProperty.Key)).stringValue = key;
            childProperty.FindPropertyRelative(nameof(ColorProperty.Color)).colorValue = defaultColor;
            return childProperty;
        }

        private SerializedProperty FindPartProperty(string path)
        {
            var index = Array.FindIndex(_component.Parts, p => p.path == path);
            if (index >= 0)
            {
                return _parts.GetArrayElementAtIndex(index);
            }

            var insertIndex = _parts.arraySize;
            _parts.InsertArrayElementAtIndex(insertIndex);

            var childProperty = _parts.GetArrayElementAtIndex(insertIndex);
            childProperty.FindPropertyRelative(nameof(PartProperty.path)).stringValue = path;
            return childProperty;
        }

        private SerializedProperty FindLayerProperty(string path, string layerKey)
        {
            var key = $"{path}//{layerKey}";
            var index = Array.FindIndex(_component.Layers, l => l.path == key);
            if (index >= 0)
            {
                return _layers.GetArrayElementAtIndex(index);
            }

            var insertIndex = _layers.arraySize;
            _layers.InsertArrayElementAtIndex(insertIndex);

            var childProperty = _layers.GetArrayElementAtIndex(insertIndex);
            childProperty.FindPropertyRelative(nameof(LayerProperty.path)).stringValue = key;
            childProperty.FindPropertyRelative(nameof(LayerProperty.index)).intValue = 0;
            return childProperty;
        }

        private SerializedProperty FindTextureProperty(string path, string textureKey)
        {
            var key = $"{path}//{textureKey}";
            var index = Array.FindIndex(_component.Textures, t => t.path == key);
            if (index >= 0)
            {
                return _textures.GetArrayElementAtIndex(index);
            }

            var insertIndex = _textures.arraySize;
            _textures.InsertArrayElementAtIndex(insertIndex);

            var childProperty = _textures.GetArrayElementAtIndex(insertIndex);
            childProperty.FindPropertyRelative(nameof(TextureProperty.path)).stringValue = key;
            childProperty.FindPropertyRelative(nameof(TextureProperty.color)).colorValue = Color.red;
            return childProperty;
        }

        private SerializedProperty FindMorphProperty(string path, string morphKey)
        {
            var key = $"{path}//{morphKey}";
            var index = Array.FindIndex(_component.Morphs, m => m.Key == key);
            if (index >= 0)
            {
                return _morphs.GetArrayElementAtIndex(index);
            }

            var insertIndex = _morphs.arraySize;
            _morphs.InsertArrayElementAtIndex(insertIndex);

            var childProperty = _morphs.GetArrayElementAtIndex(insertIndex);
            childProperty.FindPropertyRelative(nameof(MorphProperty.Key)).stringValue = key;
            childProperty.FindPropertyRelative(nameof(MorphProperty.Index)).intValue = 0;
            childProperty.FindPropertyRelative(nameof(MorphProperty.Value)).floatValue = 0;
            return childProperty;
        }

        private string GetFocusPart()
        {
            var controlName = GUI.GetNameOfFocusedControl();
            if (controlName.StartsWith(_partsControlPrefix))
            {
                _focusPartPath = controlName.Remove(0, _partsControlPrefix.Length);
            }


            return _focusPartPath;
        }

        class PartPropertyInfo
        {
            public Part def;
            public SerializedProperty prop;
        }

        private PartPropertyInfo GetPartPropertyByPath(string path)
        {
            PartVariant[] variants = new PartVariant[] { _config.Parts };
            var spritedPath = path.Split('/');
            if (spritedPath.Length == 0)
            {
                return null;
            }
            string currentPath = null;
            SerializedProperty partProp = null;
            Part part = null;
            foreach (var dir in spritedPath)
            {
                var variant = variants.FirstOrDefault(child => child.Key == dir);
                if (variant == null) return null;
                currentPath = currentPath == null ? dir : $"{currentPath}/{dir}";
                partProp = FindPartProperty(currentPath);
                var index = partProp.FindPropertyRelative(nameof(PartProperty.index)).intValue;
                part = variant.parts[index];
                variants = part.Children;
            }

            return new PartPropertyInfo()
            {
                def = part,
                prop = partProp,
            };
        }

        private Color? GetDefaultColor(string colorKey)
        {
            return _component.GetDefaultColor(colorKey);
        }
    }
#endif
}
