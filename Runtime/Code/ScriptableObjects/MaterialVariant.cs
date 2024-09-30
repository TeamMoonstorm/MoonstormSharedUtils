using UnityEngine;
using System.Linq;
using UnityEngine.Rendering;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MSU
{
    public class MaterialVariant : ScriptableObject
    {
#if UNITY_EDITOR
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

        [Serializable]
        internal struct SerializedMaterialProperty
        {
            public string propertyName;
            public UnityEditor.ShaderUtil.ShaderPropertyType propertyType;

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
                    case UnityEditor.ShaderUtil.ShaderPropertyType.Vector:
                        return _vectorValue;
                    case UnityEditor.ShaderUtil.ShaderPropertyType.Color:
                        return _colorValue;
                    case UnityEditor.ShaderUtil.ShaderPropertyType.Int:
                        return (int)_floatValue;
                    case UnityEditor.ShaderUtil.ShaderPropertyType.Range:
                    case UnityEditor.ShaderUtil.ShaderPropertyType.Float:
                        return _floatValue;
                    case UnityEditor.ShaderUtil.ShaderPropertyType.TexEnv:
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
                    case Color c when propertyType == UnityEditor.ShaderUtil.ShaderPropertyType.Color:
                        _colorValue = c;
                        break;
                    case Vector4 v when propertyType == UnityEditor.ShaderUtil.ShaderPropertyType.Vector:
                        _vectorValue = v;
                        break;
                    case float f when propertyType == UnityEditor.ShaderUtil.ShaderPropertyType.Range:
                        _floatValue = f;
                        break;
                    case float f when propertyType == UnityEditor.ShaderUtil.ShaderPropertyType.Float:
                        _floatValue = f;
                        break;
                    case int f when propertyType == UnityEditor.ShaderUtil.ShaderPropertyType.Int:
                        _floatValue = f;
                        break;
                    case TextureReference tr when propertyType == UnityEditor.ShaderUtil.ShaderPropertyType.TexEnv:
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
    }
}