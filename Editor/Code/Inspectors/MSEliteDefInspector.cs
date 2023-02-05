using RoR2;
using RoR2EditorKit.Core.Inspectors;
using RoR2EditorKit.Utilities;
using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Base = RoR2EditorKit.RoR2Related.Inspectors.EliteDefInspector;

namespace Moonstorm.EditorUtils.Inspectors
{
    [CustomEditor(typeof(MSEliteDef))]
    public class MSEliteDefInspector : MSScriptableObjectInspector<MSEliteDef>, IObjectNameConvention
    {
        public string Prefix => "med";
        public bool UsesTokenForPrefix => false;

        VisualElement inspectorData;
        PropertyField eliteTierDefs;
        PropertyValidator<UnityEngine.Object> equipValidator;
        protected override void OnEnable()
        {
            base.OnEnable();

            OnVisualTreeCopy += () =>
            {
                var container = DrawInspectorElement.Q<VisualElement>("Container");
                inspectorData = container.Q<VisualElement>("InspectorDataContainer");
                eliteTierDefs = inspectorData.Q<PropertyField>("eliteTierDefIdentifiers");
                equipValidator = new PropertyValidator<UnityEngine.Object>(inspectorData.Q<PropertyField>("eliteEquipmentDef"), DrawInspectorElement);
            };
        }

        protected override void DrawInspectorGUI()
        {
            EnumField flagsField = new EnumField("Elite Tiers", VanillaEliteTier.None);
            flagsField.bindingPath = nameof(MSEliteDef.eliteTier);
            var indexOfCustomPropLabel = inspectorData.IndexOf(inspectorData.Q<Label>("CustomProperties"));
            inspectorData.Insert(indexOfCustomPropLabel + 1, flagsField);

            SetupEquipValidator(equipValidator);
            equipValidator.ForceValidation();

            var modifierToken = inspectorData.Q<PropertyField>("modifierToken");
            AddSimpleContextMenu(modifierToken, new ContextMenuData(
                "Set Token",
                SetToken,
                statusCheck =>
                {
                    if (Settings.TokenPrefix.IsNullOrEmptyOrWhitespace())
                        return DropdownMenuAction.Status.Disabled;
                    return DropdownMenuAction.Status.Normal;
                }));

            var eliteColor = inspectorData.Q<PropertyField>("color");
            AddSimpleContextMenu(eliteColor, new ContextMenuData(
                "Set Color to Buff Color",
                SetColor,
                statusCheck =>
                {
                    if (TargetType.eliteEquipmentDef && TargetType.eliteEquipmentDef.passiveBuffDef)
                        return DropdownMenuAction.Status.Normal;
                    return DropdownMenuAction.Status.Hidden;
                }));

            var statCoefficients = inspectorData.Q<Foldout>("StatCoefficientContainer");
            BuildContextMenu(statCoefficients);
        }

        private void SetupEquipValidator(PropertyValidator<UnityEngine.Object> validator)
        {
            validator.AddValidator(() =>
            {
                var eqp = GetEquipmentDef();
                return !eqp;
            },
            "This EliteDef has no EquipmentDef assigned! Is this intentional?");

            validator.AddValidator(() =>
            {
                var eqp = GetEquipmentDef();
                return eqp && !eqp.passiveBuffDef;
            },
            $"You've assigned an EquipmentDef to this Elite, but the assigned Equipment's has no passiveBuffDef assigned!", MessageType.Warning);

            validator.AddValidator(() =>
            {
                var eqp = GetEquipmentDef();
                return eqp && eqp.passiveBuffDef && eqp.passiveBuffDef.eliteDef != TargetType;
            }, $"You've associated an EquipmentDef to this Elite, but the assigned EquipmentDef's \"passiveBuffDef\"'s EliteDef is not the inspected EliteDef!", MessageType.Warning);

            EquipmentDef GetEquipmentDef() => validator.ChangeEvent == null ? TargetType.eliteEquipmentDef : (EquipmentDef)validator.ChangeEvent.newValue;
        }

        private void SetToken(DropdownMenuAction act)
        {
            string objectName = target.name;
            if (objectName.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase))
            {
                objectName = objectName.Substring(Prefix.Length);
            }
            objectName = objectName.Replace(" ", "");
            TargetType.modifierToken = $"{Settings.GetPrefixUppercase()}_ELITE_MODIFIER_{objectName.ToUpperInvariant()}";
        }

        private void SetColor(DropdownMenuAction act)
        {
            TargetType.color = TargetType.eliteEquipmentDef.passiveBuffDef.buffColor;
        }

        private void BuildContextMenu(Foldout statCoefficients)
        {
            Add("Tier1Honor");
            Add("Tier1");
            Add("Tier2");

            void Add(string name)
            {
                AddSimpleContextMenu(statCoefficients, new ContextMenuData($"Set Coefficients To/{name}", SetCoefficients, check =>
                {
                    return AddressablesUtils.AddressableCatalogExists ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.None;
                }));
            }
        }

        private async void SetCoefficients(DropdownMenuAction action)
        {
            string assetName = action.name.Substring($"Set Coefficients To/".Length);
            string address = string.Empty;
            switch (assetName)
            {
                case "Tier1Honor": address = Base.Tier1HonorAddress; break;
                case "Tier1": address = Base.Tier1Address; break;
                case "Tier2": address = Base.Tier2Address; break;
            }
            if (address == string.Empty)
                return;

            EliteDef vanillaEliteDef = await AddressablesUtils.LoadAssetFromCatalog<EliteDef>(address);

            if (!vanillaEliteDef)
                return;

            TargetType.healthBoostCoefficient = vanillaEliteDef.healthBoostCoefficient;
            TargetType.damageBoostCoefficient = vanillaEliteDef.damageBoostCoefficient;
        }

        public PrefixData GetPrefixData()
        {
            return new PrefixData(() =>
            {
                var origName = TargetType.name;
                TargetType.name = $"{Prefix}{origName}";
                AssetDatabaseUtils.UpdateNameOfObject(TargetType);
            });
        }
    }
}
