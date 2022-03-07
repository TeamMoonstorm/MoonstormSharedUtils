using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine.UIElements;

namespace RoR2EditorKit.Core.PropertyDrawers
{
    public abstract class VisualElementPropertyDrawer : PropertyDrawer
    {
        protected VisualElement RootVisualElement
        {
            get
            {
                if (_visualElement == null)
                    _visualElement = new VisualElement();
                return _visualElement;
            }
        }

        private VisualElement _visualElement;

        protected SerializedProperty serializedProperty;

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            _ = RootVisualElement;
            serializedProperty = property;
            DrawPropertyGUI();
            return RootVisualElement;
        }

        protected abstract VisualElement DrawPropertyGUI();
    }
}
