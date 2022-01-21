using RoR2;
using RoR2EditorKit.Common;
using RoR2EditorKit.Core;
using RoR2EditorKit.Core.Windows;
using System;
using UnityEditor;
using UnityEngine;

namespace RoR2EditorKit.RoR2Related.EditorWindows
{
    public class CreateSurvivorDefWindow : CreateRoR2ScriptableObjectWindow<SurvivorDef>
    {
        public SurvivorDef survivor;

        private bool drawExtraSettings;
        private bool deriveNameFromBodyPrefab;
        private bool deriveDisplayPrefabFromBody;

        [MenuItem(Constants.RoR2EditorKitContextRoot + "ScriptableObjects/SurvivorDef", false, Constants.RoR2EditorKitContextPriority)]
        public static void Open()
        {
            OpenEditorWindow<CreateSurvivorDefWindow>(null, "Create Survivor");
        }

        protected override void OnWindowOpened()
        {
            base.OnWindowOpened();

            survivor = ScriptableObject;

            drawExtraSettings = false;
            deriveNameFromBodyPrefab = false;
            survivor.bodyPrefab = Constants.NullPrefab;
            survivor.displayPrefab = Constants.NullPrefab;

            mainSerializedObject = new SerializedObject(survivor);
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical("box");

            deriveNameFromBodyPrefab = EditorGUILayout.Toggle("Set SurvivorDef Name from BodyPrefab Name", deriveNameFromBodyPrefab);

            if (!deriveNameFromBodyPrefab)
            {
                nameField = EditorGUILayout.TextField("Survivor Name", nameField);
            }

            deriveDisplayPrefabFromBody = EditorGUILayout.Toggle("Create DisplayPrefab from BodyPrefab", deriveDisplayPrefabFromBody);

            DrawField("bodyPrefab");
            if (!deriveDisplayPrefabFromBody)
                DrawField("displayPrefab");

            DrawField("primaryColor");
            DrawField("desiredSortPosition");

            SwitchButton("Extra Settings", ref drawExtraSettings);
            if (drawExtraSettings)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.BeginVertical("box");
                DrawExtraSurvivorDefSettings();
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
            }

            if (SimpleButton("Create Survivor"))
            {
                var result = CreateSurivorDef();
                if (result)
                {
                    Debug.Log($"Succesfully Created Survivor {survivor.cachedName}");
                    TryToClose();
                }
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            ApplyChanges();
        }

        private void DrawExtraSurvivorDefSettings()
        {
            DrawField("unlockableDef");
            DrawField("hidden");
        }

        private bool CreateSurivorDef()
        {
            actualName = deriveNameFromBodyPrefab ? survivor.bodyPrefab.name.Replace("Body", "") : GetCorrectAssetName(nameField);
            try
            {
                survivor.cachedName = actualName;

                if (string.IsNullOrEmpty(Settings.TokenPrefix))
                    throw ErrorShorthands.ThrowNullTokenPrefix();

                var tokenPrefix = $"{Settings.TokenPrefix}_{actualName.ToUpperInvariant()}_";
                survivor.displayNameToken = $"{tokenPrefix}BODY_NAME";
                survivor.descriptionToken = $"{tokenPrefix}DESCRIPTION";
                survivor.outroFlavorToken = $"{tokenPrefix}OUTRO_FLAVOR";
                survivor.mainEndingEscapeFailureFlavorToken = $"{tokenPrefix}MAIN_ENDING_ESCAPE_FAILURE_FLAVOR";

                if (deriveDisplayPrefabFromBody)
                    survivor.displayPrefab = CreateDisplayPrefabFromBody();

                Util.CreateAssetAtSelectionPath(survivor);

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error while creating SurvivorDef: {e}");
                return false;
            }
        }
        private GameObject CreateDisplayPrefabFromBody()
        {
            return null;
        }
    }
}