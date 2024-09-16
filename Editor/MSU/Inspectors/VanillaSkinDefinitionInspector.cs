using RoR2;
using RoR2.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        protected override void OnEnable()
        {
            base.OnEnable();
            _bodyAddress = serializedObject.FindProperty("_bodyAddress");
            if (_bodyAddress.stringValue.IsNullOrEmptyOrWhiteSpace())
                return;
            UpdateArrays();
        }

        private void UpdateArrays()
        {
            GameObject obj = Addressables.LoadAssetAsync<GameObject>(_bodyAddress.stringValue).WaitForCompletion();
            CharacterModel model = obj.GetComponentInChildren<CharacterModel>();
            List<string> renderers = new List<string>();
            foreach (var rendererInfo in model.baseRendererInfos)
            {
                renderers.Add(rendererInfo.renderer.gameObject.name);
            }
            _rendererNames = renderers.ToArray();

            ChildLocator childLocator = obj.GetComponentInChildren<ChildLocator>();
            List<string> children = new List<string>();
            foreach (var entry in childLocator.transformPairs)
            {
                children.Add(entry.name);
            }
            _childLocatorNames = children.ToArray();
        }

        private void DrawRendererInfos()
        {
            SerializedProperty parentProperty = serializedObject.FindProperty("_rendererInfos");
            EditorGUILayout.PropertyField(parentProperty, false);
            if(!parentProperty.isExpanded)
            {
                return;
            }

            EditorGUI.indentLevel++;
            parentProperty.arraySize = EditorGUILayout.DelayedIntField("Size", parentProperty.arraySize);
            for (int i = 0; i < parentProperty.arraySize; i++)
            {
                var child = parentProperty.GetArrayElementAtIndex(i);

                EditorGUILayout.PropertyField(child, false);
                if (!child.isExpanded)
                    continue;

                EditorGUI.indentLevel++;
                DrawProperty("defaultMaterial", child);
                DrawProperty("defaultShadowCastingMode", child);
                DrawProperty("ignoreOverlays", child);
                DrawProperty("hideOnDeath", child);

                var rendererProp = child.FindPropertyRelative("rendererIndex");
                var intVal = EditorGUILayout.Popup("Renderer", rendererProp.intValue, _rendererNames);
                rendererProp.intValue = intVal;
                EditorGUI.indentLevel--;
            }
            EditorGUI.indentLevel--;
        }

        private void DrawGameObjectActivations()
        {
            SerializedProperty parentProperty = serializedObject.FindProperty("_gameObjectActivations");
            EditorGUILayout.PropertyField(parentProperty, false);
            if (!parentProperty.isExpanded)
                return;

            EditorGUI.indentLevel++;
            parentProperty.arraySize = EditorGUILayout.DelayedIntField("Size", parentProperty.arraySize);
            for(int i = 0; i < parentProperty.arraySize; i++)
            {
                var child = parentProperty.GetArrayElementAtIndex(i);

                EditorGUILayout.PropertyField(child, false);
                if (!child.isExpanded)
                    continue;

                EditorGUI.indentLevel++;
                DrawProperty("shouldActivate", child);
                var isCustom = child.FindPropertyRelative("isCustomActivation");
                EditorGUILayout.PropertyField(isCustom);
                if(isCustom.boolValue)
                {
                    DrawProperty("gameObjectPrefab", child);

                    var childLocatorProp = child.FindPropertyRelative("childName");
                    var intVal = Array.IndexOf(_childLocatorNames, childLocatorProp.stringValue);
                    if (intVal == -1)
                    {
                        intVal = 0;
                    }
                    intVal = EditorGUILayout.Popup("Child Locator Entry", intVal, _childLocatorNames);
                    childLocatorProp.stringValue = _childLocatorNames[intVal];

                    DrawProperty("localPos", child);
                    DrawProperty("localAngles", child);
                    DrawProperty("localScale", child);
                }
                else
                {
                    var rendererProp = child.FindPropertyRelative("rendererIndex");
                    var intVal = EditorGUILayout.Popup("Renderer", rendererProp.intValue, _rendererNames);
                    rendererProp.intValue = intVal;
                }
                EditorGUI.indentLevel--;
            }
            EditorGUI.indentLevel--;
        }

        private void DrawMeshReplacements()
        {
            SerializedProperty parentProperty = serializedObject.FindProperty("_meshReplacements");
            EditorGUILayout.PropertyField(parentProperty, false);
            if (!parentProperty.isExpanded)
                return;

            EditorGUI.indentLevel++;
            parentProperty.arraySize = EditorGUILayout.DelayedIntField("Size", parentProperty.arraySize);
            for (int i = 0; i < parentProperty.arraySize; i++)
            {
                var child = parentProperty.GetArrayElementAtIndex(i);

                EditorGUILayout.PropertyField(child, false);
                if (!child.isExpanded)
                    continue;

                EditorGUI.indentLevel++;
                DrawProperty("mesh", child);
                var rendererProp = child.FindPropertyRelative("rendererIndex");
                var intVal = EditorGUILayout.Popup("Renderer", rendererProp.intValue, _rendererNames);
                rendererProp.intValue = intVal;
                EditorGUI.indentLevel--;
            }
            EditorGUI.indentLevel--;
        }

        private void DrawProperty(string propName)
        {
            DrawProperty(serializedObject.FindProperty(propName));
        }

        private void DrawProperty(string propName, SerializedProperty parentProperty)
        {
            DrawProperty(parentProperty.FindPropertyRelative(propName));
        }
        private void DrawProperty(SerializedProperty property)
        {
            EditorGUILayout.PropertyField(property);
        }

        protected override void DrawIMGUI()
        {
            EditorGUI.BeginChangeCheck();
            DrawProperty("_bodyAddress");
            if (EditorGUI.EndChangeCheck())
                UpdateArrays();

            DrawProperty("_displayAddress");
            EditorGUILayout.Space(5);

            if (serializedObject.FindProperty("_bodyAddress").stringValue.IsNullOrEmptyOrWhiteSpace())
            {
                EditorGUILayout.LabelField("Please input a body address.");
                return;
            }
            DrawProperty("icon");
            DrawProperty("nameToken");
            DrawProperty("unlockableDef");
            DrawProperty("_baseSkins");

            DrawRendererInfos();
            DrawGameObjectActivations();
            DrawMeshReplacements();

            DrawProperty("_projectileGhostReplacements");
            DrawProperty("_minionSkinReplacements");

            serializedObject.ApplyModifiedProperties();
        }
    }
}