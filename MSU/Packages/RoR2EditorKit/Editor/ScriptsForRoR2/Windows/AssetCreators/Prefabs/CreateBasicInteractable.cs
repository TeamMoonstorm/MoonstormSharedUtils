using EntityStates;
using RoR2;
using RoR2EditorKit.Common;
using RoR2EditorKit.Core;
using RoR2EditorKit.Core.Windows;
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2EditorKit.RoR2Related.EditorWindows
{
    public class CreateBasicInteractable : CreateRoR2PrefabWindow<EntityStateMachine>
    {

        private bool hasCost;
        private CostTypeIndex costType;
        private int cost;
        private bool isShrine;

        private bool isChest;
        private bool summonsSomethingOnPurchased;
        private bool isPortal;

        private bool createMatchingInteractableSpawnCard;


        [MenuItem(Constants.RoR2EditorKitContextRoot + "Prefabs/Interactable", false, Constants.RoR2EditorKitContextPriority)]
        public static void Open()
        {
            OpenEditorWindow<CreateBasicInteractable>(null, "Create Basic Interactable");
        }

        protected override void OnWindowOpened()
        {
            base.OnWindowOpened();

            hasCost = false;
            costType = CostTypeIndex.None;
            cost = 0;
            isShrine = false;
            isChest = false;
            summonsSomethingOnPurchased = false;
            isPortal = false;
            createMatchingInteractableSpawnCard = false;

            //Destroying uneeded components from the main prefab
            DestroyImmediate(mainPrefab.GetComponent<MeshFilter>());
            DestroyImmediate(mainPrefab.GetComponent<MeshRenderer>());
            DestroyImmediate(mainPrefab.GetComponent<CapsuleCollider>());

            //Adding networking
            mainPrefab.AddComponent<NetworkIdentity>();
            mainPrefab.AddComponent<NetworkTransform>();
            var networkStateMachine = mainPrefab.AddComponent<NetworkStateMachine>();
            var type = networkStateMachine.GetType();
            var field = type.GetField("stateMachines", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
                field.SetValue(networkStateMachine, new EntityStateMachine[] { MainComponent });

            //Adding mdl base and the mdl children
            var baseAndChild = AddMdlChild();
            var mdlLocator = mainPrefab.AddComponent<ModelLocator>();
            mdlLocator.modelBaseTransform = baseAndChild.Item1.transform;
            mdlLocator.modelTransform = baseAndChild.Item2.transform;

            //adding highlight component
            var highlight = mainPrefab.AddComponent<Highlight>();
            highlight.targetRenderer = baseAndChild.Item2.GetComponent<MeshRenderer>();
            highlight.strength = 1;
            highlight.highlightColor = Highlight.HighlightColor.interactive;

            //Display Provider
            var displayProvider = mainPrefab.AddComponent<GenericDisplayNameProvider>();
            displayProvider.displayToken = "MYINTERACTABLE_INTERACTABLE_NAME";

            //Sound Locator
            mainPrefab.AddComponent<SfxLocator>();

            mainSerializedObject = new SerializedObject(mainPrefab);
        }

        private (GameObject, GameObject) AddMdlChild()
        {
            var modelBase = new GameObject("ModelBase");
            modelBase.transform.SetParent(mainPrefab.transform);

            var primitive = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            primitive.transform.SetParent(modelBase.transform);

            var eLocator = primitive.AddComponent<EntityLocator>();
            eLocator.entity = mainPrefab;

            primitive.AddComponent<ChildLocator>();

            return (modelBase, primitive);
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical("box");

            nameField = EditorGUILayout.TextField(new GUIContent("Interactable Name", "The name of this interactable, will be used on token creation."), nameField);
            hasCost = EditorGUILayout.Toggle(new GUIContent("Has Cost", "Wether or not this interactable costs something, enabling this enables extra settings."), hasCost);
            if (hasCost)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.BeginVertical("box");
                HandleCost();
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
            }
            isChest = EditorGUILayout.Toggle(new GUIContent("Is Chest", "Wether or not this interactable is a chest."), isChest);
            summonsSomethingOnPurchased = EditorGUILayout.Toggle(new GUIContent("Summons something on Interaction", "Wether or not this interactable summons something on interaction, the summoned object needs to be a Master prefab."), summonsSomethingOnPurchased);
            isPortal = EditorGUILayout.Toggle(new GUIContent("Is Portal", "Wether or not this interactable is a portal that'll send the player to a new stage."), isPortal);

            createMatchingInteractableSpawnCard = EditorGUILayout.Toggle(new GUIContent("Create Matching ISC", "Wether or not a matching interactable spawn card will be created for this interactable."), createMatchingInteractableSpawnCard);

            if (SimpleButton("Create Interactable"))
            {
                var result = CreateInteractable();
                if (result)
                {
                    Debug.Log($"Succesfully Created Interactable {nameField}");
                    TryToClose();
                }
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }

        private void HandleCost()
        {
            costType = (CostTypeIndex)EditorGUILayout.EnumPopup(new GUIContent("Cost Type", "The type of cost this interactable has"), costType);
            cost = EditorGUILayout.IntField(new GUIContent("Cost", "How much does this interactable Cost to Interact."), cost);
            isShrine = EditorGUILayout.Toggle(new GUIContent("Is Shrine", "Wether or not this interactable qualifies as a Shrine"), isShrine);
        }

        private bool CreateInteractable()
        {
            actualName = GetCorrectAssetName(nameField);
            try
            {
                if (string.IsNullOrEmpty(actualName))
                    throw ErrorShorthands.ThrowNullAssetName(nameof(nameField));

                mainPrefab.name = actualName;

                if (string.IsNullOrEmpty(Settings.TokenPrefix))
                    throw ErrorShorthands.ThrowNullTokenPrefix();

                MainComponent.customName = actualName;
                MainComponent.initialStateType = default(SerializableEntityStateType);
                MainComponent.mainStateType = default(SerializableEntityStateType);

                mainPrefab.GetComponent<GenericDisplayNameProvider>().displayToken = isShrine ? CreateDisplayToken(true) : CreateDisplayToken(false);

                if (hasCost) AddCostComponent();
                if (isChest) mainPrefab.AddComponent<ChestBehavior>();
                if (summonsSomethingOnPurchased) mainPrefab.AddComponent<SummonMasterBehavior>();
                if (isPortal) AddSceneExitController();

                var prefab = Util.CreatePrefabAtSelectionPath(mainPrefab);
                if (createMatchingInteractableSpawnCard)
                    CreateISC(prefab);

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error while creating Interactable: {e}");
                return false;
            }
        }

        private string CreateDisplayToken(bool shrine)
        {
            if (shrine)
                return $"{Settings.TokenPrefix}_SHRINE_{actualName.ToUpperInvariant()}_NAME";
            else
                return $"{Settings.TokenPrefix}_INTERACTABLE_{actualName.ToUpperInvariant()}_NAME";
        }

        private void AddCostComponent()
        {
            var pInteraction = mainPrefab.AddComponent<PurchaseInteraction>();
            pInteraction.displayNameToken = $"{Settings.TokenPrefix}_{actualName.ToUpperInvariant()}_NAME";
            pInteraction.contextToken = $"{Settings.TokenPrefix}_{actualName.ToUpperInvariant()}_NAME";
            pInteraction.costType = costType;
            pInteraction.cost = cost;
            pInteraction.isShrine = isShrine;
        }

        private void AddSceneExitController()
        {
            var genericInteraction = mainPrefab.AddComponent<GenericInteraction>();
            genericInteraction.contextToken = $"{Settings.TokenPrefix}_PORTAL_{actualName.ToUpperInvariant()}_NAME";
            mainPrefab.AddComponent<SceneExitController>();
        }

        private void CreateISC(GameObject cardPrefab)
        {
            var isc = ScriptableObject.CreateInstance<InteractableSpawnCard>();
            isc.name = $"isc{actualName}";
            isc.prefab = cardPrefab;

            Util.CreateAssetAtSelectionPath(isc);
        }
    }
}