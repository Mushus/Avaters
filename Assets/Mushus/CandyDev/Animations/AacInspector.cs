// このファイルはAnimation as Codeの定義ファイルです。
// 編集を想定していません

#if UNITY_EDITOR
using System;
using AnimatorAsCode.V0;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using static UnityEditor.Progress;
using AnimatorController = UnityEditor.Animations.AnimatorController;

namespace Mushus.AAC.FX
{
    public static class AacInspector
    {
        public static void InspectorTemplate(Editor editor, SerializedObject serializedObj, string propName, Action createFn, Action removeFnOptional = null)
        {
            var prop = serializedObj.FindProperty(propName);
            if (prop.stringValue.Trim() == "")
            {
                prop.stringValue = GUID.Generate().ToString();
                serializedObj.ApplyModifiedProperties();
            }

            editor.DrawDefaultInspector();

            if (GUILayout.Button("Create"))
            {
                createFn.Invoke();
            }
            if (removeFnOptional != null && GUILayout.Button("Remove"))
            {
                removeFnOptional.Invoke();
            }
        }

        public static AacFlBase AnimatorAsCode(string systemName, VRCAvatarDescriptor avatar, AnimatorController assetContainer, string assetKey)
        {
            var aac = AacV0.Create(new AacConfiguration
            {
                SystemName = systemName,
                // In the examples, we consider the avatar to be also the animator root.
                AvatarDescriptor = avatar,
                // You can set the animator root to be different than the avatar descriptor,
                // if you want to apply an animator to a different avatar without redefining
                // all of the game object references which were relative to the original avatar.
                AnimatorRoot = avatar.transform,
                // DefaultValueRoot is currently unused in AAC. It is added here preemptively
                // in order to define an avatar root to sample default values from.
                // The intent is to allow animators to be created with Write Defaults OFF,
                // but mimicking the behaviour of Write Defaults ON by automatically
                // sampling the default value from the scene relative to the transform
                // defined in DefaultValueRoot.
                DefaultValueRoot = avatar.transform,
                AssetContainer = assetContainer,
                AssetKey = assetKey,
                DefaultsProvider = new AacDefaultsProvider(writeDefaults: false)
            });
            aac.ClearPreviousAssets();
            return aac;
        }
    }
}
#endif
