using UnityEditor.Formats.Fbx.Exporter;
using System;
using System.Linq;
using System.Reflection;

namespace Mushus.CharaCreatorV0
{
    internal class Exporter
    {
        // https://forum.unity.com/threads/fbx-exporter-binary-export-doesnt-work-via-editor-scripting.1114222/
        internal static void ExportBinaryFBX(string filePath, UnityEngine.Object singleObject)
        {
            // Find relevant internal types in Unity.Formats.Fbx.Editor assembly
            Type[] types = AppDomain.CurrentDomain.GetAssemblies().First(x => x.FullName == "Unity.Formats.Fbx.Editor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null").GetTypes();
            Type optionsInterfaceType = types.First(x => x.Name == "IExportOptions");
            Type optionsType = types.First(x => x.Name == "ExportOptionsSettingsSerializeBase");

            // Instantiate a settings object instance
            MethodInfo optionsProperty = typeof(ModelExporter).GetProperty("DefaultOptions", BindingFlags.Static | BindingFlags.NonPublic).GetGetMethod(true);
            object optionsInstance = optionsProperty.Invoke(null, null);

            // Change the export setting from ASCII to binary
            FieldInfo exportFormatField = optionsType.GetField("exportFormat", BindingFlags.Instance | BindingFlags.NonPublic);
            exportFormatField.SetValue(optionsInstance, 1);

            // Invoke the ExportObject method with the settings param
            MethodInfo exportObjectMethod = typeof(ModelExporter).GetMethod("ExportObject", BindingFlags.Static | BindingFlags.NonPublic, Type.DefaultBinder, new Type[] { typeof(string), typeof(UnityEngine.Object), optionsInterfaceType }, null);
            exportObjectMethod.Invoke(null, new object[] { filePath, singleObject, optionsInstance });
        }
    }
}