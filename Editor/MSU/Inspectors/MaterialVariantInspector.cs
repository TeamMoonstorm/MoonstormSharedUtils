using RoR2.Editor;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using UnityEditor.VersionControl;
using UnityEngine;
using static MSU.MaterialVariant;

namespace MSU.Editor.Inspectors
{
    [CustomEditor(typeof(MaterialVariant))]
    public class MaterialVariantInspector : IMGUIScriptableObjectInspector<MaterialVariant>
    {
        SerializedObject _originalMaterialSO;
        SerializedProperty _originalMaterialProperty;

        SerializedProperty _materialVariantProperty;
        SerializedObject _materialVariantSO;

        MaterialEditor _materialEditorForCopy;
        Material _materialVariantCopy;

        SerializedProperty _propertyOverridesProperty;
        Shader _origMaterialShader => (_originalMaterialSO?.targetObject as Material)?.shader;
        string _currentShaderName;

        string[] _shaderPropertyNames;
        int[] _shaderPropertyIndices;
        ShaderPropertyType[] _shaderPropertyTypes;
        bool _shouldApplyOverridesOnMaterialVariant = false;
        bool _initialCopy = false;
        bool _initialApplyOverrides = false;
        protected override void OnInspectorEnabled()
        {
            base.OnInspectorEnabled();

            _originalMaterialProperty = serializedObject.FindProperty("originalMaterial");
            if(_originalMaterialProperty.objectReferenceValue)
                _originalMaterialSO = new SerializedObject(_originalMaterialProperty.objectReferenceValue);

            _materialVariantProperty = serializedObject.FindProperty("_material");
            _materialVariantSO = new SerializedObject(_materialVariantProperty.objectReferenceValue);

            _materialVariantCopy = new Material((Material)_materialVariantSO.targetObject);
            _materialEditorForCopy = (MaterialEditor)MaterialEditor.CreateEditor(_materialVariantCopy);

            _propertyOverridesProperty = serializedObject.FindProperty("_propertyOverrides");
        }

        protected override void DrawIMGUI()
        {
            if(_origMaterialShader && _currentShaderName != _origMaterialShader.name && _originalMaterialSO != null)
            {
                RetrieveShaderData(_originalMaterialSO.targetObject as Material);
            }
            serializedObject.Update();
            IMGUIUtil.DrawCheckableProperty(_originalMaterialProperty, OnMaterialChanged);
            if (!_initialCopy) //Updates the variant to the orig's materials.
            {
                _initialCopy = true;
                OnMaterialChanged(_originalMaterialProperty);
            }

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(_materialVariantProperty);
            EditorGUI.EndDisabledGroup();

            DrawPropertyOverrides();

            if((serializedObject.ApplyModifiedProperties() && _shouldApplyOverridesOnMaterialVariant) || (!_initialApplyOverrides && _shouldApplyOverridesOnMaterialVariant)) //Since the initial copy updates the variant's material back to the originals, we have to apply the override again.
            {
                _initialApplyOverrides = true;
                ApplyOverridesOnMaterialVariant(_propertyOverridesProperty);
            }
        }

        private void RetrieveShaderData(Material mat)
        {
            var shaderToUse = mat.shader;
            _currentShaderName = mat.shader.name;
            if(_currentShaderName == "MSU/AddressableMaterialShader")
            {
                var keywords = mat.shaderKeywords;
                string stubbedShaderName = HG.ArrayUtils.GetSafe(keywords, 1);

                if(stubbedShaderName.IsNullOrEmptyOrWhiteSpace())
                {
                    EditorCoroutineUtility.StartCoroutineOwnerless(SwitchBackAndForth(serializedObject.targetObject, mat));
                    return;
                }

                shaderToUse = Shader.Find(stubbedShaderName);
                _shouldApplyOverridesOnMaterialVariant = false;
            }
            else
            {
                _shouldApplyOverridesOnMaterialVariant = true;
            }

            _materialEditorForCopy.SetShader(shaderToUse);

            var propCount = UnityEditor.ShaderUtil.GetPropertyCount(shaderToUse);
            _shaderPropertyIndices = new int[propCount];
            _shaderPropertyTypes = new ShaderPropertyType[propCount];
            _shaderPropertyNames = new string[propCount];
            for(int i = 0; i < propCount; i++)
            {
                _shaderPropertyIndices[i] = i;
                _shaderPropertyNames[i] = UnityEditor.ShaderUtil.GetPropertyName(shaderToUse, i);
                _shaderPropertyTypes[i] = (ShaderPropertyType)UnityEditor.ShaderUtil.GetPropertyType(shaderToUse, i);
            }
        }

        private static IEnumerator SwitchBackAndForth(UnityEngine.Object targetObject, Material mat)
        {
            Selection.activeObject = mat;
            yield return new EditorWaitForSeconds(0.2f);
            Selection.activeObject = targetObject;
        }

        private void DrawPropertyOverrides()
        {
            if (!_originalMaterialProperty.objectReferenceValue)
            {
                EditorGUILayout.LabelField("No Original Material Selected.");
                return;
            }

            _propertyOverridesProperty.isExpanded = EditorGUILayout.Foldout(_propertyOverridesProperty.isExpanded, _propertyOverridesProperty.GetGUIContent(), true);

            if (!_propertyOverridesProperty.isExpanded)
                return;

            EditorGUI.indentLevel++;
            EditorGUILayout.BeginVertical("box");
            _propertyOverridesProperty.arraySize = EditorGUILayout.DelayedIntField("Array Size", _propertyOverridesProperty.arraySize);
            for(int i = 0; i < _propertyOverridesProperty.arraySize; i++)
            {
                var prop = _propertyOverridesProperty.GetArrayElementAtIndex(i);
                DrawPropOverride(prop);
            }
            EditorGUILayout.EndVertical();
            EditorGUI.indentLevel--;
        }

        private void ApplyOverridesOnMaterialVariant(SerializedProperty property)
        {
            var variant = _materialVariantSO.targetObject as Material;

            for(int i = 0; i < property.arraySize; i++)
            {
                var prop = property.GetArrayElementAtIndex(i);
                var shaderPropName = prop.FindPropertyRelative("propertyName");
                var propType = prop.FindPropertyRelative("propertyType");

                var texRefProp = prop.FindPropertyRelative("_textureReference");

                switch ((ShaderPropertyType)propType.enumValueIndex)
                {
                    case ShaderPropertyType.Color:
                        variant.SetColor(shaderPropName.stringValue, prop.FindPropertyRelative("_colorValue").colorValue);
                        break;
                    case ShaderPropertyType.Int:
                    case ShaderPropertyType.Range:
                    case ShaderPropertyType.Float:
                        variant.SetFloat(shaderPropName.stringValue, prop.FindPropertyRelative("_floatValue").floatValue);
                        break;
                    case ShaderPropertyType.Vector:
                        variant.SetVector(shaderPropName.stringValue, prop.FindPropertyRelative("_vectorValue").vector4Value);
                        break;
                    case ShaderPropertyType.TexEnv:
                        var tex = (Texture)texRefProp.FindPropertyRelative("texture").objectReferenceValue;
                        var offset = texRefProp.FindPropertyRelative("offset").vector2Value;
                        var scale = texRefProp.FindPropertyRelative("scale").vector2Value;

                        variant.SetTexture(shaderPropName.stringValue, tex);
                        variant.SetTextureOffset(shaderPropName.stringValue, offset);
                        variant.SetTextureScale(shaderPropName.stringValue, scale);
                        break;
                }

                EditorUtility.SetDirty(variant);
            }
        }

        private void DrawPropOverride(SerializedProperty prop)
        {
            prop.isExpanded = EditorGUILayout.Foldout(prop.isExpanded, prop.GetGUIContent(), true);
            if (!prop.isExpanded)
                return;

            EditorGUI.indentLevel++;
            EditorGUILayout.BeginVertical("box");

            var shaderPropNameProp = prop.FindPropertyRelative("propertyName");
            var propType = prop.FindPropertyRelative("propertyType");

            DrawPopup(prop, shaderPropNameProp, propType); //Draws the popup.

            ClearMaterialProperties(_materialVariantCopy); //Clear all the properties from the copy, except the shader itself.

            TransferPropertyOverrideToCopy(_materialVariantCopy, prop); //Transfer the properties so the material property we're going to get is "valid"

            MaterialProperty property = MaterialEditor.GetMaterialProperty(new UnityEngine.Object[] { _materialVariantCopy }, shaderPropNameProp.stringValue); //Get the current material override's value.

            if (property.type == MaterialProperty.PropType.Texture && !property.textureValue)
                property.textureValue = (Texture)prop.FindPropertyRelative("_textureReference").FindPropertyRelative("texture").objectReferenceValue;

            EditorGUI.BeginChangeCheck();
            _materialEditorForCopy.ShaderProperty(property, property.displayName); //Draw the property itself.
            if(EditorGUI.EndChangeCheck())
            {
                TransferMaterialPropertiesToPropertyOverrides(_materialVariantCopy, prop); //The prop has changed, transfer the new value to the property override.
            }

            EditorGUILayout.EndVertical();
            EditorGUI.indentLevel--;
        }

        private void DrawPopup(SerializedProperty property, SerializedProperty shaderPropNameProp, SerializedProperty propType)
        {
            var chosenIndex = -1;
            for (int i = 0; i < _shaderPropertyNames.Length; i++)
            {
                if (_shaderPropertyNames[i] == shaderPropNameProp.stringValue)
                {
                    chosenIndex = i;
                    break;
                }
            }
            if (chosenIndex == -1)
            {
                chosenIndex = 0;
                shaderPropNameProp.stringValue = _shaderPropertyNames[chosenIndex];
            }

            EditorGUI.BeginChangeCheck();
            chosenIndex = EditorGUILayout.IntPopup(shaderPropNameProp.displayName, chosenIndex, _shaderPropertyNames, _shaderPropertyIndices);
            if(EditorGUI.EndChangeCheck() || propType.enumValueIndex != (int)_shaderPropertyTypes[chosenIndex])
            {
                shaderPropNameProp.stringValue = _shaderPropertyNames[chosenIndex];
                var newEnumValueIndex = _shaderPropertyTypes[chosenIndex];

                if(propType.enumValueIndex != (int)newEnumValueIndex)
                {
                    Shader shaderBeingUsedInMaterialEditor = (_materialEditorForCopy.target as Material).shader;
                    propType.enumValueIndex = (int)newEnumValueIndex;

                    var floatValueProp = property.FindPropertyRelative("_floatValue");
                    var colorValueProp = property.FindPropertyRelative("_colorValue");
                    var vectorValueProp = property.FindPropertyRelative("_vectorValue");
                    var textureReferenceProp = property.FindPropertyRelative("_textureReference");
                    switch (newEnumValueIndex)
                    {
                        case ShaderPropertyType.Color:
                            floatValueProp.floatValue = 0;
                            vectorValueProp.vector4Value = Vector4.zero;
                            textureReferenceProp.FindPropertyRelative("texture").objectReferenceValue = null;
                            textureReferenceProp.FindPropertyRelative("offset").vector2Value = Vector2.zero;
                            textureReferenceProp.FindPropertyRelative("scale").vector2Value = Vector2.one;
                            var vector4 = shaderBeingUsedInMaterialEditor.GetPropertyDefaultVectorValue(_shaderPropertyIndices[chosenIndex]);
                            colorValueProp.colorValue = new Color(vector4.x, vector4.y, vector4.z, vector4.w);
                            break;
                        case ShaderPropertyType.Int:
                        case ShaderPropertyType.Range:
                        case ShaderPropertyType.Float:
                            textureReferenceProp.FindPropertyRelative("texture").objectReferenceValue = null;
                            textureReferenceProp.FindPropertyRelative("offset").vector2Value = Vector2.zero;
                            textureReferenceProp.FindPropertyRelative("scale").vector2Value = Vector2.one;
                            colorValueProp.colorValue = Color.white;
                            vectorValueProp.vector4Value = Vector4.zero;
                            floatValueProp.floatValue = shaderBeingUsedInMaterialEditor.GetPropertyDefaultFloatValue(_shaderPropertyIndices[chosenIndex]);
                            break;
                        case ShaderPropertyType.Vector:
                            textureReferenceProp.FindPropertyRelative("texture").objectReferenceValue = null;
                            textureReferenceProp.FindPropertyRelative("offset").vector2Value = Vector2.zero;
                            textureReferenceProp.FindPropertyRelative("scale").vector2Value = Vector2.one;
                            floatValueProp.floatValue = 0;
                            colorValueProp.colorValue = Color.white;
                            vectorValueProp.vector4Value = shaderBeingUsedInMaterialEditor.GetPropertyDefaultVectorValue(_shaderPropertyIndices[chosenIndex]);
                            break;
                        case ShaderPropertyType.TexEnv:
                            floatValueProp.floatValue = 0;
                            colorValueProp.colorValue = Color.white;
                            vectorValueProp.vector4Value = Vector4.zero;
                            break;
                    }
                }
            }
        }


        private void TransferPropertyOverrideToCopy(Material materialVariantCopy, SerializedProperty prop)
        {
            var shaderPropName = prop.FindPropertyRelative("propertyName");
            var propType = prop.FindPropertyRelative("propertyType");

            var texRefProp = prop.FindPropertyRelative("_textureReference");

            switch((ShaderPropertyType)propType.enumValueIndex)
            {
                case ShaderPropertyType.Color:
                    materialVariantCopy.SetColor(shaderPropName.stringValue, prop.FindPropertyRelative("_colorValue").colorValue);
                    break;
                case ShaderPropertyType.Int:
                case ShaderPropertyType.Range:
                case ShaderPropertyType.Float:
                    materialVariantCopy.SetFloat(shaderPropName.stringValue, prop.FindPropertyRelative("_floatValue").floatValue);
                    break;
                case ShaderPropertyType.Vector:
                    materialVariantCopy.SetVector(shaderPropName.stringValue, prop.FindPropertyRelative("_vectorValue").vector4Value);
                    break;
                case ShaderPropertyType.TexEnv:
                    var tex = (Texture)texRefProp.FindPropertyRelative("texture").objectReferenceValue;
                    var offset = texRefProp.FindPropertyRelative("offset").vector2Value;
                    var scale = texRefProp.FindPropertyRelative("scale").vector2Value;

                    materialVariantCopy.SetTexture(shaderPropName.stringValue, tex);
                    materialVariantCopy.SetTextureOffset(shaderPropName.stringValue, offset);
                    materialVariantCopy.SetTextureScale(shaderPropName.stringValue, scale);
                    break;
            }

            EditorUtility.SetDirty(materialVariantCopy);
        }

        private void TransferMaterialPropertiesToPropertyOverrides(Material materialVariantCopy, SerializedProperty prop)
        {
            var shaderPropName = prop.FindPropertyRelative("propertyName");
            var propType = prop.FindPropertyRelative("propertyType");

            var texRefProp = prop.FindPropertyRelative("_textureReference");

            switch ((ShaderPropertyType)propType.enumValueIndex)
            {
                case ShaderPropertyType.Color:
                    prop.FindPropertyRelative("_colorValue").colorValue = materialVariantCopy.GetColor(shaderPropName.stringValue);
                    break;
                case ShaderPropertyType.Int:
                case ShaderPropertyType.Range:
                case ShaderPropertyType.Float:
                    prop.FindPropertyRelative("_floatValue").floatValue = materialVariantCopy.GetFloat(shaderPropName.stringValue);
                    break;
                case ShaderPropertyType.Vector:
                    prop.FindPropertyRelative("_vectorValue").vector4Value = materialVariantCopy.GetVector(shaderPropName.stringValue);
                    break;
                case ShaderPropertyType.TexEnv:
                    var tex = texRefProp.FindPropertyRelative("texture");
                    var offset = texRefProp.FindPropertyRelative("offset");
                    var scale = texRefProp.FindPropertyRelative("scale");

                    tex.objectReferenceValue = materialVariantCopy.GetTexture(shaderPropName.stringValue);
                    offset.vector2Value = materialVariantCopy.GetTextureOffset(shaderPropName.stringValue);
                    scale.vector2Value = materialVariantCopy.GetTextureScale(shaderPropName.stringValue);
                    break;
            }
        }

        private void OnMaterialChanged(SerializedProperty prop)
        {
            var newMat = prop.objectReferenceValue;
            if(!newMat) //Clear values from variant itself.
            {
                var savedProperties = _materialVariantSO.FindProperty("m_SavedProperties");
                savedProperties.FindPropertyRelative("m_TexEnvs").ClearArray();
                savedProperties.FindPropertyRelative("m_Ints").ClearArray();
                savedProperties.FindPropertyRelative("m_Floats").ClearArray();
                savedProperties.FindPropertyRelative("m_Colors").ClearArray();

                _materialVariantSO.FindProperty("m_InvalidKeywords").ClearArray();
                _materialVariantSO.FindProperty("m_Shader").objectReferenceValue = Shader.Find("StubbedRoR2/Base/Shaders/HGStandard");
                _materialVariantSO.ApplyModifiedProperties();
                _originalMaterialSO = null;
                return;
            }

            //Copy over values from orig mat to variant mat
            var origMat = (Material)newMat;
            _originalMaterialSO = new SerializedObject(origMat);
            var variantMat = (Material)_materialVariantSO.targetObject;

            variantMat.shader = origMat.shader;
            variantMat.CopyPropertiesFromMaterial(origMat);
            variantMat.renderQueue = origMat.renderQueue;

            AssetDatabase.SaveAssetIfDirty(variantMat);
        }

        private static void ClearMaterialProperties(Material mat)
        {
            var serializedObject = new SerializedObject(mat);
            var savedProperties = serializedObject.FindProperty("m_SavedProperties");
            savedProperties.FindPropertyRelative("m_TexEnvs").ClearArray();
            savedProperties.FindPropertyRelative("m_Ints").ClearArray();
            savedProperties.FindPropertyRelative("m_Floats").ClearArray();
            savedProperties.FindPropertyRelative("m_Colors").ClearArray();

            serializedObject.FindProperty("m_InvalidKeywords").ClearArray();
            serializedObject.ApplyModifiedProperties();
        }

        [MenuItem("Assets/Create/MSU/Material Variant")]
        private static void CreateInstance()
        {
            var path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (!AssetDatabase.IsValidFolder(path))
                return;

            var assetPath = IOUtils.GenerateUniqueFileName(path, "New MaterialVariant", ".asset");
            var instance = CreateInstance<MaterialVariant>();
            instance.name = "New MaterialVariant";

            AssetDatabase.CreateAsset(instance, assetPath);
            instance._material = new Material(Shader.Find("StubbedRoR2/Base/Shaders/HGStandard"));
            instance._material.name = "matNew MaterialVariant";
            instance._material.hideFlags = HideFlags.NotEditable;

            AssetDatabase.AddObjectToAsset(instance._material, instance);
            AssetDatabase.ImportAsset(assetPath);
            AssetDatabase.SaveAssets();
            EditorUtility.FocusProjectWindow();
        }
    }
}