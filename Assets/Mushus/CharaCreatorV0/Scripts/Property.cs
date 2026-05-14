
using System;
using UnityEngine;

namespace Mushus.CharaCreatorV0
{
    [Serializable]
    public class PartProperty
    {
        [SerializeField] public int index;
        [SerializeField] public string path;
    }

    [Serializable]
    public class LayerProperty
    {
        [SerializeField] public int index;
        [SerializeField] public string path;
    }

    [Serializable]
    public class TextureProperty
    {
        [SerializeField] public bool IsSelectedColor;
        [SerializeField] public Color color;
        [SerializeField] public string path;
    }

    [Serializable]
    public class ColorProperty
    {
        [SerializeField] public string Key;
        [SerializeField] public Color Color;
    }

    [Serializable]
    public class MorphProperty
    {
        [SerializeField] public string Key;
        [SerializeField] public int Index;
        [SerializeField] public float Value;
    }
}

#if UNITY_EDITOR

#endif