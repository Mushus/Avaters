using UnityEngine;
using UnityEditor;
using System;

namespace Mushus.CharaCreatorV0
{
    internal class CustomGUI
    {
        internal static GUIStyle WhiteBox = new GUIStyle("WhiteBackground")
        {
            alignment = TextAnchor.MiddleCenter
        };
        internal static GUIStyle headingStyle
        {
            get
            {
                return new GUIStyle("ShurikenModuleTitle")
                {
                    font = (new GUIStyle("Label")).font,
                    border = new RectOffset(8, 7, 4, 4),
                    fixedHeight = 24,
                    contentOffset = new Vector2(20, -2),
                };
            }
        }

        internal class GroupScope : IDisposable
        {
            internal GroupScope()
            {
                GUILayout.BeginVertical("box");
            }

            public void Dispose()
            {
                GUILayout.EndVertical();
            }
        }

        internal static GroupScope Group()
        {
            return new GroupScope();
        }

        internal class IndentScope : IDisposable
        {
            private int _level;
            internal IndentScope(int level = 1)
            {
                _level = level;
                EditorGUI.indentLevel += _level;
            }

            public void Dispose()
            {
                EditorGUI.indentLevel -= _level;
            }
        }

        internal static IndentScope Indent(int level = 1)
        {
            return new IndentScope(level);
        }

        internal static void SpaceL()
        {
            EditorGUILayout.Space(32);
        }

        internal static void SpaceM()
        {
            EditorGUILayout.Space(8);
        }

        internal static void Heading(string label)
        {
            using (new EditorGUILayout.HorizontalScope(headingStyle))
            {
                EditorGUILayout.LabelField(label);
            }
            SpaceM();
        }
    }
}