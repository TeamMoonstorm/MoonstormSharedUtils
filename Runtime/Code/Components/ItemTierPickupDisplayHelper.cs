using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Moonstorm.Experimental
{
    /// <summary>
    /// A monobehaviour that displays a custom ItemTier pickup display
    /// </summary>
    public class ItemTierPickupDisplayHelper : MonoBehaviour
    {
        [SystemInitializer(new Type[] {typeof(ItemTierCatalog), typeof(ItemTierModuleBase)})]
        private static void SystemInitializer()
        {
            On.RoR2.PickupDisplay.DestroyModel += PickupDisplay_DestroyModel;
            On.RoR2.PickupDisplay.RebuildModel += PickupDisplay_RebuildModel;
        }

        private static void PickupDisplay_RebuildModel(On.RoR2.PickupDisplay.orig_RebuildModel orig, PickupDisplay self)
        {
            var component = self.gameObject.EnsureComponent<ItemTierPickupDisplayHelper>();
            orig(self);
            component.OnPickupDisplayRebuildModel();
        }

        private static void PickupDisplay_DestroyModel(On.RoR2.PickupDisplay.orig_DestroyModel orig, PickupDisplay self)
        {
            var component = self.gameObject.EnsureComponent<ItemTierPickupDisplayHelper>();
            orig(self);
            component.OnPickupDisplayDestroyModel();
        }

        private PickupDisplay display;
        private GameObject effectInstance;

        private void Awake()
        {
            display = GetComponent<PickupDisplay>();
        }

        private void OnPickupDisplayRebuildModel()
        {
            PickupDef pickupDef = PickupCatalog.GetPickupDef(display.pickupIndex);
            ItemIndex itemIndex = pickupDef?.itemIndex ?? ItemIndex.None;
            if(itemIndex != ItemIndex.None)
            {
                ItemTier itemTier = ItemCatalog.GetItemDef(itemIndex).tier;
                ItemTierDef itemTierDef = ItemTierCatalog.GetItemTierDef(itemTier);
                if(itemTierDef && ItemTierModuleBase.MoonstormItemTiers.TryGetValue(itemTierDef, out var itemTierBase))
                {
                    if(itemTierBase != null && itemTierBase.PickupDisplayVFX)
                    {
                        effectInstance = Instantiate(itemTierBase.PickupDisplayVFX, display.gameObject.transform);
                        effectInstance.transform.position = Vector3.zero;
                    }
                }
            }
        }

        private void OnPickupDisplayDestroyModel()
        {
            if(effectInstance)
            {
                Destroy(effectInstance);
            }
        }
    }
}