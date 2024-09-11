using RoR2;
using RoR2.Editor;
using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace MSU.Editor.UIElements
{
    public interface ISerializedObjectBoundCallback
    {
        public void OnBoundSerializedObjectChange(SerializedObject so);
    }
    public class NamedItemDisplayRuleSet_TargetRuleSet : VisualElement, IBindable, ISerializedObjectBoundCallback
    {
        public IBinding binding { get => targetIDRSObjectField.binding; set => targetIDRSObjectField.binding = value; }
        public string bindingPath { get => targetIDRSObjectField.bindingPath; set => targetIDRSObjectField.bindingPath = value; }

        public ExtendedHelpBox noTargetHelpBox { get; }
        public ObjectField targetIDRSObjectField { get; }

        public event Action<ItemDisplayRuleSet> onTargetIDRSChanged;

        public void OnAttach(AttachToPanelEvent evt)
        {
            targetIDRSObjectField.RegisterValueChangedCallback(OnTargetIDRSChanged);
        }

        public void OnDetach(DetachFromPanelEvent evt)
        {
            targetIDRSObjectField.UnregisterValueChangedCallback(OnTargetIDRSChanged);
        }

        private void OnTargetIDRSChanged(ChangeEvent<UnityEngine.Object> evt)
        {
            var idrs = (ItemDisplayRuleSet)evt.newValue;

            DetermineHelpBoxDisplay();

            onTargetIDRSChanged?.Invoke((ItemDisplayRuleSet)evt.newValue);
        }

        public void OnBoundSerializedObjectChange(SerializedObject so)
        {
            if(so?.targetObject is not NamedItemDisplayRuleSet)
            {
                this.SetDisplay(false);
                return;
            }

            this.SetDisplay(true);
            DetermineHelpBoxDisplay();
        }

        private void DetermineHelpBoxDisplay()
        {

            var value = targetIDRSObjectField.value;
            noTargetHelpBox.SetDisplay(!value);
        }

        public NamedItemDisplayRuleSet_TargetRuleSet()
        {
            VisualElementTemplateDictionary.instance.GetTemplateInstance(GetType().Name, this, p => p.ValidateUXMLPath());

            targetIDRSObjectField = this.Q<ObjectField>("IDRSSelector");
            noTargetHelpBox = this.Q<ExtendedHelpBox>();

            RegisterCallback<AttachToPanelEvent>(OnAttach);
            RegisterCallback<DetachFromPanelEvent>(OnDetach);
        }
        new public class UxmlFactory : UxmlFactory<NamedItemDisplayRuleSet_TargetRuleSet, UxmlTraits> { }
        new public class UxmlTraits : VisualElement.UxmlTraits
        {
            private UxmlStringAttributeDescription m_idrsPropertyPath = new UxmlStringAttributeDescription
            {
                name = VisualElementUtil.NormalizeNameForUXMLTrait(nameof(bindingPath))
            };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                NamedItemDisplayRuleSet_TargetRuleSet element = (NamedItemDisplayRuleSet_TargetRuleSet)ve;
                element.bindingPath = m_idrsPropertyPath.GetValueFromBag(bag, cc);
            }
        }
    }
}