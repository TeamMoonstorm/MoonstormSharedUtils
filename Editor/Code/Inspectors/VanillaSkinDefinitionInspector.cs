﻿using RoR2;
using RoR2EditorKit;
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
    [CustomEditor(typeof(VanillaSkinDefinition))]
    public class VanillaSkinDefinitionInspector : UnityEditor.Editor
    {
        private VanillaSkinDefinition _target;

        private SerializedProperty _bodyAddress;
        private string[] rendererNames = Array.Empty<string>();
        private string[] childLocatorNames = Array.Empty<string>();

        private void OnEnable()
        {
            _target = (VanillaSkinDefinition)target;
            _bodyAddress = serializedObject.FindProperty("bodyAddress");
            if (_bodyAddress.stringValue.IsNullOrEmptyOrWhitespace())
                return;
            UpdateArrays();
        }

        public override void OnInspectorGUI()
        {
            IMGUIUtil.DrawCheckableProperty(serializedObject.FindProperty("bodyAddress"), UpdateArrays);
            DrawProperty("displayAddress");
            EditorGUILayout.Space(5);

            if(serializedObject.FindProperty("bodyAddress").stringValue.IsNullOrEmptyOrWhitespace())
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

        private void UpdateArrays()
        {
            GameObject obj = Addressables.LoadAssetAsync<GameObject>(_bodyAddress.stringValue).WaitForCompletion();
            CharacterModel model = obj.GetComponentInChildren<CharacterModel>();
            List<string> renderers = new List<string>();
            foreach (var rendererInfo in model.baseRendererInfos)
            {
                renderers.Add(rendererInfo.renderer.gameObject.name);
            }
            rendererNames = renderers.ToArray();

            ChildLocator childLocator = obj.GetComponentInChildren<ChildLocator>();
            List<string> children = new List<string>();
            foreach (var entry in childLocator.transformPairs)
            {
                children.Add(entry.name);
            }
            childLocatorNames = children.ToArray();
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

                var rendererProp = child.FindPropertyRelative("renderer");
                var intVal = EditorGUILayout.Popup("Renderer", rendererProp.intValue, rendererNames);
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
                    DrawProperty("customObject", child);

                    var childLocatorProp = child.FindPropertyRelative("childLocatorEntry");
                    var intVal = Array.IndexOf(childLocatorNames, childLocatorProp.stringValue);
                    if (intVal == -1)
                    {
                        intVal = 0;
                    }
                    intVal = EditorGUILayout.Popup("Child Locator Entry", intVal, childLocatorNames);
                    childLocatorProp.stringValue = childLocatorNames[intVal];

                    DrawProperty("localPos", child);
                    DrawProperty("localAngles", child);
                    DrawProperty("localScale", child);
                }
                else
                {
                    var rendererProp = child.FindPropertyRelative("renderer");
                    var intVal = EditorGUILayout.Popup("Renderer", rendererProp.intValue, rendererNames);
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
                DrawProperty("newMesh", child);
                var rendererProp = child.FindPropertyRelative("renderer");
                var intVal = EditorGUILayout.Popup("Renderer", rendererProp.intValue, rendererNames);
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

    }
}