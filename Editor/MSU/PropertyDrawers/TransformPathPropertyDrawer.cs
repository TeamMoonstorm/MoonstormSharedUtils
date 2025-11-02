using R2API.AddressReferencedAssets;
using RoR2;
using RoR2.Editor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace MSU.Editor.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(TransformPathAttribute))]
    public class TransformPathAttributePropertyDrawer : IMGUIPropertyDrawer<TransformPathAttribute>
    {
        Transform _cachedRootTransform;
        bool isRootTransformFromAddress;
        protected override void DrawIMGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Transform rootTransform = GetRootTransform(propertyDrawerData.rootObjectProperty, propertyDrawerData.rootComponentType);
            if(!rootTransform)
            {
                EditorGUI.LabelField(position, $"Could not find a valid Root Transform.");
            }
            label.tooltip = $"The Transform that's being referenced";
            var prefixRect = EditorGUI.PrefixLabel(position, label);

            if (EditorGUI.DropdownButton(prefixRect, CreateDropdownContent(property), FocusType.Passive, EditorStyles.text))
            {
                Type componentType = GetRequiredComponentType(property, propertyDrawerData.siblingPropertyComponentTypeRequirement);
                bool allowSelectingRoot = propertyDrawerData.allowSelectingRoot;
                var dropdown = new TransformPathDropdown(new AdvancedDropdownState(), rootTransform, true, allowSelectingRoot, position, componentType);
                dropdown.onItemSelected += (item) =>
                {
                    if(!item.transform)
                    {
                        property.stringValue = string.Empty;
                    }
                    else
                    {
                        property.stringValue = Util.BuildPrefabTransformPath(rootTransform, item.transform, false, allowSelectingRoot);
                    }
                    property.serializedObject.ApplyModifiedProperties();
                };
                dropdown.Show(position);
            }
        }

        private GUIContent CreateDropdownContent(SerializedProperty property)
        {
            var result = new GUIContent();
            if(property.stringValue.IsNullOrEmptyOrWhiteSpace())
            {
                result.text = "None";
            }
            else
            {
                string stringValue = property.stringValue;
                string text = stringValue;
                string tooltip = stringValue;
                int lastIndexOfForwardSlash = stringValue.LastIndexOf('/');
                if(lastIndexOfForwardSlash != -1)
                {
                    text = stringValue.Substring(lastIndexOfForwardSlash + 1);
                }
                result.text = text;
                result.tooltip = tooltip;
            }
            return result;
        }

        private Transform GetRootTransform(string rootObjectPropertyName, Type rootComponentType)
        {
            if (_cachedRootTransform)
                return _cachedRootTransform;

            SerializedProperty rootObjectProperty = serializedObject.FindProperty(rootObjectPropertyName);
            if (rootObjectProperty == null)
                return null;

            //The Attribute is on a GameObject field
            if(rootObjectProperty.propertyType == SerializedPropertyType.ObjectReference && rootObjectProperty.objectReferenceValue && rootObjectProperty.objectReferenceValue is GameObject directReference0)
            {
                _cachedRootTransform = GetProperTransform(directReference0.transform, rootComponentType);
                return _cachedRootTransform;
            }

            ///The Attribute is on an AddressReferencedPrefab which has a valid _asset field
            SerializedProperty assetReferencePrefab_asset = rootObjectProperty.FindPropertyRelative("_asset");
            if(assetReferencePrefab_asset != null
                && assetReferencePrefab_asset.propertyType == SerializedPropertyType.ObjectReference
                && assetReferencePrefab_asset.objectReferenceValue
                && assetReferencePrefab_asset.objectReferenceValue is GameObject directReference1)
            {
                _cachedRootTransform = GetProperTransform(directReference1.transform, rootComponentType);
                return _cachedRootTransform;
            }

            //The Attribute is on an AddressReferencedPrefab which has a valid address value.
            SerializedProperty assetReferencePrefab_address = rootObjectProperty.FindPropertyRelative("_address");
            if(assetReferencePrefab_address != null)
            {
                var guid = assetReferencePrefab_address.stringValue;
                if(!string.IsNullOrWhiteSpace(guid))
                {
                    //This is using an AddressPath, try to get the GUID
                    if(!GUID.TryParse(guid, out _))
                    {
                        AddressablesPathDictionary pathDictionary = AddressablesPathDictionary.GetInstance();
                        if(pathDictionary.TryGetGUIDFromPath(guid, out string actualGuid))
                        {
                            guid = actualGuid;
                        }
                    }
                    var asset = Addressables.LoadAssetAsync<GameObject>(guid).WaitForCompletion();
                    if(asset)
                    {
                        _cachedRootTransform = GetProperTransform(asset.transform, rootComponentType);
                        return _cachedRootTransform;
                    }
                }
            }

            //Check if its an AssetReferenceGameObject or an AssetReferenceT<GameObject>
            SerializedProperty assetGuidProperty = rootObjectProperty.FindPropertyRelative("m_AssetGUID");
            if(assetGuidProperty != null)
            {
                SerializedProperty subAssetNameProperty = rootObjectProperty.FindPropertyRelative("m_SubObjectName");
                string guid = assetGuidProperty.stringValue;
                if(GUID.TryParse(guid, out _))
                {
                    var asset = Addressables.LoadAssetAsync<GameObject>($"{guid}[{subAssetNameProperty.stringValue}]").WaitForCompletion();
                    if(asset)
                    {
                        _cachedRootTransform = GetProperTransform(asset.transform, rootComponentType);
                        return _cachedRootTransform;
                    }
                }
            }

            return null;
        }

        private Transform GetProperTransform(Transform rootTransform, Type rootComponentType)
        {
            if(rootComponentType != null && rootComponentType.IsSameOrSubclassOf(typeof(Component)))
            {
                Component obtainedComponentForProperRoot = rootTransform.GetComponentInChildren(rootComponentType);
                if(obtainedComponentForProperRoot != null)
                {
                    return obtainedComponentForProperRoot.transform;
                }
            }
            return rootTransform;
        }
        private Type GetRequiredComponentType(SerializedProperty stringProperty, string siblingPropertyName)
        {
            var parentProperty = FindParentProperty(stringProperty);
            SerializedProperty objectReferenceProperty = null;
            if(parentProperty == null)
            {
                objectReferenceProperty = stringProperty.serializedObject.FindProperty(siblingPropertyName);
            }
            else
            {
                objectReferenceProperty = parentProperty.FindPropertyRelative(siblingPropertyName);
            }

            FieldInfo fieldInfo = GetFieldInfoFromProperty(objectReferenceProperty);
            return fieldInfo.FieldType;
        }

        private SerializedProperty FindParentProperty(SerializedProperty serializedProperty)
        {
            var propertyPaths = serializedProperty.propertyPath.Split('.');
            if (propertyPaths.Length <= 1)
            {
                return default;
            }

            var parentSerializedProperty = serializedProperty.serializedObject.FindProperty(propertyPaths.First());
            for (var index = 1; index < propertyPaths.Length - 1; index++)
            {
                if (propertyPaths[index] == "Array" && propertyPaths.Length > index + 1 && Regex.IsMatch(propertyPaths[index + 1], "^data\\[\\d+\\]$"))
                {
                    var match = Regex.Match(propertyPaths[index + 1], "^data\\[(\\d+)\\]$");
                    var arrayIndex = int.Parse(match.Groups[1].Value);
                    parentSerializedProperty = parentSerializedProperty.GetArrayElementAtIndex(arrayIndex);
                    index++;
                }
                else
                {
                    parentSerializedProperty = parentSerializedProperty.FindPropertyRelative(propertyPaths[index]);
                }
            }

            return parentSerializedProperty;
        }

        private static MethodInfo getFieldInfoFromProperty;
        private FieldInfo GetFieldInfoFromProperty(SerializedProperty property)
        {
            if(getFieldInfoFromProperty == null)
            {
                var scriptAttributeUtilityType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.ScriptAttributeUtility");
                getFieldInfoFromProperty = scriptAttributeUtilityType.GetMethod(nameof(GetFieldInfoFromProperty), BindingFlags.NonPublic | BindingFlags.Static);
            }

            var fieldInfo = (FieldInfo)getFieldInfoFromProperty.Invoke(null, new object[]
            {
                property,
                null
            });
            return fieldInfo;
        }
    }
}
