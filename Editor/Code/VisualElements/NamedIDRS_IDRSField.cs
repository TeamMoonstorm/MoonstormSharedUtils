using RoR2;
using RoR2EditorKit;
using RoR2EditorKit.VisualElements;
using System;
using ThunderKit.Core.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

[assembly: UxmlNamespacePrefix("Moonstorm.EditorUtils.VisualElements", "msu")]
namespace Moonstorm.EditorUtils.VisualElements
{
    public class NamedIDRS_IDRSField : VisualElement, IBindable
    {
        public new class UxmlFactory : UxmlFactory<NamedIDRS_IDRSField, UxmlTraits> { }

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
                NamedIDRS_IDRSField field = (NamedIDRS_IDRSField)ve;
                field.bindingPath = m_idrsPropertyPath.GetValueFromBag(bag, cc);
            }
        }
        public ObjectField ObjectField { get; }
        public HelpBox HelpBox { get; }
        public IBinding binding { get => ObjectField.binding; set => ObjectField.binding = value; }
        public string bindingPath { get => ObjectField.bindingPath; set => ObjectField.bindingPath = value; }

        public event Action<ItemDisplayRuleSet> OnIDRSFieldValueSet;


        public void CheckForNamedIDRS(SerializedObject serializedObject)
        {
            if (!(serializedObject?.targetObject is NamedIDRS))
            {
                ObjectField.SetDisplay(false);
                HelpBox.SetDisplay(true);
                HelpBox.messageType = MessageType.Warning;
                HelpBox.message = "No NamedIDRS Selected, Please Select a NamedIDRS";
                return;
            }

            ObjectField.SetDisplay(true);
            if (!ObjectField.value)
            {
                HelpBox.SetDisplay(true);
                HelpBox.message = "No IDRS Set, Cannot show data.";
                HelpBox.messageType = MessageType.Info;
            }
            else
            {
                HelpBox.SetDisplay(false);
            }
        }

        private void OnIDRSSet(ChangeEvent<UnityEngine.Object> evt)
        {
            var idrs = (ItemDisplayRuleSet)evt.newValue;
            HelpBox.SetDisplay(!idrs);
            HelpBox.message = idrs ? string.Empty : "No IDRS Set, Cannot show data.";
            HelpBox.messageType = idrs ? MessageType.None : MessageType.Info;
            OnIDRSFieldValueSet?.Invoke(idrs);
        }

        private void OnAttach(AttachToPanelEvent evt)
        {
            ObjectField.SetObjectType<ItemDisplayRuleSet>();
            ObjectField.RegisterValueChangedCallback(OnIDRSSet);
        }
        private void OnDetach(DetachFromPanelEvent evt)
        {
            ObjectField.UnregisterValueChangedCallback(OnIDRSSet);
        }

        public NamedIDRS_IDRSField()
        {
            TemplateHelpers.GetTemplateInstance(nameof(NamedIDRS_IDRSField), this, (pth) => pth.ValidateUXMLPath());

            ObjectField = this.Q<ObjectField>();
            HelpBox = this.Q<HelpBox>();

            RegisterCallback<AttachToPanelEvent>(OnAttach);
            RegisterCallback<DetachFromPanelEvent>(OnDetach);
        }

        ~NamedIDRS_IDRSField()
        {
            UnregisterCallback<AttachToPanelEvent>(OnAttach);
            UnregisterCallback<DetachFromPanelEvent>(OnDetach);
        }
    }
}