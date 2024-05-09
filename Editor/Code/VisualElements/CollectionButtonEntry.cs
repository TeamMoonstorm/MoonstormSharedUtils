using RoR2EditorKit.VisualElements;
using System;
using ThunderKit.Core.UIElements;
using UnityEditor;
using UnityEngine.UIElements;

namespace MSU.Editor.VisualElements
{
    public class CollectionButtonEntry : VisualElement
    {
        public Button Button { get; }
        public HelpBox HelpBox { get; }
        public SerializedProperty SerializedProperty
        {
            get
            {
                return _serializedProperty;
            }
            set
            {
                if (_serializedProperty != value)
                {
                    _serializedProperty = value;
                    UpdateRepresentation?.Invoke(this);
                }
            }
        }
        private SerializedProperty _serializedProperty;
        public object extraData { get; set; }
        public Action<CollectionButtonEntry> UpdateRepresentation;

        public CollectionButtonEntry()
        {
            TemplateHelpers.GetTemplateInstance(nameof(CollectionButtonEntry), this, (_) => true);
            Button = this.Q<Button>();
            HelpBox = Button.Q<HelpBox>();
        }
    }
}