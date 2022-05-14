using RoR2EditorKit.Core.Inspectors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using RoR2EditorKit.Utilities;
using RoR2;
using Moonstorm.AddressableAssets;
using System.Globalization;

namespace Moonstorm.EditorUtils.Inspectors
{
    [CustomEditor(typeof(MSIDRS))]
    public class MSIDRSInspector : IMGUIToVisualElementInspector<MSIDRS>
    {
        ObjectField idrsField;
        protected override void FinishGUI()
        {
            VisualElement root = new VisualElement();
            root.style.paddingBottom = 10;

            idrsField = new ObjectField();
            idrsField.SetObjectType<ItemDisplayRuleSet>();
            idrsField.label = "Item Display Rule Set";
            root.Add(idrsField);

            Button button = new Button();
            button.text = $"Update to NamedIDRS";
            button.tooltip = $"Creates a new NamedIDRS using the data from this MSSingleItemDisplayRule" +
                $"\nThe IDRS will be taken from the object field above" +
                $"\nThe DisplayPrefab and KeyAsset will be set to each element's AddressableGameObject & AddressableKeyAsset's address fields, be sure to fix this later on.";
            button.clicked += UpdateToNIDRS;
            root.Add(button);

            RootVisualElement.Add(root);
            root.SendToBack();
            Debug.Log(typeof(LimbFlags).AssemblyQualifiedName);
        }

        private async void UpdateToNIDRS()
        {
            NamedIDRS namedIDRS = CreateInstance<NamedIDRS>();
            namedIDRS.name = $"named{idrsField.value.name}";
            namedIDRS.idrs = (ItemDisplayRuleSet)idrsField.value;
            
            for(int i = 0; i < TargetType.MSUKeyAssetRuleGroup.Count; i++)
            {
                MSIDRS.KeyAssetRuleGroup karg = TargetType.MSUKeyAssetRuleGroup[i];
                NamedIDRS.AddressNamedRuleGroup namedRuleGroup = new NamedIDRS.AddressNamedRuleGroup { keyAsset = new AddressableKeyAsset() };
                namedRuleGroup.keyAsset.address = karg.keyAssetName;
                foreach(MSIDRS.ItemDisplayRule idr in karg.rules)
                {
                    namedRuleGroup.AddRule(await CreateRule(idr));
                }
                namedIDRS.namedRuleGroups.Add(namedRuleGroup);
            }
            AssetDatabaseUtils.CreateAssetAtSelectionPath(namedIDRS);
        }

        private async Task<NamedIDRS.AdressNamedDisplayRule> CreateRule(MSIDRS.ItemDisplayRule oldRule)
        {
            NamedIDRS.AdressNamedDisplayRule newRule = new NamedIDRS.AdressNamedDisplayRule();

            List<string> splitValues = oldRule.IDPHValues.Split(',').ToList();
            newRule.childName = splitValues[0];
            newRule.localPos = await CreateVector3FromList(new List<string> { splitValues[1], splitValues[2], splitValues[3] });
            newRule.localAngles = await CreateVector3FromList(new List<string> { splitValues[4], splitValues[5], splitValues[6] });
            newRule.localScales = await CreateVector3FromList(new List<string> { splitValues[7], splitValues[8], splitValues[9] });
            newRule.limbMask = oldRule.limbMask;
            newRule.ruleType = oldRule.ruleType;
            newRule.displayPrefab = new AddressableGameObject { address = oldRule.displayPrefabName };

            return newRule;
        }

        private async Task<Vector3> CreateVector3FromList(List<string> args)
        {
            Vector3 toReturn = Vector3.zero;
            await Task.Run(() =>
            {
                toReturn = new Vector3(float.Parse(args[0], CultureInfo.InvariantCulture), float.Parse(args[1], CultureInfo.InvariantCulture), float.Parse(args[2], CultureInfo.InvariantCulture));
            });
            return toReturn;
        }
    }
}
