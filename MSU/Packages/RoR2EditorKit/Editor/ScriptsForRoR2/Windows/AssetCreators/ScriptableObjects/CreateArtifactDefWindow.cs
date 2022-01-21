using RoR2;
using RoR2EditorKit.Common;
using RoR2EditorKit.Core;
using RoR2EditorKit.Core.Windows;
using System;
using UnityEditor;
using UnityEngine;

namespace RoR2EditorKit.RoR2Related.EditorWindows
{
    public class CreateArtifactDefWindow : CreateRoR2ScriptableObjectWindow<ArtifactDef>
    {
        public ArtifactDef artifactDef;

        private bool createPickupPrefab;
        private Mesh prefabMesh;
        private Material prefabMaterial;

        private bool drawExtraSettings;
        private bool drawPrefabSettings;

        [MenuItem(Constants.RoR2EditorKitContextRoot + "ScriptableObjects/ArtifactDef", false, Constants.RoR2EditorKitContextPriority)]
        public static void Open()
        {
            OpenEditorWindow<CreateArtifactDefWindow>(null, "Create Artifact");
        }

        protected override void OnWindowOpened()
        {
            base.OnWindowOpened();

            artifactDef = (ArtifactDef)ScriptableObject;
            artifactDef.smallIconDeselectedSprite = Constants.NullSprite;
            artifactDef.smallIconSelectedSprite = Constants.NullSprite;
            prefabMesh = Constants.NullMesh;
            prefabMaterial = Constants.NullMaterial;
            createPickupPrefab = false;
            mainSerializedObject = new SerializedObject(artifactDef);
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical("box");

            nameField = EditorGUILayout.TextField("Artifact Name", nameField);

            SwitchButton("Extra Settings", ref drawExtraSettings);

            if (drawExtraSettings)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.BeginVertical("box");
                DrawExtraArtifactSettings();
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
            }

            createPickupPrefab = EditorGUILayout.Toggle("Create Pickup Prefab", createPickupPrefab);

            if (createPickupPrefab)
            {
                SwitchButton("PrefabSettings", ref drawPrefabSettings);

                if (drawPrefabSettings)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.BeginVertical("box");
                    DrawPrefabSettings();
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                }
            }

            if (GUILayout.Button("Create Artifact"))
            {
                var result = CreateArtifact();
                if (result)
                {
                    Debug.Log($"Succesfully Created Artifact {nameField}");
                    TryToClose();
                }
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            ApplyChanges();
        }

        private void DrawExtraArtifactSettings()
        {
            DrawField("unlockableDef");
            DrawField("smallIconSelectedSprite");
            DrawField("smallIconDeselectedSprite");
        }

        private void DrawPrefabSettings()
        {
            prefabMaterial = (Material)EditorGUILayout.ObjectField("Material", prefabMaterial, typeof(Material), false);
            prefabMesh = (Mesh)EditorGUILayout.ObjectField("Mesh", prefabMesh, typeof(Mesh), false);
        }

        private bool CreateArtifact()
        {
            actualName = GetCorrectAssetName(nameField);
            try
            {
                if (string.IsNullOrEmpty(actualName))
                    throw ErrorShorthands.ThrowNullAssetName(nameof(nameField));

                artifactDef.cachedName = actualName;

                if (string.IsNullOrEmpty(Settings.TokenPrefix))
                    throw ErrorShorthands.ThrowNullTokenPrefix();

                var tokenPrefix = $"{Settings.TokenPrefix}_ARTIFACT_{actualName.ToUpperInvariant()}_";
                artifactDef.nameToken = tokenPrefix + "NAME";
                artifactDef.descriptionToken = tokenPrefix + "DESC";

                artifactDef.pickupModelPrefab = createPickupPrefab ? CreatePickupPrefab() : Constants.NullPrefab;

                Util.CreateAssetAtSelectionPath(artifactDef);

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error while creating artifact: {e}");
                return false;
            }
        }
        private GameObject CreatePickupPrefab()
        {
            var pickup = new GameObject($"Pickup{actualName}");
            var mdl = Util.CreateGenericPrefab("mdl" + actualName, prefabMesh, prefabMaterial);

            Util.AddTransformToParent(mdl, pickup);

            var boxCollider = mdl.GetComponent<BoxCollider>();
            DestroyImmediate(boxCollider);

            return Util.CreatePrefabAtSelectionPath(pickup);
        }
    }
}
