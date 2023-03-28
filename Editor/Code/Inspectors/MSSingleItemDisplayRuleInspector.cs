using Moonstorm.AddressableAssets;
using RoR2EditorKit.Inspectors;
using RoR2EditorKit;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Moonstorm.EditorUtils.Inspectors
{
    [CustomEditor(typeof(MSSingleItemDisplayRule))]
    public class MSSingleItemDisplayRuleInspector : IMGUIToVisualElementInspector<MSSingleItemDisplayRule>
    {
        ObjectField keyAsset;
        ObjectField displayPrefab;
        protected override void FinishGUI()
        {
            VisualElement root = new VisualElement();
            root.style.paddingBottom = 10;

            keyAsset = new ObjectField();
            keyAsset.SetObjectType<UnityEngine.Object>();
            keyAsset.label = $"Key Asset";
            root.Add(keyAsset);

            displayPrefab = new ObjectField();
            displayPrefab.SetObjectType<GameObject>();
            displayPrefab.label = $"Display Prefab";
            root.Add(displayPrefab);

            Button button = new Button();
            button.text = $"Update to ItemDisplayDictionary";
            button.tooltip = $"Creates a new ItemDisplayDictionary using the data from this MSSingleItemDisplayRule" +
                $"\nThe KeyAsset and DisplayPrefabs will be taken from the objects above" +
                $"\nThe IDRS for each single item display rule will be set to the AddressableIDRS's address, be sure to fix this later on.";
            button.clicked += UpdateToIDD;
            root.Add(button);

            RootVisualElement.Add(root);
            root.SendToBack();
        }

        private async void UpdateToIDD()
        {
            ItemDisplayDictionary itemDisplayDictionary = CreateInstance<ItemDisplayDictionary>();
            char[] nameAsCharArray = keyAsset.value.name.ToCharArray();
            nameAsCharArray[0] = char.ToUpper(nameAsCharArray[0]);
            itemDisplayDictionary.name = $"idd{new string(nameAsCharArray)}";
            itemDisplayDictionary.displayPrefab = (GameObject)displayPrefab.value;
            itemDisplayDictionary.keyAsset = keyAsset.value;

            for (int i = 0; i < TargetType.singleItemDisplayRules.Count; i++)
            {
                MSSingleItemDisplayRule.SingleKeyAssetRuleGroup skarg = TargetType.singleItemDisplayRules[i];
                ItemDisplayDictionary.NamedDisplayDictionary namedDisplayDictionary = new ItemDisplayDictionary.NamedDisplayDictionary { idrs = new AddressableIDRS() };
                namedDisplayDictionary.idrs.address = skarg.vanillaIDRSKey;
                foreach (var sidr in skarg.itemDisplayRules)
                {
                    namedDisplayDictionary.AddDisplayRule(await CreateRule(sidr));
                }
                itemDisplayDictionary.namedDisplayDictionary.Add(namedDisplayDictionary);
            }
            AssetDatabaseUtils.CreateAssetAtSelectionPath(itemDisplayDictionary);
        }

        private async Task<ItemDisplayDictionary.DisplayRule> CreateRule(MSSingleItemDisplayRule.SingleItemDisplayRule oldRule)
        {
            ItemDisplayDictionary.DisplayRule newRule = new ItemDisplayDictionary.DisplayRule();

            List<string> splitValues = oldRule.IDPHValues.Split(',').ToList();
            newRule.childName = splitValues[0];
            newRule.localPos = await CreateVector3FromList(new List<string> { splitValues[1], splitValues[2], splitValues[3] });
            newRule.localAngles = await CreateVector3FromList(new List<string> { splitValues[4], splitValues[5], splitValues[6] });
            newRule.localScales = await CreateVector3FromList(new List<string> { splitValues[7], splitValues[8], splitValues[9] });
            newRule.limbMask = oldRule.limbMask;
            newRule.ruleType = oldRule.ruleType;

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