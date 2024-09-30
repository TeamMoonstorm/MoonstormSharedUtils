using RoR2.Editor;
using System.Drawing;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using UnityEditor.VersionControl;
using UnityEngine;

namespace MSU.Editor.Inspectors
{
    [CustomEditor(typeof(MaterialVariant))]
    public class MaterialVariantInspector : IMGUIScriptableObjectInspector<MaterialVariant>
    {
        SerializedObject materialSO;

        protected override void DrawIMGUI()
        {
            materialSO ??= new SerializedObject(serializedObject.FindProperty("_material").objectReferenceValue);

            IMGUIUtil.DrawCheckableProperty(serializedObject.FindProperty("originalMaterial"), OnMaterialChanged);
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_material"));
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("_propertyOverrides"));
        }

        private void OnMaterialChanged(SerializedProperty prop)
        {
            var newMaterial = (Material)prop.objectReferenceValue;

            if(!newMaterial)
            {
                var savedProperties = materialSO.FindProperty("m_SavedProperties");
                savedProperties.FindPropertyRelative("m_TexEnvs").ClearArray();
                savedProperties.FindPropertyRelative("m_Ints").ClearArray();
                savedProperties.FindPropertyRelative("m_Floats").ClearArray();
                savedProperties.FindPropertyRelative("m_Colors").ClearArray();
                materialSO.FindProperty("m_InvalidKeywords").ClearArray();
                materialSO.FindProperty("m_Shader").objectReferenceValue = Shader.Find("StubbedRoR2/Base/Shaders/HGStandard");

                materialSO.ApplyModifiedProperties();
                return;
            }

            var mat = (Material)materialSO.targetObject;
            mat.CopyPropertiesFromMaterial(newMaterial);
            mat.shader = newMaterial.shader;
            mat.renderQueue = newMaterial.renderQueue;

            AssetDatabase.SaveAssetIfDirty(mat);
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