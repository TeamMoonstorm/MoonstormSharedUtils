using BepInEx;
using BepInEx.Configuration;
using MSU.Config;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExampleMod
{
    public class ExampleConfig
    {
        public const string PREFIX = "EXAMPLEMOD.";
        public const string ID_MAIN = PREFIX + "Main";
        public const string ID_ITEM = PREFIX + "Items";
        public const string ID_EQUIPMENT = PREFIX + "Equipments";

        internal static ConfigFactory configFactory { get; private set; }

        public static ConfigFile configMain { get; private set; }
        public static ConfigFile configItems { get; private set; }
        public static ConfigFile configEquipments { get; private set; }

        internal static IEnumerator RegisterToModSettingsManager()
        {
            yield break;
        }

        internal ExampleConfig(BaseUnityPlugin bup)
        {
            configFactory = new ConfigFactory(bup, true);
            configMain = configFactory.CreateConfigFile(ID_MAIN, true);
            configItems = configFactory.CreateConfigFile(ID_ITEM, true);
            configEquipments = configFactory.CreateConfigFile(ID_EQUIPMENT, true);
        }
    }
}