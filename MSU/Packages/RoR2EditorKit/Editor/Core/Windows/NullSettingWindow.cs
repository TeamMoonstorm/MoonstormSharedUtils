using RoR2EditorKit.Settings;
using System.Collections.Generic;
using ThunderKit.Core.Windows;
using UnityEditor;
using UnityEngine;

namespace RoR2EditorKit.Core.Windows
{
    public class NullShaderPairWindow : EditorWindow
    {
        public List<string> nullPairs;
        public static void Create(List<string> nullShaderPairs)
        {
            NullShaderPairWindow window = GetWindow<NullShaderPairWindow>($"You have Shader pairs that are null!");
            window.position = new Rect(Screen.width / 2, Screen.height / 2, 650, 150);
            window.nullPairs = nullShaderPairs;
            window.ShowPopup();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField($"It appears you have a total of {nullPairs.Count} shader pairs that are null...");
            EditorGUILayout.LabelField($"Please go to \"Tools/Thunderkit/Settings\" and fill the missing shader pairs in the {nameof(MaterialEditorSettings)} settings.");
            EditorGUILayout.LabelField($"This popup will appear every time unity reloads until the null references are fixed.");

            GUILayout.Space(30);
            if (GUILayout.Button("Ok")) Close();
            if (GUILayout.Button("Open Settings & Close"))
            {
                SettingsWindow.ShowSettings();
                Close();
            }
        }
    }
}
