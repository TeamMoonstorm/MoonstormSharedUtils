using RoR2;
using RoR2.Editor;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace MSU.Editor.Inspectors
{
    [CustomEditor(typeof(VanillaSkinDef))]
    public class VanillaSkinDefInspector : IMGUIScriptableObjectInspector<VanillaSkinDef>
    {
        private SerializedProperty _bodyAddress;
        private string[] _rendererNames = Array.Empty<string>();
        private string[] _childLocatorNames = Array.Empty<string>();

        protected override void DrawIMGUI()
        {
            if(GUILayout.Button("Migrate to UberSkinDef"))
            {
                PromptMigration();
            }

            EditorGUI.BeginDisabledGroup(true);
            DrawDefaultInspector();
            EditorGUI.EndDisabledGroup();
        }

        private void PromptMigration()
        {
            Rect rect = new Rect();
            rect.height = EditorGUIUtility.singleLineHeight * 10;
            rect.width = EditorGUIUtility.currentViewWidth;
            Vector2 mousePos = Event.current.mousePosition;
            rect.position = mousePos;
            PopupWindow.Show(rect, new PopupContent(rect.size, this));
        }

        private void Migrate()
        {

        }

        private class PopupContent : PopupWindowContent
        {
            bool changeToSkinDef = true;
            bool autoCreateSkinDefParams = true;

            Vector2 size;
            VanillaSkinDefInspector creator;
            public override void OnGUI(Rect rect)
            {
                EditorGUILayout.HelpBox("This will migrate your VanillaSkinDef into the all in one UberSkinDef. Check each field's tooltip for information.", MessageType.Info);

                GUIContent content = new GUIContent();
                content.text = "Change to SkinDef";
                content.tooltip = "Changes the VanillaSkinDef into a regular SkinDef, while keeping all references intact. Extremely recommended";
                EditorGUILayout.Toggle(content, changeToSkinDef);

                content.text = "Auto Create SkinDefParams";
                content.tooltip = "Automatically creates a SkinDefParams object to move all the data into, it'll be created as a sibling asset of the SkinDef";
                EditorGUILayout.Toggle(content, autoCreateSkinDefParams);

                GUILayout.Space(10);
                if(GUILayout.Button("Migrate!"))
                {
                    creator.Migrate();
                }
            }

            public override Vector2 GetWindowSize()
            {
                return size;
            }

            public PopupContent(Vector2 size, VanillaSkinDefInspector creator)
            {
                this.size = size;
                this.creator = creator;
            }
        }
    }
}