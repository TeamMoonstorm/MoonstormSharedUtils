using RoR2;
using RoR2EditorKit;
using RoR2EditorKit.VisualElements;
using System;
using System.Collections.Generic;
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
        public SerializedProperty SerializedProperty { get; private set; }
        public HelpBox HelpBox { get; }
        public Toggle UseDirectReference { get; }
        public ObjectField PrefabField { get; }
        public TextField AddressField { get; }
        public EnumField ItemDisplayRuleType { get; }
        public Button PasteButton { get; }
        public TextField ChildName { get; }
        public Vector3Field LocalPos { get; }
        public Vector3Field LocalRot { get; }
        public Vector3Field LocalScale { get; }
        public EnumFlagsField LimbMask { get; }

        private VisualElement standardViewContainer;
        private SerializedProperty ruleType;
        private SerializedProperty useDirectReference;
        private SerializedProperty asset;
        private SerializedProperty address;
        private SerializedProperty childName;
        private SerializedProperty localPos;
        private SerializedProperty localRot;
        private SerializedProperty localScale;
        private void UpdateBinding()
        {
            if(SerializedProperty == null)
            {
                ruleType = null;
                useDirectReference = null;
                asset = null;
                address = null;
                childName = null;
                localPos = null;
                localRot = null;
                localScale = null;
                HelpBox.SetDisplay(true);
                standardViewContainer.SetDisplay(false);
                UseDirectReference.Unbind();
                PrefabField.Unbind();
                AddressField.Unbind();
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
            var addressablePrefab = SerializedProperty.FindPropertyRelative("displayPrefab");
            useDirectReference = addressablePrefab.FindPropertyRelative("useDirectReference");
            asset = addressablePrefab.FindPropertyRelative("asset");
            address = addressablePrefab.FindPropertyRelative("address");
            childName = SerializedProperty.FindPropertyRelative("childName");
            localPos = SerializedProperty.FindPropertyRelative("localPos");
            localRot = SerializedProperty.FindPropertyRelative("localAngles");
            localScale = SerializedProperty.FindPropertyRelative("localScales");

            UseDirectReference.BindProperty(useDirectReference);
            PrefabField.BindProperty(asset);
            AddressField.BindProperty(address);
            ItemDisplayRuleType.BindProperty(ruleType);
            ChildName.BindProperty(childName);
            LocalPos.BindProperty(localPos);
            LocalRot.BindProperty(localRot);
            LocalScale.BindProperty(localScale);
            LimbMask.BindProperty(SerializedProperty.FindPropertyRelative("limbMask"));

            HelpBox.SetDisplay(false);
            standardViewContainer.SetDisplay(true);

            OnUseDirectReferenceChange(null, useDirectReference.boolValue);
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

        private void OnUseDirectReferenceChange(ChangeEvent<bool> changeEvent, bool? defaultValue)
        {
            if (changeEvent == null && !defaultValue.HasValue)
            {
                UpdateButton(changeEvent);
                return;
            }
            bool newValue = changeEvent?.newValue ?? defaultValue.Value;
            useDirectReference.boolValue = newValue;
            PrefabField.SetDisplay(newValue);
            AddressField.SetDisplay(!newValue);

            UpdateButton(changeEvent);
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

        private void OnPrefabSet(ChangeEvent<UnityEngine.Object> evt)
        {
            var value = evt.newValue;
            if(!value)
            {
                asset.objectReferenceValue = value;
                UpdateButton(evt);
                return;
            }

            if(value is GameObject gameObject)
            {
                if(!gameObject.GetComponent<ItemDisplay>())
                {
                    Debug.LogWarning("Supplied GameObject does not have an ItemDisplay component!");
                }
                asset.objectReferenceValue = value;
                UpdateButton(evt);
            }
        }

        private void OnAddressSet(ChangeEvent<string> evt)
        {
            var val = evt.newValue;
            if(AddressablesUtils.AddressableCatalogExists && !useDirectReference.boolValue)
            {
                Addressables.LoadAssetAsync<GameObject>(val);
            }

            address.stringValue = evt.newValue;
            UpdateButton(evt);
        }

        private void UpdateButton(EventBase evt)
        {
            if(CurrentEntry != null && CurrentEntry.parent != null)
            {
                CurrentEntry.UpdateRepresentation?.Invoke(CurrentEntry);
            }
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

        private void OnAttach(AttachToPanelEvent evt)
        {
            HelpBox.SetDisplay(SerializedProperty == null);
            standardViewContainer.SetDisplay(SerializedProperty != null);

            PrefabField.SetObjectType<GameObject>();
            PrefabField.allowSceneObjects = false;
            PrefabField.RegisterValueChangedCallback(OnPrefabSet);
            UseDirectReference.RegisterValueChangedCallback((x) => OnUseDirectReferenceChange(x, null));
            AddressField.RegisterValueChangedCallback(OnAddressSet);
            ItemDisplayRuleType.RegisterValueChangedCallback((x) => OnRuleTypeChange(x, null));
            AddressField.isDelayed = true;
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
            UseDirectReference = this.Q<Toggle>();
            PrefabField = this.Q<ObjectField>();
            AddressField = this.Q<TextField>("Address");
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
