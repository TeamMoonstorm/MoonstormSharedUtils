#if RISKOFTHUNDER_R2API_ADDRESSABLES
using MSU.AddressReferencedAssets;
using RoR2;
using RoR2.Editor.PropertyDrawers;
using System;
using UnityEditor;

namespace MSU.Editor.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(AddressReferencedSkinDef))]
    public class AddressReferencedSkinDefDrawer : AddressReferencedAssetDrawer<AddressReferencedSkinDef>
    {
        protected override Type GetRequiredAssetType()
        {
            return typeof(SkinDef);
        }
    }
}
#endif