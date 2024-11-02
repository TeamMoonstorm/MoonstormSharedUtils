using RoR2;
using RoR2.Editor;
using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace MSU.Editor.UIElements
{
    public class ItemDisplayDictionary_KeyAssetDisplayPrefabsElement : VisualElement, ISerializedObjectBoundCallback
    {
        public ExtendedHelpBox noKeyAssetHelpBox { get; }
        public ObjectField keyAssetObjectField { get; }
        public event Action<ScriptableObject> onKeyAssetChanged;

        public ExtendedHelpBox noDisplayPrefabsHelpBox { get; }
        public ListView displayPrefabsList { get; }
        public event Action<SerializedProperty> onDisplayPrefabsChanged;
        
        public void OnAttach(AttachToPanelEvent evt)
        {
            keyAssetObjectField.RegisterValueChangedCallback(OnKeyAssetChanged);
        }

        public void OnDetach(DetachFromPanelEvent evt)
        {
            keyAssetObjectField.UnregisterValueChangedCallback(OnKeyAssetChanged);
        }

        private void OnKeyAssetChanged(ChangeEvent<UnityEngine.Object> evt)
        {
            DetermineKeyAssetHelpBoxDisplay();

            onKeyAssetChanged?.Invoke(evt.newValue as ScriptableObject);
        }

        private void DetermineKeyAssetHelpBoxDisplay()
        {
            var value = keyAssetObjectField.value;
            noKeyAssetHelpBox.SetDisplay(!value);
        }

        private void DetermineDisplayPrefabHelpBoxDisplay(SerializedProperty p)
        {
            if(p.arraySize == 0)
            {
                noDisplayPrefabsHelpBox.SetDisplay(true);
            }
            else
            {
                noDisplayPrefabsHelpBox.SetDisplay(false);
            }

            onDisplayPrefabsChanged?.Invoke(p);
        }


        public void OnBoundSerializedObjectChange(SerializedObject so)
        {
            if (so?.targetObject is not ItemDisplayDictionary)
            {
                this.SetDisplay(false);
                return;
            }

            this.SetDisplay(true);
            DetermineKeyAssetHelpBoxDisplay();
            displayPrefabsList.TrackPropertyValue(so.FindProperty(displayPrefabsList.bindingPath), DetermineDisplayPrefabHelpBoxDisplay);
        }

        public ItemDisplayDictionary_KeyAssetDisplayPrefabsElement()
        {
            VisualElementTemplateDictionary.instance.GetTemplateInstance(GetType().Name, this, p => p.ValidateUXMLPath());

            noKeyAssetHelpBox = this.Q<ExtendedHelpBox>("NoKeyAsset");
            noDisplayPrefabsHelpBox = this.Q<ExtendedHelpBox>("NoDisplayPrefabs");

            keyAssetObjectField = this.Q<ObjectField>("KeyAssetSelector");
            displayPrefabsList = this.Q<ListView>("DisplayPrefabs");

            RegisterCallback<AttachToPanelEvent>(OnAttach);
            RegisterCallback<DetachFromPanelEvent>(OnDetach);
        }

        new public class UxmlFactory : UxmlFactory<ItemDisplayDictionary_KeyAssetDisplayPrefabsElement, UxmlTraits> { }
        new public class UxmlTraits : VisualElement.UxmlTraits
        {
        }
    }
}