using RoR2;
using RoR2EditorKit.Common;
using RoR2EditorKit.Core;
using RoR2EditorKit.Core.Windows;
using System;
using UnityEditor;
using UnityEngine;

namespace RoR2EditorKit.RoR2Related.EditorWindows
{
    public class CreateItemDefWindow : CreateRoR2ScriptableObjectWindow<ItemDef>
    {
        public ItemDef itemDef;

        private bool createPickupPrefab;
        private bool createItemDisplayPrefab;
        private Mesh prefabMesh = null;
        private Material prefabMaterial = null;

        private bool drawExtraSettings = false;
        private bool drawPrefabSettings = false;

        [MenuItem(Constants.RoR2EditorKitContextRoot + "ScriptableObjects/ItemDef", false, Constants.RoR2EditorKitContextPriority)]
        public static void Open()
        {
            OpenEditorWindow<CreateItemDefWindow>(null, "Create Item");
        }
        protected override void OnWindowOpened()
        {
            base.OnWindowOpened();

            itemDef = (ItemDef)ScriptableObject;
            itemDef.pickupIconSprite = Constants.NullSprite;
            prefabMesh = Constants.NullMesh;
            prefabMaterial = Constants.NullMaterial;
            createPickupPrefab = false;
            createItemDisplayPrefab = false;
            mainSerializedObject = new SerializedObject(itemDef);
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical("box");

            nameField = EditorGUILayout.TextField("Item Name", nameField);

            DrawField(mainSerializedObject.FindProperty("tier"), true, "Item Tier");

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.LabelField($"Item Tags");
            DrawValueSidebar(mainSerializedObject.FindProperty("tags"));

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            SwitchButton("Extra Settings", ref drawExtraSettings);

            if (drawExtraSettings)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.BeginVertical("box");
                DrawExtraItemSettings();
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

            if (GUILayout.Button($"Create Item"))
            {
                var result = CreateItem();
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

        private void DrawExtraItemSettings()
        {
            DrawField("unlockableDef");
            DrawField("pickupIconSprite");
            DrawField("hidden");
            DrawField("canRemove");
        }

        private void DrawPrefabSettings()
        {
            prefabMaterial = (Material)EditorGUILayout.ObjectField("Material", prefabMaterial, typeof(Material), false);
            prefabMesh = (Mesh)EditorGUILayout.ObjectField("Mesh", prefabMesh, typeof(Mesh), false);
        }

        private bool CreateItem()
        {
            actualName = GetCorrectAssetName(nameField);
            try
            {
                if (string.IsNullOrEmpty(actualName))
                    throw ErrorShorthands.ThrowNullAssetName(nameof(nameField));

                itemDef.name = actualName;

                if (string.IsNullOrEmpty(Settings.TokenPrefix))
                    throw ErrorShorthands.ThrowNullTokenPrefix();

                var tokenPrefix = $"{Settings.TokenPrefix}_ITEM_{actualName.ToUpperInvariant()}_";
                itemDef.nameToken = tokenPrefix + "NAME";
                itemDef.pickupToken = tokenPrefix + "PICKUP";
                itemDef.descriptionToken = tokenPrefix + "DESC";
                itemDef.loreToken = tokenPrefix + "LORE";

                itemDef.pickupModelPrefab = createPickupPrefab ? CreatePickupPrefab() : Constants.NullPrefab;

                if (createItemDisplayPrefab)
                    CreateDisplayPrefab();

                Util.CreateAssetAtSelectionPath(itemDef);

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error while creating item: {e}");
                return false;
            }
        }

        private GameObject CreateDisplayPrefab()
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
        private GameObject CreatePickupPrefab()
        {
            //Create game objects
            var pickup = new GameObject("Pickup" + actualName);
            var mdl = Util.CreateGenericPrefab("mdl" + actualName, prefabMesh, prefabMaterial);

            //Parents prefabs
            Util.AddTransformToParent(mdl, pickup);

            //Destroy box collider
            var boxCollider = mdl.GetComponent<BoxCollider>();
            DestroyImmediate(boxCollider);

            return Util.CreatePrefabAtSelectionPath(pickup);
        }
    }
}
