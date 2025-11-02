using System.Collections.Generic;
using R2API.AddressReferencedAssets;
using RoR2.Editor;
using UnityEditor;
using UnityEngine;

namespace MSU.Editor.PropertyDrawers
{
    public static class UberSkinDefPropertyDrawers
    {
        public class PropertyDrawHelper
        {
            private bool _isFirstDraw;
            private Rect _drawRect;
            public void Reset(in Rect drawRect)
            {
                _isFirstDraw = true;
                _drawRect = drawRect;
            }

            public bool DrawProperty(SerializedProperty property, GUIContent guiContent = null, bool includeChildren = false)
            {
                guiContent ??= property.GetGUIContent();
                if(_isFirstDraw)
                {
                    _drawRect.height = EditorGUI.GetPropertyHeight(property, guiContent, includeChildren) + (EditorGUIUtility.standardVerticalSpacing);
                    _isFirstDraw = false;
                }
                else
                {
                    _drawRect.y += _drawRect.height;
                    _drawRect.height = EditorGUI.GetPropertyHeight(property, guiContent, includeChildren) + (EditorGUIUtility.standardVerticalSpacing);
                }

                return EditorGUI.PropertyField(_drawRect, property, guiContent, includeChildren);
            }
        }

        public abstract class BaseDrawer<T> : IMGUIPropertyDrawer<T>
        {
            public bool IsUsingDirectRootReference(SerializedObject uberSkinDefSerializedObject)
            {
                var targetObjectProperty = uberSkinDefSerializedObject.FindProperty("targetObject");

                return targetObjectProperty.FindPropertyRelative("_useDirectReference").boolValue;    
            }
        }

        [CustomPropertyDrawer(typeof(UberSkinDef.RendererInfo))]
        public class UberSkinDef_RendererInfoPropertyDrawer : BaseDrawer<MSU.UberSkinDef.RendererInfo>
        {
            PropertyDrawHelper _drawHelper = new PropertyDrawHelper();
            List<SerializedProperty> _propertyHeightList = new();
            protected override void DrawIMGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                _drawHelper.Reset(position);
                if (_drawHelper.DrawProperty(property))
                {
                    using var _ = new EditorGUI.IndentLevelScope(1);
                    var rendererProperty = property.FindPropertyRelative("renderer");
                    if (IsUsingDirectRootReference(serializedObject))
                    {
                        _drawHelper.DrawProperty(rendererProperty.FindPropertyRelative("reference"));
                    }
                    else
                    {
                        _drawHelper.DrawProperty(rendererProperty.FindPropertyRelative("transformPath"));
                    }
                    _drawHelper.DrawProperty(property.FindPropertyRelative("defaultMaterial"));
                    _drawHelper.DrawProperty(property.FindPropertyRelative(nameof(UberSkinDef.RendererInfo.defaultShadowCastingMode)));
                    _drawHelper.DrawProperty(property.FindPropertyRelative(nameof(UberSkinDef.RendererInfo.ignoreOverlays)));
                    _drawHelper.DrawProperty(property.FindPropertyRelative(nameof(UberSkinDef.RendererInfo.hideOnDeath)));
                    _drawHelper.DrawProperty(property.FindPropertyRelative(nameof(UberSkinDef.RendererInfo.ignoresMaterialOverrides)));
                }
                property.serializedObject.ApplyModifiedProperties();
            }
            public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            {
                base.GetPropertyHeight(property, label);
                _propertyHeightList.Clear();

                float result = EditorGUI.GetPropertyHeight(property, label, false) + EditorGUIUtility.standardVerticalSpacing;

                if(property.isExpanded)
                {
                    SerializedProperty rendererProperty = property.FindPropertyRelative("renderer");
                    _propertyHeightList.Add(property.FindPropertyRelative("defaultMaterial"));
                    _propertyHeightList.Add(property.FindPropertyRelative("defaultShadowCastingMode"));
                    _propertyHeightList.Add(property.FindPropertyRelative("ignoreOverlays"));
                    _propertyHeightList.Add(property.FindPropertyRelative("hideOnDeath"));
                    _propertyHeightList.Add(property.FindPropertyRelative("ignoresMaterialOverrides"));

                    if (IsUsingDirectRootReference(serializedObject))
                    {
                        result += EditorGUI.GetPropertyHeight(rendererProperty.FindPropertyRelative("reference")) + EditorGUIUtility.standardVerticalSpacing;
                    }
                    else
                    {
                        result += EditorGUI.GetPropertyHeight(rendererProperty.FindPropertyRelative("transformPath")) + EditorGUIUtility.standardVerticalSpacing;
                    }

                    foreach(var prop in _propertyHeightList)
                    {
                        result += EditorGUI.GetPropertyHeight(prop) + EditorGUIUtility.standardVerticalSpacing;
                    }
                }
                return result;
            }
        }

        [CustomPropertyDrawer(typeof(UberSkinDef.GameObjectActivation))]
        public class UberSkinDef_GameObjectActivationPropertyDrawer : BaseDrawer<UberSkinDef.GameObjectActivation>
        {
            PropertyDrawHelper _drawHelper = new PropertyDrawHelper();
            List<SerializedProperty> _propertyHeightList = new();
            protected override void DrawIMGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                _drawHelper.Reset(position);
                if(_drawHelper.DrawProperty(property))
                {
                    using var _0 = new EditorGUI.IndentLevelScope(1);
                    var gameObjectProperty = property.FindPropertyRelative("gameObject");
                    if (IsUsingDirectRootReference(serializedObject))
                    {
                        _drawHelper.DrawProperty(gameObjectProperty.FindPropertyRelative("reference"));
                    }
                    else
                    {
                        _drawHelper.DrawProperty(gameObjectProperty.FindPropertyRelative("transformPath"));
                    }

                    SerializedProperty spawnPrefabOnModelObjectProperty = property.FindPropertyRelative("spawnPrefabOnModelObject");
                    using(new EditorGUI.DisabledScope(spawnPrefabOnModelObjectProperty.boolValue == true))
                    {
                        _drawHelper.DrawProperty(property.FindPropertyRelative("shouldActivate"));
                    }
                    _drawHelper.DrawProperty(spawnPrefabOnModelObjectProperty);

                    if(spawnPrefabOnModelObjectProperty.boolValue)
                    {
                        _drawHelper.DrawProperty(property.FindPropertyRelative("prefab"));
                        _drawHelper.DrawProperty(property.FindPropertyRelative("localPosition"));
                        _drawHelper.DrawProperty(property.FindPropertyRelative("localRotation"));
                        _drawHelper.DrawProperty(property.FindPropertyRelative("localScale"));
                    }
                }
                property.serializedObject.ApplyModifiedProperties();
            }

            public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            {
                base.GetPropertyHeight(property, label);
                _propertyHeightList.Clear();

                float result = EditorGUI.GetPropertyHeight(property, label, false) + EditorGUIUtility.standardVerticalSpacing;

                if (property.isExpanded)
                {
                    SerializedProperty gameObjectProperty = property.FindPropertyRelative("gameObject");
                    SerializedProperty spawnPrefabOnModelObjectProperty = property.FindPropertyRelative("spawnPrefabOnModelObject");

                    if (IsUsingDirectRootReference(serializedObject))
                    {
                        result += EditorGUI.GetPropertyHeight(gameObjectProperty.FindPropertyRelative("reference")) + EditorGUIUtility.standardVerticalSpacing;
                    }
                    else
                    {
                        result += EditorGUI.GetPropertyHeight(gameObjectProperty.FindPropertyRelative("transformPath")) + EditorGUIUtility.standardVerticalSpacing;
                    }

                    _propertyHeightList.Add(spawnPrefabOnModelObjectProperty);
                    _propertyHeightList.Add(property.FindPropertyRelative("shouldActivate"));

                    if(spawnPrefabOnModelObjectProperty.boolValue)
                    {
                        _propertyHeightList.Add(property.FindPropertyRelative("prefab"));
                        _propertyHeightList.Add(property.FindPropertyRelative("localPosition"));
                        _propertyHeightList.Add(property.FindPropertyRelative("localRotation"));
                        _propertyHeightList.Add(property.FindPropertyRelative("localScale"));
                    }

                    foreach (var prop in _propertyHeightList)
                    {
                        result += EditorGUI.GetPropertyHeight(prop) + EditorGUIUtility.standardVerticalSpacing;
                    }
                }
                return result;
            }
        }

        [CustomPropertyDrawer(typeof(UberSkinDef.MeshReplacement))]
        public class UberSkinDef_MeshReplacementPropertyDrawer : BaseDrawer<UberSkinDef.MeshReplacement>
        {
            PropertyDrawHelper _drawHelper = new PropertyDrawHelper();
            List<SerializedProperty> _propertyHeightList = new();
            protected override void DrawIMGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                _drawHelper.Reset(position);
                if (_drawHelper.DrawProperty(property))
                {
                    using var _ = new EditorGUI.IndentLevelScope(1);
                    var rendererProperty = property.FindPropertyRelative("renderer");
                    if (IsUsingDirectRootReference(serializedObject))
                    {
                        _drawHelper.DrawProperty(rendererProperty.FindPropertyRelative("reference"));
                    }
                    else
                    {
                        _drawHelper.DrawProperty(rendererProperty.FindPropertyRelative("transformPath"));
                    }
                    _drawHelper.DrawProperty(property.FindPropertyRelative("mesh"));
                }
                property.serializedObject.ApplyModifiedProperties();
            }
            public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            {
                base.GetPropertyHeight(property, label);
                _propertyHeightList.Clear();

                float result = EditorGUI.GetPropertyHeight(property, label, false) + EditorGUIUtility.standardVerticalSpacing;

                if (property.isExpanded)
                {
                    SerializedProperty rendererProperty = property.FindPropertyRelative("renderer");
                    _propertyHeightList.Add(property.FindPropertyRelative("mesh"));

                    if (IsUsingDirectRootReference(serializedObject))
                    {
                        result += EditorGUI.GetPropertyHeight(rendererProperty.FindPropertyRelative("reference")) + EditorGUIUtility.standardVerticalSpacing;
                    }
                    else
                    {
                        result += EditorGUI.GetPropertyHeight(rendererProperty.FindPropertyRelative("transformPath")) + EditorGUIUtility.standardVerticalSpacing;
                    }

                    foreach (var prop in _propertyHeightList)
                    {
                        result += EditorGUI.GetPropertyHeight(prop) + EditorGUIUtility.standardVerticalSpacing;
                    }
                }
                return result;
            }
        }

        [CustomPropertyDrawer(typeof(UberSkinDef.LightInfo))]
        public class UberSkinDef_LightInfoPropertyDrawer : BaseDrawer<UberSkinDef.LightInfo>
        {
            PropertyDrawHelper _drawHelper = new PropertyDrawHelper();
            List<SerializedProperty> _propertyHeightList = new();
            protected override void DrawIMGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                _drawHelper.Reset(position);
                if (_drawHelper.DrawProperty(property))
                {
                    using var _ = new EditorGUI.IndentLevelScope(1);
                    var rendererProperty = property.FindPropertyRelative("light");
                    if (IsUsingDirectRootReference(serializedObject))
                    {
                        _drawHelper.DrawProperty(rendererProperty.FindPropertyRelative("reference"));
                    }
                    else
                    {
                        _drawHelper.DrawProperty(rendererProperty.FindPropertyRelative("transformPath"));
                    }
                    _drawHelper.DrawProperty(property.FindPropertyRelative("lightColor"));
                }
                property.serializedObject.ApplyModifiedProperties();
            }
            public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            {
                base.GetPropertyHeight(property, label);
                _propertyHeightList.Clear();

                float result = EditorGUI.GetPropertyHeight(property, label, false) + EditorGUIUtility.standardVerticalSpacing;

                if (property.isExpanded)
                {
                    SerializedProperty rendererProperty = property.FindPropertyRelative("light");
                    _propertyHeightList.Add(property.FindPropertyRelative("lightColor"));

                    if (IsUsingDirectRootReference(serializedObject))
                    {
                        result += EditorGUI.GetPropertyHeight(rendererProperty.FindPropertyRelative("reference")) + EditorGUIUtility.standardVerticalSpacing;
                    }
                    else
                    {
                        result += EditorGUI.GetPropertyHeight(rendererProperty.FindPropertyRelative("transformPath")) + EditorGUIUtility.standardVerticalSpacing;
                    }

                    foreach (var prop in _propertyHeightList)
                    {
                        result += EditorGUI.GetPropertyHeight(prop) + EditorGUIUtility.standardVerticalSpacing;
                    }
                }
                return result;
            }
        }

        [CustomPropertyDrawer(typeof(UberSkinDef.VFXOverride))]
        public class UberSkinDef_VFXOverridePropertyDrawer : BaseDrawer<UberSkinDef.VFXOverride>
        {
            PropertyDrawHelper _drawHelper = new PropertyDrawHelper();
            List<SerializedProperty> _propertyHeightList = new();
            protected override void DrawIMGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                _drawHelper.Reset(position);
                if (_drawHelper.DrawProperty(property))
                {
                    using var _ = new EditorGUI.IndentLevelScope(1);
                    _drawHelper.DrawProperty(property.FindPropertyRelative("targetEffect"));

                    var replacementEffectPrefabProperty = property.FindPropertyRelative("replacementEffectPrefab");
                    _drawHelper.DrawProperty(replacementEffectPrefabProperty);
                    if(!replacementEffectPrefabProperty.objectReferenceValue)
                    {
                        _drawHelper.DrawProperty(property.FindPropertyRelative("serializedOnEffectSpawned"));
                    }
                }
                property.serializedObject.ApplyModifiedProperties();
            }

            public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            {
                base.GetPropertyHeight(property, label);
                _propertyHeightList.Clear();

                float result = EditorGUI.GetPropertyHeight(property, label, false) + EditorGUIUtility.standardVerticalSpacing;

                if (property.isExpanded)
                {
                    _propertyHeightList.Add(property.FindPropertyRelative("targetEffect"));
                 
                    SerializedProperty replacementEffectPrefabProperty = property.FindPropertyRelative("replacementEffectPrefab");
                    _propertyHeightList.Add(replacementEffectPrefabProperty);

                    if(!replacementEffectPrefabProperty.objectReferenceValue)
                    {
                        _propertyHeightList.Add(property.FindPropertyRelative("serializedOnEffectSpawned"));
                    }

                    foreach (var prop in _propertyHeightList)
                    {
                        result += EditorGUI.GetPropertyHeight(prop) + EditorGUIUtility.standardVerticalSpacing;
                    }
                }
                return result;
            }
        }
    }
}