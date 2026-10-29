using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Collections;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Collections.Generic;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MSU
{
    /// <summary>
    /// A <see cref="MaterialDefinition"/> is a <see cref="ScriptableObject"/> that allows you to define and create materials via custom properties. It is a replacement for <see cref="MaterialVariant"/> and the "AddressableMaterialShader" system in MSU 2.0
    /// <br></br>
    /// When created in the editor, the MaterialDefinition will create a sub-asset material, which will be marked as not editable, this sub-asset is the material itself, selecting the ScriptableObject allows you to define an original material via either a direct material reference (<see cref="originalMaterialAsset"/>), or via an addressable material reference (<see cref="originalMaterialAddress"/>). Once assigned, all it's values will be copied over to the MAterial Definition's sub-asset material.
    /// <br></br>
    /// When the original material is present, you'll be able to specify Shader Property Overrides, which in turn are applied to the material variant.
    /// <para></para>
    /// To migrate from the old deprecated systems to a material definition, you can do the following for each type:
    /// <list type="bullet">
    /// <item>Material Variant: Create a Material Definition, assign <see cref="originalMaterialAsset"/> or <see cref="originalMaterialAddress"/>, and apply properties.</item>
    /// <item>AddressableMaterialShader: Create a Material Definition, and assign <see cref="originalMaterialAddress"/></item>
    /// </list>
    /// </summary>
    public sealed class MaterialDefinition : ScriptableObject
    {        
        /// <summary>
        /// Represents a Shader Property that can be serialized within a <see cref="MaterialVariant"/>
        /// </summary>
        [Serializable]
        public struct SerializedMaterialProperty
        {
            [Tooltip("The Shader Property's name")]
            public string propertyName;
            [Tooltip("The type of the Shader's Property")]
            public ShaderPropertyType propertyType;

            [Tooltip("The Float value stored within the property")]
            [SerializeField]
            private float _floatValue;

            [Tooltip("The Color value stored within the property")]
            [SerializeField]
            private Color _colorValue;

            [Tooltip("The Vector4 value stored within the property")]
            [SerializeField]
            private Vector4 _vectorValue;

            [Tooltip("The Texture Reference stored wtihin the property")]
            [SerializeField]
            private TextureReference _textureReference;

            /// <summary>
            /// Obtains the value stored within this <see cref="SerializedMaterialProperty"/>
            /// </summary>
            /// <returns>The Serialized value boxed as an <see cref="System.Object"/>, the value in <see cref="propertyType"/> can be used to cast this boxed object safely.</returns>
            public object GetValue()
            {
                switch (propertyType)
                {
                    case ShaderPropertyType.Vector:
                        return _vectorValue;
                    case ShaderPropertyType.Color:
                        return _colorValue;
                    case ShaderPropertyType.Int:
                        return (int)_floatValue;
                    case ShaderPropertyType.Range:
                    case ShaderPropertyType.Float:
                        return _floatValue;
                    case ShaderPropertyType.TexEnv:
                        return _textureReference;
                }
                return null;
            }

            /// <summary>
            /// Retrieves the value stored wtihin this <see cref="SerializedMaterialProperty"/>, casted to <typeparamref name="T"/>
            /// </summary>
            /// <typeparam name="T">The type of value</typeparam>
            /// <returns>The Serialized value, the value in <see cref="propertyType"/> can be used to specify the value for <typeparamref name="T"/></returns>
            public T GetValue<T>() where T : struct
            {
                return (T)GetValue();
            }

            /// <summary>
            /// Assigns the value <paramref name="value"/> to this <see cref="SerializedMaterialProperty"/>
            /// <para>This does not set the <see cref="propertyType"/>, if the <see cref="propertyType"/> does not match the value of <typeparamref name="T"/>, no value will be set.</para>
            /// </summary>
            /// <typeparam name="T">The type of value to set</typeparam>
            /// <param name="value">The value itself</param>
            public void SetValue<T>(T value)
            {
                switch (value)
                {
                    case Color c when propertyType == ShaderPropertyType.Color:
                        _colorValue = c;
                        break;
                    case Vector4 v when propertyType == ShaderPropertyType.Vector:
                        _vectorValue = v;
                        break;
                    case float f when propertyType == ShaderPropertyType.Range:
                        _floatValue = f;
                        break;
                    case float f when propertyType == ShaderPropertyType.Float:
                        _floatValue = f;
                        break;
                    case int f when propertyType == ShaderPropertyType.Int:
                        _floatValue = f;
                        break;
                    case TextureReference tr when propertyType == ShaderPropertyType.TexEnv:
                        _textureReference = tr;
                        break;
                }
            }
        }

        /// <summary>
        /// Represents a Texture property from a shader
        /// </summary>
        [Serializable]
        public struct TextureReference
        {
            [Tooltip("The texture itself.")]
            public Texture texture;
            [Tooltip("The vertical and horizontal offset for the texture.")]
            public Vector2 offset;
            [Tooltip("The vertical and horizontal scaling for the texture.")]
            public Vector2 scale;
        }

        /// <summary>
        /// Represents the types of property a shader can use.
        /// </summary>
        public enum ShaderPropertyType
        {
            /// <summary>
            /// Represents a <see cref="UnityEngine.Color"/> property
            /// </summary>
            Color,
            /// <summary>
            /// Represents a <see cref="Vector4"/> property
            /// </summary>
            Vector,
            /// <summary>
            /// Represents a float property
            /// </summary>
            Float,
            /// <summary>
            /// Represents a float property that has minimum and maximum values.
            /// </summary>
            Range,
            /// <summary>
            /// Represents a Texture property that has Scaling and Offset values.
            /// </summary>
            TexEnv,
            /// <summary>
            /// Represents an int property, internally shaders only use floats, so the value for the float will be rounded to the nearest valid integer.
            /// </summary>
            Int
        }

        [Tooltip("A direct reference to the original material, when assigned, all of it's properties will be assigned to the material instance field.")]
        public Material originalMaterialAsset;
        [Tooltip("An addressable reference to the original material, when assigned, all of it's properties except texture references will be assigned to the material instance field.")]
        public AssetReferenceT<Material> originalMaterialAddress = new AssetReferenceT<Material>("");

        /// <summary>
        /// Access to the defined material by the MaterialDefinition
        /// </summary>
        public Material material => _material;
        [Tooltip("The material variant itself, cannot be overriden or edited directly, utilize the \"Property Overrides\" field instead.")]
        [SerializeField, ReadOnly]
        internal Material _material;

        [Space(10)]
        [Tooltip("Used to override the render queue of the original material.")]
        public int renderQueue;
        [Tooltip("An array of SerializedMaterialProperties, used to override shader properties for the material definition")]
        public SerializedMaterialProperty[] propertyOverrides = Array.Empty<SerializedMaterialProperty>();

        private void OnValidate()
        {
            var nameCheck = $"{name}";
            if(!nameCheck.StartsWith("mat"))
            {
                MSULog.Warning($"By convention, all material assets should start with \"mat\"");
            }

            if(_material.name != nameCheck)
            {
                _material.name = nameCheck;

#if UNITY_EDITOR
                EditorUtility.SetDirty(_material);
                EditorUtility.SetDirty(this);
                AssetDatabase.SaveAssetIfDirty(this);
                AssetDatabase.SaveAssetIfDirty(_material);
#endif
            }
        }

        public IEnumerator ApplyOverrides()
        {
            if(!originalMaterialAsset || originalMaterialAddress.RuntimeKeyIsValid() == false)
            {
                MSULog.Warning($"Cannot apply overrides to {this} as there's no original material asset or address!");
            }

            Material materialToUse = originalMaterialAsset;
            AsyncOperationHandle<Material>? loadHandle = null;
            if(!originalMaterialAsset)
            {
                if(originalMaterialAddress.RuntimeKeyIsValid() == false)
                {
                    MSULog.Warning($"Cannot apply overrides to {this} as there's no original material asset or address!");
                    yield break;
                }

                loadHandle = Addressables.LoadAssetAsync<Material>(originalMaterialAddress.RuntimeKey);
                yield return loadHandle;

                if(loadHandle.Value.Status == AsyncOperationStatus.Succeeded)
                {
                    materialToUse = loadHandle.Value.Result;
                }
                else
                {
                    MSULog.Warning($"Cannot apply overrides to {this}. The originalMaterialAddress failed to complete succesfully.");
                    yield break;
                }
            }

            material.shader = materialToUse.shader;
            material.CopyPropertiesFromMaterial(materialToUse);
            material.renderQueue = renderQueue;

            foreach(var propOverride in propertyOverrides)
            {
                switch(propOverride.propertyType)
                {
                    case ShaderPropertyType.Color:
                        material.SetColor(propOverride.propertyName, propOverride.GetValue<Color>());
                        break;
                    case ShaderPropertyType.Float:
                    case ShaderPropertyType.Range:
                        material.SetFloat(propOverride.propertyName, propOverride.GetValue<float>());
                        break;
                    case ShaderPropertyType.Int:
                        material.SetInt(propOverride.propertyName, (int)propOverride.GetValue<float>());
                        break;
                    case ShaderPropertyType.Vector:
                        material.SetVector(propOverride.propertyName, propOverride.GetValue<Vector4>());
                        break;
                    case ShaderPropertyType.TexEnv:
                        var texRef = propOverride.GetValue<TextureReference>();
                        material.SetTexture(propOverride.propertyName, texRef.texture);
                        material.SetTextureOffset(propOverride.propertyName, texRef.offset);
                        material.SetTextureScale(propOverride.propertyName, texRef.scale);
                        break;
                }
            }
        }
    }
}