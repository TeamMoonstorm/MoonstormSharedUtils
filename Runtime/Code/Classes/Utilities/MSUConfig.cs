using BepInEx;
using BepInEx.Configuration;
using Moonstorm.Loaders;
using UnityEngine;

namespace Moonstorm
{
    /// <summary>
    /// MSU's Configuration Loader
    /// </summary>
    public class MSUConfig : ConfigLoader<MSUConfig>
    {
        /// <summary>
        /// Identifier for the General config file
        /// </summary>
        public const string general = "MSU.General";
        /// <summary>
        /// Identifier for the Events file
        /// </summary>
        public const string events = "MSU.Events";
        public override BaseUnityPlugin MainClass => MoonstormSharedUtils.Instance;
        public override bool CreateSubFolder => true;

        /// <summary>
        /// The general config file
        /// </summary>
        public static ConfigFile generalConfig;
        internal static ConfigEntry<bool> enableDebugFeatures;
        internal static ConfigEntry<bool> enableLoggingOfIDRS;
        internal static ConfigEntry<KeyCode> instantiateMaterialTester;

        /// <summary>
        /// The events config file
        /// </summary>
        public static ConfigFile eventsConfig;
        internal static ConfigEntry<bool> addDummyEvents;
        internal static ConfigEntry<float> maxDifficultyScaling;
        internal static ConfigEntry<bool> familyEventUsesEventAnnouncementInsteadOfChatMessage;
        internal static ConfigEntry<bool> eventAnnouncementsAsChatMessages;

        internal void Init()
        {
            generalConfig = CreateConfigFile(general, false);
            eventsConfig = CreateConfigFile(events, false);

            SetConfigs();
        }

        private static void SetConfigs()
        {
            enableDebugFeatures = generalConfig.Bind<bool>("MoonstormSharedUtils :: Debug Features",
                                                    "Enable Debug",
                                                    false,
                                                    "Enables Debug features from Moonstorm Shared Utils, Features include:" +
                                                    "\nAbility to connect to yourself via a second instance of the game" +
                                                    "\nAddition of the MoonstormItemDisplayHelper" +
                                                    "\nAutomatic deployment of \"stage1_pod\", \"no_enemies\" & \"enable_event_logging\", assuming DebugToolkit is installed" +
                                                    "\nSpawning of the MaterialTester");

            enableLoggingOfIDRS = generalConfig.Bind<bool>("MoonstormSharedUtils :: IDRS",
                                                    "Log IDRS-Related names",
                                                    false,
                                                    "Setting this to true causes MSU to Log inportant KEY values from RoR2's IDRS system\n" +
                                                    "It'll Log:\n" +
                                                    "IDRS names\n" +
                                                    "Key Assets names (ItemDefs & Equipment Defs)\n" +
                                                    "Display Prefabs");

            instantiateMaterialTester = generalConfig.Bind<KeyCode>("MoonstormSharedUtils :: Keybinds",
                                                             "Instantiate Material Tester",
                                                             KeyCode.Insert,
                                                             "Keybind used for instantiating the material tester." +
                                                             "Only available if EnableDebugFeatures is set to true");

            addDummyEvents = eventsConfig.Bind("MoonstormSharedUtils :: Events",
                "Add Dummy Events",
                false,
                "Adds a dummy event card that can be triggered manually on every stage. this event card does nothing in particular and its purely for testing purposes");

            maxDifficultyScaling = eventsConfig.Bind("MoonstormSharedUtils :: Events",
                "Max Difficulty Scaling",
                3.5f,
                "The maximum difficulty scaling for events, this is used for calculating the event duration among other tidbits such as event effects");

            eventAnnouncementsAsChatMessages = eventsConfig.Bind<bool>("MoonstormSharedUtils :: Event Messages",
                "Event Announcements as Chat Messages",
                false,
                "If set to true, Event announcements will appear in the chat instead of their own UI container");

            familyEventUsesEventAnnouncementInsteadOfChatMessage = eventsConfig.Bind<bool>("MoonstormSharedUtils :: Event Messages",
                "Family Event chat message as Event Announcement",
                true,
                "Setting this to true causes the family event chat message to display as an event announcement instead");
        }
    }
    /*public static class MSUConfig
    {
        public static ConfigEntry<bool> EnableDebugFeatures;

        public static ConfigEntry<bool> EnableLoggingOfIDRS;

        public static ConfigEntry<KeyCode> InstantiateMaterialTester;

        internal static void Init(ConfigFile config)
        {
            EnableDebugFeatures = config.Bind<bool>("MoonstormSharedUtils :: Debug Features",
                                                    "Enable Debug",
                                                    false,
                                                    "Enables Debug features from Moonstorm Shared Utils");

            EnableLoggingOfIDRS = config.Bind<bool>("MoonstormSharedUtils :: IDRS",
                                                    "Log IDRS-Related names",
                                                    false,
                                                    "Setting this to true causes MSU to Log inportant KEY values from RoR2's IDRS system\n" +
                                                    "It'll Log:\n" +
                                                    "IDRS names\n" +
                                                    "Key Assets names (ItemDefs & Equipment Defs)\n" +
                                                    "Display Prefabs");

            InstantiateMaterialTester = config.Bind<KeyCode>("MoonstormSharedUtils :: Keybinds",
                                                             "Instantiate Material Tester",
                                                             KeyCode.Insert,
                                                             "Keybind used for instantiating the material tester.");
        }
    }*/
}
