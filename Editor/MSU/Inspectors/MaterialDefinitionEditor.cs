using RoR2.Editor;
using System;
using System.Net;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UIElements;
using static MSU.MaterialDefinition;
using static UnityEditor.MaterialProperty;

namespace MSU.Editor.Inspectors
{
    [CustomEditor(typeof(MaterialDefinition))]
    public class MaterialDefinitionEditor : UnityEditor.Editor
    {
        private struct DeconstructedAddress
        {
            public string assetGUID;
            public string subObjectName;

            public bool IsEmpty() => string.IsNullOrWhiteSpace(assetGUID);

            public static readonly DeconstructedAddress empty = new DeconstructedAddress { assetGUID = "", subObjectName = "" };
        }

        public override VisualElement CreateInspectorGUI()
        {
            var tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(AssetDatabase.GUIDToAssetPath("689f074f77119904b876dfe82bcad8c6"));
            var root = tree.CloneTree();
            InitElements(root);
            return root;
        }

        PropertyField materialAddressField;
        SerializedProperty materialAddressProperty;

        PropertyField materialAssetField;
        SerializedProperty materialAssetProperty;

        ObjectField materialInstanceField;
        SerializedProperty materialInstanceProperty;

        //IntegerField renderQueueField;
        SerializedProperty renderQueueProperty;

        IMGUIContainer propertyOverridesIMGUIContainer;
        SerializedProperty propertyOverridesProperty;

        private void InitElements(VisualElement root)
        {
            materialAddressProperty = serializedObject.FindProperty("originalMaterialAddress");
            materialAddressField = root.Q<PropertyField>();
            materialAddressField.bindingPath = materialAddressProperty.propertyPath;

            materialAssetProperty = serializedObject.FindProperty("originalMaterialAsset");
            materialAssetField = root.Q<PropertyField>("MaterialAsset");
            materialAssetField.bindingPath = materialAssetProperty.propertyPath;

            materialInstanceProperty = serializedObject.FindProperty("_material");
            materialInstanceField = root.Q<ObjectField>("MaterialInstance");
            materialInstanceField.bindingPath = materialInstanceProperty.propertyPath;
            materialInstanceField.SetEnabled(false);

            renderQueueProperty = serializedObject.FindProperty("renderQueue");
            //renderQueueField = root.Q<IntegerField>("RenderQueue");
            //renderQueueField.bindingPath = renderQueueProperty.propertyPath;

            propertyOverridesIMGUIContainer = root.Q<IMGUIContainer>();
            propertyOverridesIMGUIContainer.onGUIHandler = DrawPropertyOverrides;
            propertyOverridesProperty = serializedObject.FindProperty(nameof(MaterialDefinition.propertyOverrides));

            materialAddressField.RegisterValueChangeCallback(propEvent =>
            {
                serializedObject.ApplyModifiedProperties();
                var materialAddress = GetMaterialAddress();

                SetMaterialAddress(materialAddress.assetGUID, materialAddress.subObjectName);

                bool isAddressEmpty = string.IsNullOrWhiteSpace(materialAddress.assetGUID);

                if(isAddressEmpty)
                {
                    materialAssetField.SetDisplay(true);
                }
                else
                {
                    materialAssetField.SetDisplay(false);
                    serializedObject.FindProperty("originalMaterialAsset").objectReferenceValue = null;
                }

                OnOriginalMaterialReferenceChanged(materialAddress.IsEmpty() ? null : materialAddress);
            });
            materialAddressField.SetDisplay(materialAssetProperty.objectReferenceValue == null);

            materialAssetField.RegisterValueChangeCallback(propEvent =>
            {
                serializedObject.ApplyModifiedProperties();
                var asset = propEvent.changedProperty.objectReferenceValue;

                if(!asset)
                {
                    materialAddressField.SetDisplay(true);
                }
                else
                {
                    materialAddressField.SetDisplay(false);
                    var originalMaterialAddress = serializedObject.FindProperty("originalMaterialAddress");
                    originalMaterialAddress.FindPropertyRelative("m_AssetGUID").stringValue = "";
                    originalMaterialAddress.FindPropertyRelative("m_SubObjectName").stringValue = "";
                }

                materialAssetField.MarkDirtyRepaint();
                OnOriginalMaterialReferenceChanged(asset);
            });
            materialAssetField.SetDisplay(GetMaterialAddress().IsEmpty() == true);

            //Apply the editor
            ApplyEditor((MaterialDefinition)serializedObject.targetObject);

            //Retrieve shader metadata, which will let us use the dropdowns
            RetrieveShaderMetadata(materialInstanceProperty.objectReferenceValue as Material);

            if(_materialInstanceCopy)
            {
                DestroyImmediate(_materialInstanceCopy);
            }
            _materialInstanceCopy = new Material((Material)materialInstanceProperty.objectReferenceValue);
            if(_materialEditorForCopy)
            {
                DestroyImmediate(_materialEditorForCopy);
            }
            _materialEditorForCopy = (MaterialEditor)MaterialEditor.CreateEditor(_materialInstanceCopy);
        }

        private void OnOriginalMaterialReferenceChanged(object reference)
        {
            int baseRenderQueue = -1;
            if(reference is null)
            {
                propertyOverridesIMGUIContainer.onGUIHandler = NoReferenceSetHelpBox;
            }
            else if(reference is DeconstructedAddress deconstructedAddress)
            {
                propertyOverridesIMGUIContainer.onGUIHandler = DrawPropertyOverrides;
            }
            else if(reference is Material material)
            {
                propertyOverridesIMGUIContainer.onGUIHandler = DrawPropertyOverrides;
                baseRenderQueue = material.renderQueue;
            }

            //Apply the editor
            ApplyEditor((MaterialDefinition)serializedObject.targetObject, updateRenderQueue: true);

            //Retrieve shader metadata, which will let us use the dropdowns
            RetrieveShaderMetadata(materialInstanceProperty.objectReferenceValue as Material);

            if (_materialInstanceCopy)
            {
                DestroyImmediate(_materialInstanceCopy);
            }
            _materialInstanceCopy = new Material((Material)materialInstanceProperty.objectReferenceValue);
            if (_materialEditorForCopy)
            {
                DestroyImmediate(_materialEditorForCopy);
            }
            _materialEditorForCopy = (MaterialEditor)MaterialEditor.CreateEditor(_materialInstanceCopy);
        }

        private DeconstructedAddress GetMaterialAddress()
        {
            SerializedProperty m_AssetGUID = materialAddressProperty.FindPropertyRelative("m_AssetGUID");
            SerializedProperty m_SubObjectName = materialAddressProperty.FindPropertyRelative("m_SubObjectName");

            if (string.IsNullOrWhiteSpace(m_AssetGUID.stringValue))
                return DeconstructedAddress.empty;

            return new DeconstructedAddress { assetGUID = m_AssetGUID.stringValue, subObjectName = m_SubObjectName.stringValue };
        }

        private void SetMaterialAddress(string assetGUID, string subObjectName)
        {
            SerializedProperty m_AssetGUID = materialAddressProperty.FindPropertyRelative("m_AssetGUID");
            SerializedProperty m_SubObjectName = materialAddressProperty.FindPropertyRelative("m_SubObjectName");

            m_AssetGUID.stringValue = assetGUID;
            m_SubObjectName.stringValue = subObjectName;
        }

        private void RetrieveShaderMetadata(Material materialInstance)
        {
            var _shaderToUse = materialInstance.shader;
            if (!_shaderToUse)
                return;

            var propCount = UnityEditor.ShaderUtil.GetPropertyCount(_shaderToUse);
            _shaderPropertyIndices = new int[propCount];
            _shaderPropertyTypes = new ShaderPropertyType[propCount];
            _shaderPropertyNames = new string[propCount];

            for(int i = 0; i < propCount; i++)
            {
                _shaderPropertyIndices[i] = i;
                _shaderPropertyNames[i] = UnityEditor.ShaderUtil.GetPropertyName(_shaderToUse, i);
                _shaderPropertyTypes[i] = (ShaderPropertyType)UnityEditor.ShaderUtil.GetPropertyType(_shaderToUse, i);
            }
        }

        private void NoReferenceSetHelpBox()
        {
            EditorGUILayout.HelpBox($"Cannot Display Overrides as there's no Original Material Reference set.", MessageType.Info);
        }

        MaterialEditor _materialEditorForCopy;
        Material _materialInstanceCopy;

        string[] _shaderPropertyNames;
        int[] _shaderPropertyIndices;
        ShaderPropertyType[] _shaderPropertyTypes;
        private void DrawPropertyOverrides()
        {
            serializedObject.Update();

            EditorGUI.BeginChangeCheck();
            _materialEditorForCopy.RenderQueueField();
            if(EditorGUI.EndChangeCheck())
            {
                //Store the new render queue from the copy
                renderQueueProperty.intValue = _materialInstanceCopy.renderQueue;
                //Apply modified properties
                serializedObject.ApplyModifiedProperties();
                //Apply editor
                ApplyEditor(serializedObject.targetObject as MaterialDefinition);
            }

            propertyOverridesProperty.isExpanded = EditorGUILayout.Foldout(propertyOverridesProperty.isExpanded, propertyOverridesProperty.GetGUIContent(), true);

            if (propertyOverridesProperty.isExpanded == false)
                return;

            using var indentScope = new EditorGUI.IndentLevelScope();
            using var vertScope = new EditorGUILayout.VerticalScope("box");
            propertyOverridesProperty.arraySize = EditorGUILayout.DelayedIntField("Array Size", propertyOverridesProperty.arraySize);
            for(int i = 0; i < propertyOverridesProperty.arraySize; i++)
            {
                var prop = propertyOverridesProperty.GetArrayElementAtIndex(i);
                DrawPropertyOverride(prop);
            }

            if(serializedObject.ApplyModifiedProperties())
            {
                ApplyEditor(serializedObject.targetObject as MaterialDefinition);
            }
        }

        private static readonly Material[] materialArray = new Material[1];
        private void DrawPropertyOverride(SerializedProperty prop)
        {
            prop.isExpanded = EditorGUILayout.Foldout(prop.isExpanded, prop.GetGUIContent(), true);
            if (prop.isExpanded == false)
                return;

            using var indentScope = new EditorGUI.IndentLevelScope();
            using var vertScope = new EditorGUILayout.VerticalScope("box");

            var shaderPropNameProp = prop.FindPropertyRelative("propertyName");
            var propType = prop.FindPropertyRelative("propertyType");

            DrawPropertyOverridePopup(prop, shaderPropNameProp, propType);

            materialArray[0] = materialInstanceProperty.objectReferenceValue as Material;
            MaterialProperty property = MaterialEditor.GetMaterialProperty(materialArray, shaderPropNameProp.stringValue); // Get the current material override's value.
            EditorGUI.BeginChangeCheck();
            _materialEditorForCopy.ShaderProperty(property, property.displayName);
            if(EditorGUI.EndChangeCheck())
            {
                TransferMaterialPropertyToPropertyOverride(property, prop);
            }
        }

        private void DrawPropertyOverridePopup(SerializedProperty propertyOverride, SerializedProperty shaderPropNameProp, SerializedProperty propertyTypeProp)
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
            if (EditorGUI.EndChangeCheck() || propertyTypeProp.enumValueIndex != (int)_shaderPropertyTypes[chosenIndex])
            {
                shaderPropNameProp.stringValue = _shaderPropertyNames[chosenIndex];
                var newEnumValueIndex = _shaderPropertyTypes[chosenIndex];

                if (propertyTypeProp.enumValueIndex != (int)newEnumValueIndex)
                {
                    Shader shaderBeingUsedInMaterialEditor = (materialInstanceProperty.objectReferenceValue as Material).shader;
                    propertyTypeProp.enumValueIndex = (int)newEnumValueIndex;

                    var floatValueProp = propertyOverride.FindPropertyRelative("_floatValue");
                    var colorValueProp = propertyOverride.FindPropertyRelative("_colorValue");
                    var vectorValueProp = propertyOverride.FindPropertyRelative("_vectorValue");
                    var textureReferenceProp = propertyOverride.FindPropertyRelative("_textureReference");
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

        private void TransferMaterialPropertyToPropertyOverride(MaterialProperty matProp, SerializedProperty prop)
        {
            var shaderPropName = prop.FindPropertyRelative("propertyName");
            var propType = prop.FindPropertyRelative("propertyType");

            var texRefProp = prop.FindPropertyRelative("_textureReference");

            switch((ShaderPropertyType)propType.enumValueIndex)
            {
                case ShaderPropertyType.Color:
                    prop.FindPropertyRelative("_colorValue").colorValue = matProp.colorValue;
                    break;
                case ShaderPropertyType.Int:
                case ShaderPropertyType.Range:
                case ShaderPropertyType.Float:
                    prop.FindPropertyRelative("_floatValue").floatValue = matProp.floatValue;
                    break;
                case ShaderPropertyType.Vector:
                    prop.FindPropertyRelative("_vectorValue").vector4Value = matProp.vectorValue;
                    break;
                case ShaderPropertyType.TexEnv:
                    var texProp = texRefProp.FindPropertyRelative("texture");
                    var scaleProp = texRefProp.FindPropertyRelative("scale");
                    var offsetProp = texRefProp.FindPropertyRelative("offset");

                    texProp.objectReferenceValue = matProp.textureValue;
                    scaleProp.vector2Value = new Vector2(matProp.textureScaleAndOffset[0], matProp.textureScaleAndOffset[1]);
                    offsetProp.vector2Value = new Vector2(matProp.textureScaleAndOffset[2], matProp.textureScaleAndOffset[3]);
                    break;
            }
        }

        [MenuItem("Assets/Create/MSU/Material Definition")]
        private static void CreateInstance()
        {
            var path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (!AssetDatabase.IsValidFolder(path))
                return;

            var assetPath = IOUtils.GenerateUniqueFileName(path, "New MaterialDefinition", ".asset");
            var instance = CreateInstance<MaterialDefinition>();
            instance.name = "New MaterialDefinition";

            AssetDatabase.CreateAsset(instance, assetPath);

            assetPath = AssetDatabase.GetAssetPath(instance);

            Material material = new Material(Shader.Find("StubbedRoR2/Base/Shaders/HGStandard"));
            material.name = "matNew MaterialDefinition";
            material.hideFlags = HideFlags.NotEditable;

            AssetDatabase.AddObjectToAsset(material, instance);

            AssetDatabase.SaveAssets();

            instance._material = material;

            EditorUtility.SetDirty(instance);
            AssetDatabase.SaveAssetIfDirty(instance);

            Selection.activeObject = instance;
            EditorGUIUtility.PingObject(instance);
        }

        public static void ApplyEditor(MaterialDefinition instance, bool updateRenderQueue = false)
        {
            if (!instance.originalMaterialAsset && instance.originalMaterialAddress.RuntimeKeyIsValid() == false)
            {
                MSULog.Warning($"Cannot apply overrides to {instance} as there's no original material asset or address!");
                return;
            }

            Material materialToUse = instance.originalMaterialAsset;
            bool isUsingAddressableMaterial = false;
            if (!materialToUse)
            {
                if (instance.originalMaterialAddress.RuntimeKeyIsValid() == false)
                {
                    MSULog.Warning($"Cannot apply overrides to {instance} as there's no original material asset or address!");
                    return;
                }

                var loadHandle = Addressables.LoadAssetAsync<Material>(instance.originalMaterialAddress.RuntimeKey);
                materialToUse = loadHandle.WaitForCompletion();

                if (loadHandle.Status == AsyncOperationStatus.Succeeded)
                {
                    materialToUse = loadHandle.Result;
                    isUsingAddressableMaterial = true;
                }
                else
                {
                    MSULog.Warning($"Cannot apply overrides to {instance}. The originalMaterialAddress failed to complete succesfully.");
                    return;
                }
            }

            //Copy the shader, if its an addressable material, store the stubbed shader.
            if(isUsingAddressableMaterial)
            {
                if (ShaderDictionary.addressableShaderNameToStubbed.TryGetValue(materialToUse.shader.name, out var stubbed) == false)
                {
                    if (ShaderDictionary.IsEmpty())
                    {
                        bool takeMeThere = EditorUtility.DisplayDialog("Shader Dictionary Empty", "It seems you're trying to use an addressable material in a Material Definition but the shader dictionary is empty. Consider opening the Shader Dictionary, clicking \"Add Default Stubbed Shaders\", followed by \"Reload Internal Dictionary\" and \"Save\". This will let you use the Addressable Material Shader in the editor", "Take me There", "Get me out of this material.");

                        if(takeMeThere)
                        {
                            ShaderDictionary.OpenSettings();
                        }
                        Selection.activeObject = null;
                        return;
                    }
                    MSULog.Warning($"Cannot apply overrides to {instance}. The loaded addressable material's shader doesnt have a stubbed counterpart in the project. You should file a bug report so the stubbed shaders are updated.");
                    return;
                }
                instance.material.shader = stubbed;
            }
            else
            {
                instance.material.shader = materialToUse.shader;
            }


            //Copy properties, if it's an addressable material, copy all properties except texture values.
            instance.material.CopyPropertiesFromMaterial(materialToUse);
            if(isUsingAddressableMaterial)
            {
                var props = MaterialEditor.GetMaterialProperties(new Material[] { instance.material });
                foreach(var prop in props)
                {
                    if(prop.type == MaterialProperty.PropType.Texture)
                    {
                        var textureValue = instance.material.GetTexture(prop.name);
                        if(textureValue)
                        {
                            instance.material.SetTexture(prop.name, null);
                        }
                    }
                }
            }
            else
            {
                instance.material.CopyPropertiesFromMaterial(materialToUse);
            }

            //this is set to true by the editor when the material reference changes.
            if(updateRenderQueue)
            {
                instance.renderQueue = materialToUse.renderQueue;
            }

            instance.material.renderQueue = instance.renderQueue;

            foreach (var propOverride in instance.propertyOverrides)
            {
                switch (propOverride.propertyType)
                {
                    case ShaderPropertyType.Color:
                        instance.material.SetColor(propOverride.propertyName, propOverride.GetValue<Color>());
                        break;
                    case ShaderPropertyType.Float:
                    case ShaderPropertyType.Range:
                        instance.material.SetFloat(propOverride.propertyName, propOverride.GetValue<float>());
                        break;
                    case ShaderPropertyType.Int:
                        instance.material.SetInt(propOverride.propertyName, (int)propOverride.GetValue<float>());
                        break;
                    case ShaderPropertyType.Vector:
                        instance.material.SetVector(propOverride.propertyName, propOverride.GetValue<Vector4>());
                        break;
                    case ShaderPropertyType.TexEnv:
                        var texRef = propOverride.GetValue<TextureReference>();
                        instance.material.SetTexture(propOverride.propertyName, texRef.texture);
                        instance.material.SetTextureOffset(propOverride.propertyName, texRef.offset);
                        instance.material.SetTextureScale(propOverride.propertyName, texRef.scale);
                        break;
                }
            }

            EditorUtility.SetDirty(instance._material);
            AssetDatabase.SaveAssetIfDirty(instance._material);
        }
    }
}