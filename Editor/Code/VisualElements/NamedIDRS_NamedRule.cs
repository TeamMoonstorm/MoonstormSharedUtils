using RoR2;
using RoR2EditorKit;
using RoR2EditorKit.VisualElements;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderKit.Core.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UIElements;

namespace Moonstorm.EditorUtils.VisualElements
{
    public class NamedIDRS_NamedRule : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<NamedIDRS_NamedRule, UxmlTraits> { }
        public new class UxmlTraits : VisualElement.UxmlTraits { }
        public ItemDisplayCatalog Catalog { get; internal set; }
        public CollectionButtonEntry CurrentEntry
        {
            get
            {
                return _currentEntry;
            }
            set
            {
                if(_currentEntry != value)
                {
                    _currentEntry = value;
                    SerializedProperty?.serializedObject.ApplyModifiedProperties();
                    SerializedProperty = value?.SerializedProperty;
                    UpdateBinding();
                }
            }
        }
        private CollectionButtonEntry _currentEntry;
        public ReadOnlyCollection<string> AvailableDisplayPrefabs
        {
            get
            {
                return CurrentEntry?.extraData as ReadOnlyCollection<string>;
            }
            set
            {
                if(CurrentEntry != null)
                {
                    CurrentEntry.extraData = value;
                }
            }
        }
        public SerializedProperty SerializedProperty { get; private set; }
        public HelpBox HelpBox { get; }
        public IMGUIContainer DisplayPrefab { get; }
        public EnumField ItemDisplayRuleType { get; }
        public Button PasteButton { get; }
        public TextField ChildName { get; }
        public Vector3Field LocalPos { get; }
        public Vector3Field LocalRot { get; }
        public Vector3Field LocalScale { get; }
        public EnumFlagsField LimbMask { get; }

        private VisualElement standardViewContainer;
        private SerializedProperty ruleType;
        private SerializedProperty displayPrefab;
        private SerializedProperty childName;
        private SerializedProperty localPos;
        private SerializedProperty localRot;
        private SerializedProperty localScale;
        private void UpdateBinding()
        {
            if(SerializedProperty == null)
            {
                ruleType = null;
                displayPrefab = null;
                childName = null;
                localPos = null;
                localRot = null;
                localScale = null;
                HelpBox.SetDisplay(true);
                standardViewContainer.SetDisplay(false);
                DisplayPrefab.onGUIHandler = null;
                ItemDisplayRuleType.Unbind();
                PasteButton.Unbind();
                ChildName.Unbind();
                LocalPos.Unbind();
                LocalRot.Unbind();
                LocalScale.Unbind();
                LimbMask.Unbind();
                return;
            }

            ruleType = SerializedProperty.FindPropertyRelative("ruleType");
            displayPrefab = SerializedProperty.FindPropertyRelative("displayPrefabName");
            childName = SerializedProperty.FindPropertyRelative("childName");
            localPos = SerializedProperty.FindPropertyRelative("localPos");
            localRot = SerializedProperty.FindPropertyRelative("localAngles");
            localScale = SerializedProperty.FindPropertyRelative("localScales");

            ItemDisplayRuleType.BindProperty(ruleType);
            DisplayPrefab.onGUIHandler = DrawDropDown;
            ChildName.BindProperty(childName);
            LocalPos.BindProperty(localPos);
            LocalRot.BindProperty(localRot);
            LocalScale.BindProperty(localScale);
            LimbMask.BindProperty(SerializedProperty.FindPropertyRelative("limbMask"));

            HelpBox.SetDisplay(false);
            standardViewContainer.SetDisplay(true);

            OnRuleTypeChange(null, (ItemDisplayRuleType?)ruleType.enumValueIndex);
        }

        public void OnIDRSFieldValueSet(ItemDisplayRuleSet obj)
        {
            this.SetDisplay(obj);
        }

        public void CheckForNamedIDRS(SerializedObject serializedObject)
        {
            if (serializedObject == null)
            {
                this.SetDisplay(false);
                SerializedProperty = null;
            }
            else
            {
                HelpBox.SetDisplay(true);
                standardViewContainer.SetDisplay(false);
            }
        }

        private void OnRuleTypeChange(ChangeEvent<Enum> evt, ItemDisplayRuleType? defaultValue)
        {
            if((evt == null || evt.newValue == null) && !defaultValue.HasValue)
            {
                return;
            }

            ItemDisplayRuleType ruleType = (ItemDisplayRuleType)(evt?.newValue ?? defaultValue.Value);

            LimbMask.SetDisplay(ruleType == RoR2.ItemDisplayRuleType.LimbMask);
        }


        private void PasteValues()
        {
            if (SerializedProperty == null)
                return;

            string clipboardContent = GUIUtility.systemCopyBuffer;
            try
            {
                var split = clipboardContent.Split(',').ToArray();
                childName.stringValue = split[0];
                localPos.vector3Value = CreateVector3FromArray(new string[3] { split[1], split[2], split[3] });
                localRot.vector3Value = CreateVector3FromArray(new string[3] { split[4], split[5], split[6] });
                localScale.vector3Value = CreateVector3FromArray(new string[3] { split[7], split[8], split[9] });
                SerializedProperty.serializedObject.ApplyModifiedProperties();
            }
            catch(Exception ex)
            {
                Debug.LogError($"Failed to paste clipboard contents ({clipboardContent}) to {CurrentEntry.Button.text}'s values!\n{ex}");
            }

            Vector3 CreateVector3FromArray(string[] args)
            {
                return new Vector3(float.Parse(args[0], CultureInfo.InvariantCulture), float.Parse(args[1], CultureInfo.InvariantCulture), float.Parse(args[2], CultureInfo.InvariantCulture));
            }
        }

        private void DrawDropDown()
        {
            if (AvailableDisplayPrefabs == null)
                return;

            int currentIndex = AvailableDisplayPrefabs.IndexOf(displayPrefab.stringValue);
            if (currentIndex == -1 && displayPrefab.stringValue.IsNullOrEmptyOrWhitespace())
            {
                currentIndex = 0;
                displayPrefab.stringValue = AvailableDisplayPrefabs[currentIndex];
                displayPrefab.serializedObject.ApplyModifiedProperties();
                return;
            }
            else if(!AvailableDisplayPrefabs.Contains(displayPrefab.stringValue))
            {
                EditorGUILayout.LabelField(new GUIContent($"Display Prefab of name \"{displayPrefab.stringValue}\" could not be found.", $"The available Display Prefabs are:\n{string.Join("\n", AvailableDisplayPrefabs)}"), EditorStyles.boldLabel);
                return;
            }
            int newIndex = EditorGUILayout.Popup("Display Prefab", currentIndex, AvailableDisplayPrefabs.ToArray());
            string newDisplayPrefab = AvailableDisplayPrefabs[newIndex];
            displayPrefab.stringValue = newDisplayPrefab;
            if (displayPrefab.serializedObject.ApplyModifiedProperties())
                CurrentEntry?.UpdateRepresentation?.Invoke(CurrentEntry);
        }

        private void OnAttach(AttachToPanelEvent evt)
        {
            HelpBox.SetDisplay(SerializedProperty == null);
            standardViewContainer.SetDisplay(SerializedProperty != null);

            DisplayPrefab.onGUIHandler = DrawDropDown;
            ItemDisplayRuleType.RegisterValueChangedCallback((x) => OnRuleTypeChange(x, null));
            ChildName.isDelayed = true;
            PasteButton.clickable.clicked += PasteValues;
        }

        private void OnDetach(DetachFromPanelEvent evt)
        {

        }
        public NamedIDRS_NamedRule()
        {
            TemplateHelpers.GetTemplateInstance(nameof(NamedIDRS_NamedRule), this, (_) => true);

            HelpBox = this.Q<HelpBox>();
            standardViewContainer = this.Q<VisualElement>("StandardView");
            DisplayPrefab = this.Q<IMGUIContainer>();
            PasteButton = this.Q<Button>();
            ChildName = this.Q<TextField>("ChildName");
            LocalPos = this.Q<Vector3Field>("LocalPos");
            LocalRot = this.Q<Vector3Field>("LocalRot");
            LocalScale = this.Q<Vector3Field>("LocalScale");

            ItemDisplayRuleType = new EnumField("Item Display Rule Type", RoR2.ItemDisplayRuleType.ParentedPrefab);
            standardViewContainer.Insert(0, ItemDisplayRuleType);

            LimbMask = new EnumFlagsField("Limb Mask", RoR2.LimbFlags.None);
            standardViewContainer.Add(LimbMask);

            RegisterCallback<DetachFromPanelEvent>(OnDetach);
            RegisterCallback<AttachToPanelEvent>(OnAttach);
        }
        ~NamedIDRS_NamedRule()
        {
            UnregisterCallback<DetachFromPanelEvent>(OnDetach);
            UnregisterCallback<AttachToPanelEvent>(OnAttach);
        }
    }
}
