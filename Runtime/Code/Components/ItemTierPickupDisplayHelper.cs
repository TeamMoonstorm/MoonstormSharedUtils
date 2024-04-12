using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            if(tier > ItemTier.AssignedAtRuntime)
            {
                ItemTierDef tierDef = ItemTierCatalog.GetItemTierDef(tier);
                if(tierDef && ItemTierModule._itemTierToPickupFX.TryGetValue(tierDef, out var prefab))
                {
                    _vfxInstance = Instantiate(prefab, _display.transform);
                    _vfxInstance.transform.position = Vector3.zero;
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
