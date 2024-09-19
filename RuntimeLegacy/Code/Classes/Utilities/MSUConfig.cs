using BepInEx;
using BepInEx.Configuration;
using Moonstorm.Config;
using Moonstorm.Loaders;
using RiskOfOptions;
using RiskOfOptions.OptionConfigs;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace Moonstorm
{
    public class MSUConfig : ConfigLoader<MSUConfig>
    {
        public const string general = "MSU.General";

        public const string events = "MSU.Events";

#if DEBUG
        public const string debug = "MSU.Debug";
#endif
        public override BaseUnityPlugin MainClass => MoonstormSharedUtils.Instance;
        public override bool CreateSubFolder => true;

        public static ConfigFile generalConfig;

        public static ConfigFile eventsConfig;

#if DEBUG
        public static ConfigFile debugConfig;
#endif

#if DEBUG
        internal static ConfigurableBool enableSelfConnect;
        internal static ConfigurableBool enableCommandInvoking;
        internal static ConfigurableBool invokeGod;
        internal static ConfigurableBool invokeStage1Pod;
        internal static ConfigurableBool invokeNoMonsters;
        internal static ConfigurableBool invokeMSEnableEventLogging;
        internal static ConfigurableBool invoke100Dios;

        internal static ConfigurableBool enableDebugToolkitBindings;
        internal static ConfigurableString spawnAIParameters;
        internal static HashSet<DebugCommandBinding> bindings = new HashSet<DebugCommandBinding>();

        internal static ConfigurableKeyBind printDebugEventMessage;
        internal static ConfigurableBool addDummyEvent;
#endif

        internal static ConfigurableFloat maxDifficultyScaling;
        internal static ConfigurableFloat maxOpacityForEventMessage;
        internal static ConfigurableFloat eventMessageFontSize;
        internal static ConfigurableFloat eventMessageYOffset;
        internal static ConfigurableFloat eventMessageXOffset;
        internal static ConfigurableBool eventAnnouncementsAsChatMessages;
        internal static ConfigurableBool familyEventUsesEventAnnouncementInsteadOfChatMessage;

        [System.Obsolete]
        internal void Init()
        {
            generalConfig = CreateConfigFile(general, false);
            eventsConfig = CreateConfigFile(events, false);
#if DEBUG
            debugConfig = CreateConfigFile(debug, false, true);
#endif
            SetConfigs();

            var icon = MoonstormSharedUtils.MSUAssetBundle.LoadAsset<Sprite>("icon");
            ModSettingsManager.SetModIcon(icon);
            ModSettingsManager.SetModDescription("An API focused with the intention of working in an editor enviroment using ThunderKit, MSU is a modular API system designed for ease of use and simplicity.");

#if DEBUG
            ModSettingsManager.SetModIcon(icon, MoonstormSharedUtils.GUID + "." + debug, MoonstormSharedUtils.MODNAME + "." + debug);
            ModSettingsManager.SetModDescription("The debug configuration of Moonstorm Shared Utils", MoonstormSharedUtils.GUID + "." + debug, MoonstormSharedUtils.MODNAME + "." + debug);
#endif
        }

        [System.Obsolete]
        private void SetConfigs()
        {
#if DEBUG
            enableSelfConnect = new ConfigurableBool(true)
            {
                Section = "Debugging",
                Description = "Allows you to connect to yourself using a second instance of the game, by hosting a private game with one and opening the console on the other and typing \"connect localhost:7777\"\nYou need to exit the lobby for this to take effect.",
                ConfigFile = debugConfig,
            }.AddOnConfigChanged((b) =>
            {
                if (b)
                {
                    On.RoR2.Networking.NetworkManagerSystem.OnClientConnect -= DoNothing;
                    On.RoR2.Networking.NetworkManagerSystem.OnClientConnect += DoNothing;
                }
                else
                {
                    On.RoR2.Networking.NetworkManagerSystem.OnClientConnect -= DoNothing;
                }
            });

            enableCommandInvoking = new ConfigurableBool(true)
            {
                Section = "Command Invoking",
                Description = "Invokes commands at run start, disabling this causes no commands to be invoked at run start.",
                ConfigFile = debugConfig,
            };

            invokeGod = new ConfigurableBool(true)
            {
                Section = "Command Invoking",
                Description = "Invokes the command \"god\"",
                ConfigFile = debugConfig,
                CheckBoxConfig = new CheckBoxConfig
                {
                    checkIfDisabled = () => !enableCommandInvoking
                }
            };

            invokeStage1Pod = new ConfigurableBool(true)
            {
                Section = "Command Invoking",
                Description = "Invokes the convar \"stage1_pod\" with the value 0",
                ConfigFile = debugConfig,
                CheckBoxConfig = new CheckBoxConfig
                {
                    checkIfDisabled = () => !enableCommandInvoking
                }
            };

            invokeNoMonsters = new ConfigurableBool(true)
            {
                Section = "Command Invoking",
                Description = "Invokes the command \"no_monsters\"",
                ConfigFile = debugConfig,
                CheckBoxConfig = new CheckBoxConfig
                {
                    checkIfDisabled = () => !enableCommandInvoking
                }
            };

            invokeMSEnableEventLogging = new ConfigurableBool(true)
            {
                Section = "Command Invoking",
                Description = "Invokes the convar \"msEnable_Event_Logging\" with the value 1",
                ConfigFile = debugConfig,
                CheckBoxConfig = new CheckBoxConfig
                {
                    checkIfDisabled = () => !enableCommandInvoking
                }
            };

            invoke100Dios = new ConfigurableBool(true)
            {
                Section = "Command Invoking",
                Description = "Invokes \"give_item\" with the params \"extralife 100\". Essentially gives 100 dios on run start",
                ConfigFile = debugConfig,
                CheckBoxConfig = new CheckBoxConfig
                {
                    checkIfDisabled = () => !enableCommandInvoking
                }
            };

            enableDebugToolkitBindings = new ConfigurableBool(true)
            {
                Section = "Debug Toolkit Bindings",
                Description = "Enables the usage of bindings with DebugToolkit. Requires DebugToolkit to be installed.",
                ConfigFile = debugConfig,
                CheckBoxConfig = new CheckBoxConfig
                {
                    checkIfDisabled = () => !MSUtil.DebugToolkitInstalled
                }
            };

            GenerateDebugBinding(KeyCode.None, "NextStage Bind", "Binds the selected key to invoke \"next_stage\"", new DebugCommandBinding.Command("next_stage"));
            GenerateDebugBinding(KeyCode.None, "KillAll Bind", "Binds the selected key to invoke \"kill_all\"", new DebugCommandBinding.Command("kill_all"));
            GenerateDebugBinding(KeyCode.None, "SpawnAI Bind", "Binds the selected key to invoke \"spawn_ai\"", new DebugCommandBinding.Command("spawn_ai", "lemurian", "1"));
            spawnAIParameters = new ConfigurableString("lemurian,1")
            {
                Section = "Debug Toolkit Bindings",
                Description = "The parameters for the Spawn Monster Binding the parameters must be separated by \',\'",
                ConfigFile = debugConfig,
                InputFieldConfig = new InputFieldConfig
                {
                    checkIfDisabled = () => !enableCommandInvoking || !MSUtil.DebugToolkitInstalled
                }
            }.AddOnConfigChanged(s =>
            {
                DebugCommandBinding binding = bindings.FirstOrDefault(x => x.description == "Binds the selected key to invoke \"spawn_ai\"");
                if (binding != null)
                {
                    binding.tiedCommands = new DebugCommandBinding.Command[]
                    {
                        new DebugCommandBinding.Command("spawn_ai", s.Split(','))
                    };
                }
            });
            GenerateDebugBinding(KeyCode.None, "TeleportOnCursor Bind", "Binds the selected key to invoke \"teleport_on_cursor\"", new DebugCommandBinding.Command("teleport_on_cursor"));
            GenerateDebugBinding(KeyCode.None, "Respawn Bind", "Binds the selected key to invoke \"respawn\"", new DebugCommandBinding.Command("respawn", () => new string[] { MSUDebug.GetNetworkUser().ToString() }));
            GenerateDebugBinding(KeyCode.None, "RemoveAllItems Bind", "Binds the selected key to invoke \"remove_all_items\"", new DebugCommandBinding.Command("remove_all_items", () => new string[] { MSUDebug.GetNetworkUser().ToString() }));
            GenerateDebugBinding(KeyCode.None, "NoClip Bind", "Binds the selected key to invoke \"noclip\"", new DebugCommandBinding.Command("noclip", () => new string[] { MSUDebug.GetNetworkUser().ToString() }));

            printDebugEventMessage = new ConfigurableKeyBind(new KeyboardShortcut(KeyCode.KeypadMinus))
            {
                Section = "Events",
                Description = "Keybind used for printing a debug event message.",
                ConfigFile = debugConfig
            };

            addDummyEvent = new ConfigurableBool(false)
            {
                Section = "Events",
                Description = "Adds a Dummy event card that can be triggered manually on every stage. this event card does nothing in particular and its purely for testing purposes",
                Key = "Add Dummy Event",
                ConfigFile = debugConfig,
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

#if DEBUG
        private void DoNothing(On.RoR2.Networking.NetworkManagerSystem.orig_OnClientConnect orig, RoR2.Networking.NetworkManagerSystem self, NetworkConnection conn)
        {
        }

        private void GenerateDebugBinding(KeyCode defaultVal, string key, string description, params DebugCommandBinding.Command[] commands)
        {
            var configurableKeyBind = MakeConfigurableKeyBind(new KeyboardShortcut(defaultVal), c =>
            {
                c.Section = "Debug Toolkit Bindings";
                c.Description = description;
                c.ConfigFile = debugConfig;
                c.Key = key;
                c.KeyBindConfig = new KeyBindConfig
                {
                    checkIfDisabled = () => !enableDebugToolkitBindings || !MSUtil.DebugToolkitInstalled
                };
            });
            configurableKeyBind.DoConfigure();
            DebugCommandBinding binding = new DebugCommandBinding
            {
                description = description,
                tiedKeyBind = configurableKeyBind,
                tiedCommands = commands
            };
            bindings.Add(binding);
        }
#endif
    }
}
