using BepInEx;
using BepInEx.Configuration;
using Moonstorm.Config;
using Moonstorm.Loaders;
using RiskOfOptions;
using RiskOfOptions.OptionConfigs;
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
        /// <summary>
        /// The events config file
        /// </summary>
        public static ConfigFile eventsConfig;

#if DEBUG
        internal static ConfigurableEnum<KeyCode> instantiateMaterialTester;
        internal static ConfigurableEnum<KeyCode> printDebugEventMessage;
        internal static ConfigurableBool addDummyEvent;
#endif

        internal static ConfigurableFloat maxDifficultyScaling;
        internal static ConfigurableFloat maxOpacityForEventMessage;
        internal static ConfigurableFloat eventMessageFontSize;
        internal static ConfigurableFloat eventMessageYOffset;
        internal static ConfigurableFloat eventMessageXOffset;
        internal static ConfigurableBool eventAnnouncementsAsChatMessages;
        internal static ConfigurableBool familyEventUsesEventAnnouncementInsteadOfChatMessage;

        internal void Init()
        {
            generalConfig = CreateConfigFile(general, false);
            eventsConfig = CreateConfigFile(events, false);
            SetConfigs();

            ModSettingsManager.SetModIcon(MoonstormSharedUtils.MSUAssetBundle.LoadAsset<Sprite>("icon"));
            ModSettingsManager.SetModDescription("An API focused with the intention of working in an editor enviroment using ThunderKit, MSU is a modular API system designed for ease of use and simplicity.");
        }

        private static void SetConfigs()
        {
#if DEBUG
            instantiateMaterialTester = new ConfigurableEnum<KeyCode>(KeyCode.Insert)
            {
                Section = "KeyBinds",
                Description = "Keybind used for instantiating the material tester.",
                ConfigFile = generalConfig,
            };

            printDebugEventMessage = new ConfigurableEnum<KeyCode>(KeyCode.KeypadMinus)
            {
                Section = "Events",
                Description = "Keybind used for printing a debug event message.",
                ConfigFile = eventsConfig
            };

            addDummyEvent = new ConfigurableBool(false)
            {
                Section = "Events",
                Description = "Adds a Dummy event card that can be triggered manually on every stage. this event card does nothing in particular and its purely for testing purposes",
                Key = "Add Dummy Event",
                ConfigFile = eventsConfig,
                CheckBoxConfig = new CheckBoxConfig
                {
                    restartRequired = true,
                },
                ModGUID = MoonstormSharedUtils.GUID,
                ModName = MoonstormSharedUtils.MODNAME,
            }.DoConfigure();
#endif

            maxDifficultyScaling = new ConfigurableFloat(3.5f)
            {
                Section = "Events",
                Description = "The maximum difficulty scaling for vents, this is used for calculating event duration and other tidbits such as event effects.",
                ConfigFile = eventsConfig,
                UseStepSlider = false,
                SliderConfig = new SliderConfig
                {
                    min = 1f,
                    max = 10f,
                    formatString = "{0:0.00}", 
                }
            };

            maxOpacityForEventMessage = new ConfigurableFloat(0.75f)
            {
                Section = "Event Messages",
                Description = "The Maximum opacity for the event message.",
                ConfigFile = eventsConfig,
                UseStepSlider = false,
                SliderConfig = new SliderConfig
                {
                    min = 0f,
                    max = 1f,
                    formatString = "{0:0.0%}",
                    checkIfDisabled = () => eventAnnouncementsAsChatMessages
                }
            };

            eventMessageFontSize = new ConfigurableFloat(40f)
            {
                Section = "Event Messages",
                Description = "The Size of the font used in the event message.",
                ConfigFile = eventsConfig,
                UseStepSlider = false,
                SliderConfig = new SliderConfig
                {
                    min = 0f,
                    max = 100f,
                    checkIfDisabled = () => eventAnnouncementsAsChatMessages
                }
            };

            eventMessageYOffset = new ConfigurableFloat(225f)
            {
                Section = "Event Messages",
                Description = "The Y Offset for the event message.",
                ConfigFile = eventsConfig,
                UseStepSlider = false,
                SliderConfig = new SliderConfig
                {
                    min = 0f,
                    max = 4320f,
                    formatString = "{0:0.0}",
                    checkIfDisabled = () => eventAnnouncementsAsChatMessages
                }
            };

            eventMessageXOffset = new ConfigurableFloat(0f)
            {
                Section = "Event Messages",
                Description = "The X Offset for the event message.",
                ConfigFile = eventsConfig,
                UseStepSlider = false,
                SliderConfig = new SliderConfig
                {
                    min = -2560f,
                    max = 2560f,
                    formatString = "{0:0.0}",
                    checkIfDisabled = () => eventAnnouncementsAsChatMessages
                }
            };

            eventAnnouncementsAsChatMessages = new ConfigurableBool(false)
            {
                Section = "Event Messages",
                Description = "If set to true, Event Announcements will appear in the chat instead of their own UI container",
                ConfigFile = eventsConfig,
            };

            familyEventUsesEventAnnouncementInsteadOfChatMessage = new ConfigurableBool(true)
            {
                Section = "Event Messages",
                Description = "Setting this to true causes the family event chat message to display as an event announcement instead.",
                ConfigFile = eventsConfig
            };
        }
    }
}
