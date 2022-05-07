using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using RoR2EditorKit.Core.Inspectors;
using RoR2;
using System;
using ThunderKit.Core.Data;
using RoR2EditorKit.Settings;

namespace RoR2EditorKit.RoR2Related.Inspectors
{
    [CustomEditor(typeof(ItemDef))]
    public class ItemDefInspector : ScriptableObjectInspector<ItemDef>
    {
        protected override string Prefix => "";

        protected override bool PrefixUsesTokenPrefix => false;

        protected override bool HasVisualTreeAsset => true;

        private VisualElement inspectorDataHolder;
        private VisualElement itemTierHolder;
        private VisualElement tokenHolder;

        private PropertyField itemTierDef;

        protected override void OnEnable()
        {
            base.OnEnable();
            OnVisualTreeCopy += () =>
            {
                var container = DrawInspectorElement.Q<VisualElement>("Container");
                inspectorDataHolder = container.Q<VisualElement>("InspectorDataHolder");
                itemTierHolder = inspectorDataHolder.Q<VisualElement>("ItemTierHolder");
                itemTierDef = itemTierHolder.Q<PropertyField>("itemTierDef");
                tokenHolder = inspectorDataHolder.Q<VisualElement>("TokenHolder");
            };
        }
        protected override void DrawInspectorGUI()
        {
            var deprecatedTierProp = serializedObject.FindProperty("deprecatedTier");
            var enumValue = (ItemTier)deprecatedTierProp.enumValueIndex;
            var deprecatedTier = itemTierHolder.Q<PropertyField>("deprecatedTier");
            deprecatedTier.RegisterCallback<ChangeEvent<string>>(OnTierEnumSet);

            itemTierDef.style.display = enumValue == ItemTier.AssignedAtRuntime ? DisplayStyle.Flex : DisplayStyle.None;

            tokenHolder.AddManipulator(new ContextualMenuManipulator((manipulator) =>
            {
                manipulator.menu.AppendAction("Set Tokens", SetTokens, (callback) =>
                {
                    var tokenPrefix = Settings.TokenPrefix;
                    if (string.IsNullOrEmpty(tokenPrefix))
                        return DropdownMenuAction.Status.Disabled;
                    return DropdownMenuAction.Status.Normal;
                });
            }));
        }

        private void SetTokens(DropdownMenuAction action)
        {
            if (string.IsNullOrEmpty(Settings.TokenPrefix))
                throw ErrorShorthands.NullTokenPrefix();

            string objName = serializedObject.targetObject.name.ToLowerInvariant().Replace(" ", "");
            if (!string.IsNullOrEmpty(Prefix) && objName.Contains(Prefix.ToLowerInvariant()))
            {
                objName = objName.Replace(Prefix.ToLowerInvariant(), "");
            }
            string tokenBase = $"{Settings.GetPrefixUppercase()}_ITEM_{objName.ToUpperInvariant()}_";
            TargetType.nameToken = $"{tokenBase}NAME";
            TargetType.pickupToken = $"{tokenBase}PICKUP";
            TargetType.descriptionToken = $"{tokenBase}DESC";
            TargetType.loreToken = $"{tokenBase}LORE";
            serializedObject.Update();
            serializedObject.ApplyModifiedProperties();
        }

        private void OnTierEnumSet(ChangeEvent<string> evt)
        {
            string val = evt.newValue.Replace(" ", "");
            if(Enum.TryParse<ItemTier>(val, out ItemTier newTier))
            {
                itemTierDef.style.display = newTier == ItemTier.AssignedAtRuntime ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }
    }
}