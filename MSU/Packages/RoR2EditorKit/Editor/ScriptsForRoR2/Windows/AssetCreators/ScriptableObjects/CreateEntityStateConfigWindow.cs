using HG.GeneralSerializer;
using RoR2;
using RoR2EditorKit.Common;
using RoR2EditorKit.Core.Windows;
using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace RoR2EditorKit.RoR2Related.EditorWindows
{
    public class CreateEntityStateConfigWindow : CreateRoR2ScriptableObjectWindow<EntityStateConfiguration>
    {
        public EntityStateConfiguration entityStateConfiguration;
        private bool deriveNameFromType = true;

        [MenuItem(Constants.RoR2EditorKitContextRoot + "ScriptableObjects/EntityStateConfiguration", false, Constants.RoR2EditorKitContextPriority)]
        public static void Open()
        {
            OpenEditorWindow<CreateEntityStateConfigWindow>(null, "Create Entity State Configuration");
        }

        protected override void OnWindowOpened()
        {
            base.OnWindowOpened();

            entityStateConfiguration = (EntityStateConfiguration)ScriptableObject;
            mainSerializedObject = new SerializedObject(entityStateConfiguration);
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical("box");

            deriveNameFromType = EditorGUILayout.Toggle("Set AssetName from Type Name", deriveNameFromType);

            if (!deriveNameFromType)
            {
                nameField = EditorGUILayout.TextField("Asset Name", nameField);
            }

            DrawField("targetType");

            if (SimpleButton("Create EntityStateConfiguration"))
            {
                var result = CreateEntityStateConfiguration();
                if (result)
                {
                    Debug.Log($"Succesfully Created EntityStateConfiguration {entityStateConfiguration.name}");
                    TryToClose();
                }
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }

        private bool CreateEntityStateConfiguration()
        {
            try
            {
                var targetType = mainSerializedObject.FindProperty("targetType");
                var assemblyQualifiedName = targetType.FindPropertyRelative("assemblyQualifiedName").stringValue;

                Type stateType = Type.GetType(assemblyQualifiedName);
                if (stateType != null)
                {
                    var allFieldsInType = stateType.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                    var filteredFields = allFieldsInType.Where(fieldInfo =>
                    {
                        bool canSerialize = SerializedValue.CanSerializeField(fieldInfo);
                        bool shouldSerialize = !fieldInfo.IsStatic || (fieldInfo.DeclaringType == stateType);
                        return canSerialize && shouldSerialize;
                    });

                    var staticFields = filteredFields.Where(fieldInfo => fieldInfo.IsStatic);
                    var instanceFields = filteredFields.Where(fieldInfo => !fieldInfo.IsStatic);


                    FieldInfo[] fieldsToSerialize = staticFields.Union(instanceFields).ToArray();

                    Debug.LogWarning(fieldsToSerialize.Length);

                    for (int i = 0; i < fieldsToSerialize.Length; i++)
                    {
                        entityStateConfiguration.serializedFieldsCollection.GetOrCreateField(fieldsToSerialize[i].Name);
                    }

                    if (deriveNameFromType)
                        entityStateConfiguration.SetNameFromTargetType();
                    else
                        entityStateConfiguration.name = nameField;

                    Util.CreateAssetAtSelectionPath(entityStateConfiguration);

                    return true;
                }
                else
                {
                    throw new NullReferenceException($"could not find type {targetType}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error while creating Entity State Configuration: {e}");
                return false;
            }
        }
    }
}