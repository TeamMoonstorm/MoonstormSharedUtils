using UnityEngine;
using System.Linq;
using UnityEngine.Rendering;
using System;
using System.Collections.Generic;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MSU
{
    /// <summary>
    /// A <see cref="MaterialVariant"/> is a <see cref="ScriptableObject"/> that allows you to define and create a variant for an Existing material.
    /// <br>When created in the editor, the MaterialVariant will create a sub-asset material, which will be marked as not editable, this sub-asset is the variant itself, selecting the ScriptableObject allows you to define an original material, which when assigned, all it's values will be copied over to the Material Variant.</br>
    /// <br>When the original material is present, you'll be able to specify Shader Property Overrides, which in turn are applied to the material variant.</br>
    /// <para>It has complete support for any and all shaders used within the project, if the original material is using MSU's AddressableMaterialShader, said addressable material's shader will be used to obtain the overridable properties, and the property overrides will be applied at runtime using the method <see cref="ApplyOverrides"/></para>
    /// </summary>
    public class MaterialVariant : ScriptableObject
    {
#if UNITY_EDITOR
        /// <summary>
        /// This is an Editor only List.
        /// 
        /// <para>Contains all the instances of MaterialVariants, used for making sure the variant always has correct values when compared between the original material and the specified overrides.</para>
        /// </summary>
        public static List<MaterialVariant> instances = new List<MaterialVariant>();
        /// <summary>
        /// This is an Editor only field
        /// 
        /// <para>Defines the original material used for the creation of the variant.</para>
        /// </summary>
        public Material originalMaterial;
#endif
        /// <summary>
        /// Access to the material variant itself.
        /// </summary>
        public Material material => _material;
        [Tooltip("The material variant itself, cannot be overriden or edited directly, instead utilize the \"Property Overrides\" field")]
        [SerializeField] internal Material _material;

        [Tooltip("A list of Property Overrides to apply to the Material Variant.")]
        [SerializeField] public SerializedMaterialProperty[] _propertyOverrides;

        private void OnValidate()
        {
            var nameCheck = $"{name}";
            if(!nameCheck.StartsWith("mat"))
            {
                nameCheck = $"mat{nameCheck}";
            }

            if(_material.name != nameCheck)
            {
                _material.name = nameCheck;
#if UNITY_EDITOR
                AssetDatabase.SaveAssetIfDirty(_material);
#endif
            }
        }

        /// <summary>
        /// Applies any and all overrides found within the <see cref="MaterialVariant"/>'s property voerrides.
        /// <para>This is a Coroutine, as it awaits until the <see cref="material"/>'s shader is not MSU's Addressable Shader, this method should be called either after you load all your materials to swap them using <see cref="ShaderUtil.LoadAddressableMaterialShadersAsync(AssetBundle)"/>'s methods, or on a <see cref="ParallelCoroutine"/>/<see cref="ParallelMultiStartCoroutine"/> to ensure the while condition eventually returns false.</para>
        /// </summary>
        /// <returns>A Coroutine that can be awaited.</returns>
        public IEnumerator ApplyOverrides()
        {
            while (_material.shader.name == "MSU/AddressableMaterialShader")
            {
                yield return new WaitForEndOfFrame();
            }

            foreach (var propOverride in _propertyOverrides)
            {
                switch (propOverride.propertyType)
                {
                    case ShaderPropertyType.Color:
                        _material.SetColor(propOverride.propertyName, propOverride.GetValue<Color>());
                        break;
                    case ShaderPropertyType.Float:
                    case ShaderPropertyType.Range:
                        _material.SetFloat(propOverride.propertyName, propOverride.GetValue<float>());
                        break;
                    case ShaderPropertyType.Int:
                        _material.SetInt(propOverride.propertyName, (int)propOverride.GetValue<float>());
                        break;
                    case ShaderPropertyType.Vector:
                        _material.SetVector(propOverride.propertyName, propOverride.GetValue<Vector4>());
                        break;
                    case ShaderPropertyType.TexEnv:
                        var texRef = propOverride.GetValue<TextureReference>();
                        _material.SetTexture(propOverride.propertyName, texRef.texture);
                        _material.SetTextureOffset(propOverride.propertyName, texRef.offset);
                        _material.SetTextureScale(propOverride.propertyName, texRef.scale);
                        break;
                }
            }
        }

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
                switch(propertyType)
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

#if UNITY_EDITOR
        /// <summary>
        /// This is an Editor Only Method.
        /// 
        /// <para>Ensures the MaterialVariant's properties are up to date, by copying over all the data from the original material, then applying any overrides present.</para>
        /// </summary>
        public void ApplyEditor()
        {
            if(!originalMaterial)
                return;
                
            _material.shader = originalMaterial.shader;
            _material.CopyPropertiesFromMaterial(originalMaterial);
            _material.renderQueue = originalMaterial.renderQueue;

            foreach(var propOverride in _propertyOverrides)
            {
                switch (propOverride.propertyType)
                {
                    case ShaderPropertyType.Color:
                        _material.SetColor(propOverride.propertyName, propOverride.GetValue<Color>());
                        break;
                    case ShaderPropertyType.Float:
                    case ShaderPropertyType.Range:
                        _material.SetFloat(propOverride.propertyName, propOverride.GetValue<float>());
                        break;
                    case ShaderPropertyType.Int:
                        _material.SetInt(propOverride.propertyName, (int)propOverride.GetValue<float>());
                        break;
                    case ShaderPropertyType.Vector:
                        _material.SetVector(propOverride.propertyName, propOverride.GetValue<Vector4>());
                        break;
                    case ShaderPropertyType.TexEnv:
                        var texRef = propOverride.GetValue<TextureReference>();
                        _material.SetTexture(propOverride.propertyName, texRef.texture);
                        _material.SetTextureOffset(propOverride.propertyName, texRef.offset);
                        _material.SetTextureScale(propOverride.propertyName, texRef.scale);
                        break;
                }
            }
            EditorUtility.SetDirty(_material);
        }

        /// <summary>
        /// This is an Editor Only Constructor.
        /// 
        /// <para>This adds the instance to the internal <see cref="instances"/> field. Remember that ScriptableObjects should not be created this way, instead use <see cref="ScriptableObject.CreateInstance{T}()"/>, which will eventually call this constructor.</para>
        /// </summary>
        public MaterialVariant()
        {
            instances.Add(this);
        }
#endif
    }
}