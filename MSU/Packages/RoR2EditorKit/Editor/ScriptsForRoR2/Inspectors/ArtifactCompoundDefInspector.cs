using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor;
using RoR2EditorKit.Core.Inspectors;
using RoR2;
using System;
using RoR2EditorKit.Utilities;

namespace RoR2EditorKit.RoR2Related.Inspectors
{
    [CustomEditor(typeof(ArtifactCompoundDef))]
    public sealed class ArtifactCompoundDefInspector : ScriptableObjectInspector<ArtifactCompoundDef>
    {
        protected override string Prefix => "acd";
        protected override bool PrefixUsesTokenPrefix => false;
        protected override bool HasVisualTreeAsset => true;

        private VisualElement inspectorDataHolder;

        private IMGUIContainer valueMessageContainer;

        private Button objectNameSetter;

        protected override void OnEnable()
        {
            base.OnEnable();
            OnVisualTreeCopy += () =>
            {
                var container = DrawInspectorElement.Q<VisualElement>("Container");
                inspectorDataHolder = container.Q<VisualElement>("InspectorDataHolder");
            };
        }

        protected override void DrawInspectorGUI()
        {
            var value = inspectorDataHolder.Q<PropertyField>("value");
            value.RegisterCallback<ChangeEvent<int>>(OnValueSet);
            OnValueSet();
            value.AddManipulator(new ContextualMenuManipulator(BuildValueMenu));
        }

        private void BuildValueMenu(ContextualMenuPopulateEvent obj)
        {
            obj.menu.AppendAction("Use RNG for Value", (dma) =>
            {
                var valueProp = serializedObject.FindProperty("value");
                valueProp.intValue = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
                serializedObject.ApplyModifiedProperties();
            });
        }

        private void OnValueSet(ChangeEvent<int> evt = null)
        {
            int value = evt == null ? TargetType.value : evt.newValue;

            if(valueMessageContainer != null)
            {
                valueMessageContainer.RemoveFromHierarchy();
            }

            switch(value)
            {
                case 1:
                    valueMessageContainer = CompoundHelpBox(value, "Circle");
                    break;
                case 5:
                    valueMessageContainer = CompoundHelpBox(value, "Diamond");
                    break;
                case 11:
                    valueMessageContainer = CompoundHelpBox(value, "Empty");
                    break;
                case 7:
                    valueMessageContainer = CompoundHelpBox(value, "Square");
                    break;
                case 3:
                    valueMessageContainer = CompoundHelpBox(value, "Triangle");
                    break;
                default:
                    valueMessageContainer = null;
                    break;
            }

            if(valueMessageContainer != null)
            {
                DrawInspectorElement.Add(valueMessageContainer);
            }

            IMGUIContainer CompoundHelpBox(int v, string name) => CreateHelpBox($"Compound value cannot be {v}, as that value is reserved for the {name} compound", MessageType.Error);
        }

        protected override IMGUIContainer EnsureNamingConventions(ChangeEvent<string> evt = null)
        {
            IMGUIContainer container = base.EnsureNamingConventions(evt);
            if(container != null)
            {
                container.style.alignItems = Align.FlexEnd;
                container.style.paddingBottom = 10;
                container.name += "_NamingConvention";
                if (InspectorEnabled)
                {
                    objectNameSetter = new Button(SetObjectName);
                    objectNameSetter.name = "objectNameSetter";
                    objectNameSetter.text = "Fix Naming\nConvention";
                    container.Add(objectNameSetter);
                    RootVisualElement.Add(container);
                    container.SendToBack();
                }
                else
                {
                    RootVisualElement.Add(container);
                    container.SendToBack();
                }
            }
            else if(objectNameSetter != null)
            {
                objectNameSetter.RemoveFromHierarchy();
            }

            return null;
        }

        private void SetObjectName()
        {
            var origName = TargetType.name;
            TargetType.name = Prefix + origName;
            AssetDatabaseUtils.UpdateNameOfObject(TargetType);
        }
    }
}