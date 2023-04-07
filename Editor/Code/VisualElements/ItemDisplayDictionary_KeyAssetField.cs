using RoR2;
using RoR2EditorKit;
using RoR2EditorKit.VisualElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderKit.Core.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Moonstorm.EditorUtils.VisualElements
{
    public class ItemDisplayDictionary_KeyAssetField : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<ItemDisplayDictionary_KeyAssetField, UxmlTraits> { }
        public new class UxmlTraits : VisualElement.UxmlTraits { }

        public HelpBox HelpBox { get; }
        public ObjectField KeyAsset { get; }
        public ExtendedListView DisplayPrefabs { get; }
        public event Action<ScriptableObject> OnKeyAssetValueSet;

        private SerializedProperty keyAssetProperty;
        private SerializedProperty displayPrefabsProperty;
        private VisualElement _container;
        public void CheckForIDD(SerializedObject serializedObject)
        {
            if(!(serializedObject?.targetObject is ItemDisplayDictionary))
            {
                keyAssetProperty = null;
                displayPrefabsProperty = null;
                KeyAsset.Unbind();
                DisplayPrefabs.Unbind();
                _container.SetDisplay(false);
                HelpBox.SetDisplay(true);
                HelpBox.messageType = MessageType.Warning;
                HelpBox.message = "No ItemDisplayDictionary Selected, Please Select an ItemDisplayDictionary";
                return;
            }

            keyAssetProperty = serializedObject.FindProperty("keyAsset");
            displayPrefabsProperty = serializedObject.FindProperty("displayPrefabs");
            KeyAsset.BindProperty(keyAssetProperty);
            DisplayPrefabs.collectionProperty = displayPrefabsProperty;
            _container.SetDisplay(true);
            if(!KeyAsset.value)
            {
                HelpBox.SetDisplay(true);
                HelpBox.message = "No KeyAsset Set, Cannot show data.";
                HelpBox.messageType = MessageType.Info;
            }
            else
            {
                HelpBox.SetDisplay(false);
            }
        }

        public void OnKeyAssetSet(ChangeEvent<UnityEngine.Object> evt)
        {
            ScriptableObject so = (ScriptableObject)evt.newValue;
            if(!so)
            {
                HelpBox.SetDisplay(false);
                HelpBox.message = "No KeyAsset Set, Cannot show data.";
                HelpBox.messageType = MessageType.Info;
            }
            else if(!(so is ItemDef ^ so is EquipmentDef))
            {
                Debug.LogWarning("KeyAsset MUST be either an ItemDef or EquipmentDef!");
                KeyAsset.SetValueWithoutNotify(evt.previousValue);
                return;
            }
            HelpBox.SetDisplay(!so);
            HelpBox.message = so ? string.Empty : "No KeyAsset Set, Cannot show data.";
            HelpBox.messageType = so ? MessageType.None : MessageType.Info;
            OnKeyAssetValueSet?.Invoke(so);
        }

        private VisualElement CreateObjectField() => new ObjectField();
        private void BindObjectField(VisualElement ve, SerializedProperty property)
        {
            ObjectField objField = (ObjectField)ve;
            objField.allowSceneObjects = false;
            objField.SetObjectType<GameObject>();
            objField.style.height = DisplayPrefabs.listViewItemHeight;
            objField.label = property.displayName;
            objField.BindProperty(property);
        }
        public void OnAttach(AttachToPanelEvent evt)
        {
            KeyAsset.SetObjectType<ScriptableObject>();
            KeyAsset.RegisterValueChangedCallback(OnKeyAssetSet);

            DisplayPrefabs.CreateElement = CreateObjectField;
            DisplayPrefabs.BindElement = BindObjectField;
        }
        public void OnDetach(DetachFromPanelEvent evt)
        {
            KeyAsset.UnregisterValueChangedCallback(OnKeyAssetSet);
        }
        public ItemDisplayDictionary_KeyAssetField()
        {
            TemplateHelpers.GetTemplateInstance(GetType().Name, this, (_) => true);
            
            _container = this.Q<VisualElement>("ContentContainer");
            HelpBox = this.Q<HelpBox>();
            KeyAsset = this.Q<ObjectField>();
            DisplayPrefabs = this.Q<ExtendedListView>();

            RegisterCallback<AttachToPanelEvent>(OnAttach);
            RegisterCallback<DetachFromPanelEvent>(OnDetach);
        }

        ~ItemDisplayDictionary_KeyAssetField()
        {
            UnregisterCallback<AttachToPanelEvent>(OnAttach);
            UnregisterCallback<DetachFromPanelEvent>(OnDetach);
        }
    }
}
