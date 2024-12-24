using RoR2;
using UnityEngine;

namespace MSU
{
    /// <summary>
    /// A class utilized to display custom Pickup VFX for new ItemTiers added using MSU's Systems.
    /// </summary>
    public class ItemTierPickupDisplayHelper : MonoBehaviour
    {
        private PickupDisplay _display;
        private GameObject _vfxInstance;
        private void Awake()
        {
            _display = GetComponent<PickupDisplay>();
        }

        /// <summary>
        /// Rebuilds the custom model.
        /// </summary>
        public void RebuildCustomModel()
        {
            if (!_display)
                return;

            PickupDef def = PickupCatalog.GetPickupDef(_display.pickupIndex);
            if (def == null)
                return;

            ItemTier tier = def.itemTier;
            if (tier > ItemTier.AssignedAtRuntime)
            {
                ItemTierDef tierDef = ItemTierCatalog.GetItemTierDef(tier);
                if (tierDef && ItemTierModule._itemTierToPickupFX.TryGetValue(tierDef, out var prefab))
                {
                    if (!prefab)
                    {
#if DEBUG
                        MSULog.Warning($"{tierDef} is being handled by the ItemTierModule but it's IItemTierContentPiece does not provide a custom prefab for VFX!");
#endif
                        return;
                    }
                    _vfxInstance = Instantiate(prefab, _display.transform.parent);
                }
            }
        }

        /// <summary>
        /// Destroys the custom model
        /// </summary>
        public void DestroyCustomModel()
        {
            if (_vfxInstance)
            {
                Destroy(_vfxInstance);
            }
        }
    }
}
