using System.Collections.Generic;
using System.Linq;
using ThunderKit.Core.Config;
using ThunderKit.Core.Data;
using UnityEngine;

namespace Moonstorm.EditorUtils.Importers
{
    public class AssemblyBlacklister : BlacklistProcessor
    {
        public override string Name => "MSU Related Assembly Blacklist";
        public override int Priority => 9_000;

        public override IEnumerable<string> Process(IEnumerable<string> blacklist)
        {
            var importConfig = ThunderKitSetting.GetOrCreateSettings<ImportConfiguration>();

            string old = string.Join("\n", blacklist);
            Debug.Log($"Old: \n{old}");
            if (importConfig.ConfigurationExecutors.OfType<WWiseBlacklister>().Any(ie => ie.enabled))
            {
                blacklist = blacklist.Append($"Wwise.dll");
                blacklist = blacklist.Append($"AkSoundEngine.dll");
                blacklist = blacklist.Append($"AkWaapiClient.dll");
            }
            string neww = string.Join("\n", blacklist);
            Debug.Log($"New: \n{neww}");
            return blacklist;
        }
    }
}