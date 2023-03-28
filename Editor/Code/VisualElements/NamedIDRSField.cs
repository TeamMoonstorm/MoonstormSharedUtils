using System.Collections;
using System.Collections.Generic;
using ThunderKit.Markdown;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using RoR2EditorKit;
using ThunderKit.Core.UIElements;
using UnityEditor;
using RoR2;
using System;
using RoR2EditorKit.VisualElements;
using UObject = UnityEngine.Object;

[assembly: UxmlNamespacePrefix("Moonstorm.EditorUtils.VisualElements", "msu")]
namespace Moonstorm.EditorUtils.VisualElements
{
    public class NamedIDRSField : VisualElement, IBindable
    {
        public new class UxmlFactory : UxmlFactory<NamedIDRSField, UxmlTraits>
        {
        }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            static string NormalizeName(string text) => ObjectNames.NicifyVariableName(text).ToLower().Replace(" ", "-");

            private UxmlStringAttributeDescription m_idrsPropertyPath = new UxmlStringAttributeDescription
            {
                name = NormalizeName(nameof(bindingPath)),
            };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                NamedIDRSField field = (NamedIDRSField)ve;

                field.ObjectField.SetObjectType<ItemDisplayRuleSet>();
                field.bindingPath = m_idrsPropertyPath.GetValueFromBag(bag, cc);
            }
        }
        public ObjectField ObjectField { get; }
        public HelpBox HelpBox { get; }
        public IBinding binding { get => ObjectField.binding; set => ObjectField.binding = value; }
        public string bindingPath { get => ObjectField.bindingPath; set => ObjectField.bindingPath = value; }

        public event Action<ItemDisplayRuleSet> OnIDRSFieldValueSet;


        private void OnAttach(AttachToPanelEvent evt)
        {
            ObjectField.RegisterValueChangedCallback(OnIDRSSet);
            RegisterCallback<SelectionChangeEvent>(OnSelectionChanged, TrickleDown.TrickleDown);
        }

        private void OnSelectionChanged(SelectionChangeEvent evt)
        {

        }
        private void OnDetach(DetachFromPanelEvent evt)
        {
            ObjectField.UnregisterValueChangedCallback(OnIDRSSet);
        }

        private void OnIDRSSet(ChangeEvent<UnityEngine.Object> evt)
        {
            var idrs = (ItemDisplayRuleSet)evt.newValue;
            OnIDRSFieldValueSet?.Invoke(idrs);
        }

        public NamedIDRSField()
        {
            TemplateHelpers.GetTemplateInstance(nameof(NamedIDRSField), this, (pth) => true);

            ObjectField = this.Q<ObjectField>();
            HelpBox = this.Q<HelpBox>();
            RegisterCallback<AttachToPanelEvent>(OnAttach);
            RegisterCallback<DetachFromPanelEvent>(OnDetach);
        }

        ~NamedIDRSField()
        {
            UnregisterCallback<AttachToPanelEvent>(OnAttach);
            UnregisterCallback<DetachFromPanelEvent>(OnDetach);
        }
    }
}