using RoR2.Editor;
using UnityEditor;
using UnityEngine.UIElements;

namespace MSU.Editor.UIElements
{
    public class PropertySelectorButton : VisualElement
    {
        public Button button { get; }
        public ExtendedHelpBox helpBox { get; }
        public SerializedProperty representingProperty
        {
            get => _representingProperty;
            set
            {
                if (_representingProperty != value)
                {
                    _representingProperty = value;
                    updateRepresentation?.Invoke(this);
                }
            }
        }
        private SerializedProperty _representingProperty;
        public int index;
        public UpdateRepresentationDelegate updateRepresentation;
        public object extraData;

        public delegate void UpdateRepresentationDelegate(PropertySelectorButton instance);

        public PropertySelectorButton()
        {
            button = new Button();
            button.style.flexDirection = FlexDirection.Row;

            helpBox = new ExtendedHelpBox();
            helpBox.isDismissable = false;
            helpBox.messageIsExplicit = false;
            button.Add(helpBox);
            this.Add(button);
        }
    }
}