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
    public class MaterialVariant : ScriptableObject
    {
#if UNITY_EDITOR
        public static List<MaterialVariant> _instances = new List<MaterialVariant>();
        public Material originalMaterial;
#endif

        [SerializeField] internal Material _material;
        [SerializeField] private SerializedMaterialProperty[] _propertyOverrides;

        private void OnValidate()
        {
            var nameCheck = $"mat{name}";
            if(_material.name != nameCheck)
            {
                _material.name = nameCheck;
#if UNITY_EDITOR
                AssetDatabase.SaveAssetIfDirty(_material);
#endif
            }
        }

        public IEnumerator ApplyOverrides()
        {
            while (_material.shader.name == "MSU/AddressableMaterialShader")
            {
                yield return null;
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

        [Serializable]
        internal struct SerializedMaterialProperty
        {
            public string propertyName;
            public ShaderPropertyType propertyType;

            [SerializeField]
            private float _floatValue;
            [SerializeField]
            private Color _colorValue;
            [SerializeField]
            private Vector4 _vectorValue;
            [SerializeField]
            private TextureReference _textureReference;

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

            public T GetValue<T>() where T : struct
            {
                return (T)GetValue();
            }

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

        [Serializable]
        internal struct TextureReference
        {
            public Texture texture;
            public Vector2 offset;
            public Vector2 scale;
        }

        internal enum ShaderPropertyType
        {
            //
            // Summary:
            //     Color Property.
            Color,
            //
            // Summary:
            //     Vector Property.
            Vector,
            //
            // Summary:
            //     Float Property.
            Float,
            //
            // Summary:
            //     Range Property.
            Range,
            //
            // Summary:
            //     Texture Property.
            TexEnv,
            //
            // Summary:
            //     Int Property.
            Int
        }

#if UNITY_EDITOR
        public void ApplyEditor()
        {
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
        public MaterialVariant()
        {
            _instances.Add(this);
        }
#endif
    }
}