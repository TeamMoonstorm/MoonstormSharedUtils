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
        internal static ConfigEntry<KeyCode> instantiateMaterialTester;

        /// <summary>
        /// The events config file
        /// </summary>
        public static ConfigFile eventsConfig;
        internal static ConfigEntry<bool> addDummyEvents;
        internal static ConfigEntry<KeyCode> printDebugEventMessage;
        internal static ConfigEntry<float> maxDifficultyScaling;
        internal static ConfigEntry<float> maxOpacityForEventMessage;
        internal static ConfigEntry<float> eventMessageFontSize;
        internal static ConfigEntry<float> eventMessageYOffset;
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

            instantiateMaterialTester = generalConfig.Bind<KeyCode>("MoonstormSharedUtils :: Keybinds",
                                                             "Instantiate Material Tester",
                                                             KeyCode.Insert,
                                                             "Keybind used for instantiating the material tester." +
                                                             "Only available if EnableDebugFeatures is set to true");

            addDummyEvents = eventsConfig.Bind("MoonstormSharedUtils :: Events",
                "Add Dummy Events",
                false,
                "Adds a dummy event card that can be triggered manually on every stage. this event card does nothing in particular and its purely for testing purposes");

            printDebugEventMessage = eventsConfig.Bind($"MoonstormSharedUtils :: Events",
                "Print Debug Event Message",
                KeyCode.KeypadMinus,
                "Keybind used for printing a debug event message." +
                "Only available if EnableDebugFeatures is set to true");

            maxDifficultyScaling = eventsConfig.Bind("MoonstormSharedUtils :: Events",
                "Max Difficulty Scaling",
                3.5f,
                "The maximum difficulty scaling for events, this is used for calculating the event duration among other tidbits such as event effects");

            maxOpacityForEventMessage = eventsConfig.Bind("MoonstormSharedUtils :: Event Messages",
                "Max Opacity for Event Message",
                0.75f,
                "The maximum opacity for the event message. Irrelevant if the Event Announcement system is disabled via configs.");

            eventMessageFontSize = eventsConfig.Bind("MoonstormSharedUtils :: Event Messages",
                "Event Message Font Size",
                40f,
                "The size of the font used in the event message. Irrelevant if the Event Announcement system is disabled via configs.");

            eventMessageYOffset = eventsConfig.Bind("MoonstormSharedUtils :: Event Messages",
                "Event Message Y Offset",
                225f,
                "The Y Offset for the event message. Irrelevant if the Event Announcement system is disabled via configs.");

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
}
