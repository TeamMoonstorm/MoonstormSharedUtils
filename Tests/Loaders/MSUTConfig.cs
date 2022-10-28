using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Configuration;
using Moonstorm.Loaders;

namespace Moonstorm
{
    public class MSUTConfig : ConfigLoader<MSUTConfig>
    {
        public const string items = "MSU.Items";
        public const string equips = "MSU.Equips";
        public override BaseUnityPlugin MainClass => MSUTestsMain.Instance;

        public override bool CreateSubFolder => true;

        public static ConfigFile itemConfig;
        public static ConfigFile equipsConfig;

        internal void Init()
        {
            MSUTLog.Info("ConfigLoader Initialized");
            itemConfig = CreateConfigFile(items);
            MSUTLog.Info($"Config file {itemConfig.ConfigFilePath} created");
            equipsConfig = CreateConfigFile(equips);
            MSUTLog.Info($"Config file {equipsConfig.ConfigFilePath} created");
        }
    }
}