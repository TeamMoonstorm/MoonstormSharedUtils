using R2API.ScriptableObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moonstorm.Modules
{
    public class Buffs : BuffModuleBase
    {
        public override R2APISerializableContentPack SerializableContentPack => MSUTContent.Instance.SerializableContentPack;

        public override void Initialize()
        {
            MSUTLog.Info("Buff Module initialized");
            base.Initialize();
            MSUTLog.Info("Getting buff bases");
            GetBuffBases();
        }

        protected override IEnumerable<BuffBase> GetBuffBases()
        {
            foreach(BuffBase bb in base.GetBuffBases())
            {
                MSUTLog.Info($"Adding {bb.GetType().BaseType.Name} {bb.GetType().Name}");
                AddBuff(bb);
            }
            return null;
        }
    }
}
