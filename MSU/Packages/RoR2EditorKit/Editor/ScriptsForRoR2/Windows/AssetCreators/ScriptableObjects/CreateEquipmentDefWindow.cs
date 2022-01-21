using RoR2;
using RoR2EditorKit.Common;
using RoR2EditorKit.Core;
using RoR2EditorKit.Core.Windows;
using System;
using UnityEditor;
using UnityEngine;

namespace RoR2EditorKit.RoR2Related.EditorWindows
{
    public class CreateEquipmentDefWindow : CreateRoR2ScriptableObjectWindow<EquipmentDef>
    {
        public EquipmentDef equipDef;

        private bool createPickupPrefab;
        private bool createItemDisplayPrefab;
        private Mesh prefabMesh = null;
        private Material prefabMaterial = null;

        private bool drawExtraSettings = false;

        private bool drawPrefabSettings = false;

        [MenuItem(Constants.RoR2EditorKitContextRoot + "ScriptableObjects/EquipmentDef", false, Constants.RoR2EditorKitContextPriority)]
        public static void Open()
        {
            OpenEditorWindow<CreateEquipmentDefWindow>(null, "Create Equipment");
        }

        protected override void OnWindowOpened()
        {
            base.OnWindowOpened();

            equipDef = (EquipmentDef)ScriptableObject;
            equipDef.pickupIconSprite = Constants.NullSprite;
            prefabMesh = Constants.NullMesh;
            prefabMaterial = Constants.NullMaterial;
            createPickupPrefab = false;
            createItemDisplayPrefab = false;
            mainSerializedObject = new SerializedObject(equipDef);
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical("box");

            nameField = EditorGUILayout.TextField("Equipment Name", nameField);
            DrawField("cooldown");
            DrawField("enigmaCompatible");
            DrawField("isLunar");

            SwitchButton("Extra Settings", ref drawExtraSettings);

            if (drawExtraSettings)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.BeginVertical("box");
                DrawExtraSettings();
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
            }

            createItemDisplayPrefab = EditorGUILayout.Toggle("Create Display Prefab", createItemDisplayPrefab);
            createPickupPrefab = EditorGUILayout.Toggle("Create Pickup Prefab", createPickupPrefab);

            if (createItemDisplayPrefab || createPickupPrefab)
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

            if (GUILayout.Button($"Create Equiipment"))
            {
                var result = CreateEquipment();
                if (result)
                {
                    Debug.Log($"Succesfully Created Item {nameField}");
                    TryToClose();
                }
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            ApplyChanges();
        }

        private void DrawExtraSettings()
        {
            DrawField("pickupIconSprite");
            DrawField("unlockableDef");
            DrawField("colorIndex");
            DrawField("canDrop");
            DrawField("isBoss");
            DrawField("passiveBuffDef");
            DrawField("appearsInSinglePlayer");
            DrawField("appearsInMultiPlayer");
        }

        private void DrawPrefabSettings()
        {
            prefabMaterial = (Material)EditorGUILayout.ObjectField("Material", prefabMaterial, typeof(Material), false);
            prefabMesh = (Mesh)EditorGUILayout.ObjectField("Mesh", prefabMesh, typeof(Mesh), false);
        }

        private bool CreateEquipment()
        {
            actualName = GetCorrectAssetName(nameField);
            try
            {
                if (string.IsNullOrEmpty(actualName))
                    throw ErrorShorthands.ThrowNullAssetName(nameof(nameField));

                equipDef.name = actualName;

                if (string.IsNullOrEmpty(Settings.TokenPrefix))
                    throw ErrorShorthands.ThrowNullTokenPrefix();

                var tokenPrefix = $"{Settings.TokenPrefix}_EQUIP_{actualName.ToUpperInvariant()}_";
                equipDef.nameToken = tokenPrefix + "NAME";
                equipDef.pickupToken = tokenPrefix + "PICKUP";
                equipDef.descriptionToken = tokenPrefix + "DESC";
                equipDef.loreToken = tokenPrefix + "LORE";

                equipDef.pickupModelPrefab = createPickupPrefab ? CreatePickupPrefab() : Constants.NullPrefab;

                if (createItemDisplayPrefab)
                    CreateItemDisplayPrefab();

                Util.CreateAssetAtSelectionPath(equipDef);

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error while creating item: {e}");
                return false;
            }
        }

        private GameObject CreatePickupPrefab()
        {
            var pickup = new GameObject("Pickup" + actualName);
            var mdl = Util.CreateGenericPrefab("mdl" + actualName, prefabMesh, prefabMaterial);

            //Parents prefabs
            Util.AddTransformToParent(mdl, pickup);

            //Destroy box collider
            var boxCollider = mdl.GetComponent<BoxCollider>();
            DestroyImmediate(boxCollider);

            return Util.CreatePrefabAtSelectionPath(pickup);
        }

        private GameObject CreateItemDisplayPrefab()
        {
            //Creates game objects
            var display = new GameObject($"Display{actualName}");
            var mdl = Util.CreateGenericPrefab("mdl" + actualName, prefabMesh, prefabMaterial);

            //Parents mdl rpefab to parentPrefab
            Util.AddTransformToParent(mdl, display);

            //Destroy uneeded components from mdl prefab
            var boxCollider = mdl.GetComponent<BoxCollider>();
            DestroyImmediate(boxCollider);

            //Add ItemDisplay component to parent prefab
            var itemDisplay = display.AddComponent<ItemDisplay>();
            var meshRenderer = mdl.GetComponent<MeshRenderer>();

            Array.Resize(ref itemDisplay.rendererInfos, 1);
            itemDisplay.rendererInfos[0].defaultMaterial = meshRenderer.sharedMaterial;
            itemDisplay.rendererInfos[0].renderer = meshRenderer;

            return Util.CreatePrefabAtSelectionPath(display);
        }
    }
}
