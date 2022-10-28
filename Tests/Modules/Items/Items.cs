using R2API.ScriptableObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moonstorm.Modules
{
    public class Items : ItemModuleBase
    {
        public override R2APISerializableContentPack SerializableContentPack => MSUTContent.Instance.SerializableContentPack;

        public override void Initialize()
        {
            MSUTLog.Info("Items module initialized");
            base.Initialize();
            MSUTLog.Info("Getting item bases");
            GetItemBases();
        }

        protected override IEnumerable<ItemBase> GetItemBases()
        {
            foreach(ItemBase ib in base.GetItemBases())
            {
                MSUTLog.Info($"Adding {ib.GetType().BaseType.Name} {ib.GetType().Name}");
                AddItem(ib);
            }
            return null;
        }
    }
}
