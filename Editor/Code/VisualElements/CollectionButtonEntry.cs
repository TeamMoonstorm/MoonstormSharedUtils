using Moonstorm.AddressableAssets;
using RoR2EditorKit;
using RoR2EditorKit.VisualElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderKit.Core.UIElements;
using UnityEditor;
using UnityEngine.UIElements;

namespace Moonstorm.EditorUtils.VisualElements
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
                if(_serializedProperty != value)
                {
                    _serializedProperty = value;
                    UpdateRepresentation?.Invoke(this);
                }
            }
        }
        private SerializedProperty _serializedProperty;
        public Action<CollectionButtonEntry> UpdateRepresentation;

        public CollectionButtonEntry()
        {
            TemplateHelpers.GetTemplateInstance(nameof(CollectionButtonEntry), this, (_) => true);
            Button = this.Q<Button>();
            HelpBox = Button.Q<HelpBox>();
        }
    }
}