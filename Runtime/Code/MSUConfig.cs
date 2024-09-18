using BepInEx;
using BepInEx.Configuration;
using MSU.Config;
using RiskOfOptions;
using RiskOfOptions.OptionConfigs;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using AutoConfig = MSU.Config.ConfiguredVariable.AutoConfigAttribute;

namespace MSU
{
    /// <summary>
    /// The config class for MSU
    /// </summary>
    public class MSUConfig
    {
        /// <summary>
        /// Identifier for MSU's General config file
        /// </summary>
        public const string GENERAL = "MSU.General";

#if DEBUG
        public const string DEBUG = "MSU.Debug";
#endif
        /// <summary>
        /// MSUConfig's ConfigFactory
        /// </summary>
        public static ConfigFactory configFactory { get; private set; }

        /// <summary>
        /// The General Config File, it's identifier is <see cref="GENERAL"/>
        /// </summary>
        public static ConfigFile generalConfig { get; private set; }

        [AutoConfig]
        internal static ConfiguredFloat _maxOpacityForEventMessage;
        [AutoConfig]
        internal static ConfiguredFloat _eventMessageFontSize;
        [AutoConfig]
        internal static ConfiguredFloat _eventMessageYOffset;
        [AutoConfig]
        internal static ConfiguredFloat _eventMessageXOffset;
        [AutoConfig]
        internal static ConfiguredBool _familyEventUsesEventAnnouncementInsteadOfChatMessage;

#if DEBUG
        public static ConfigFile debugConfig { get; private set; }

        internal static ConfiguredBool _enableStackLogging;
        [AutoConfig]
        internal static ConfiguredBool _enableSelfConnect;
        [AutoConfig]
        internal static ConfiguredBool _enableCommandInvoking;
        [AutoConfig]
        internal static ConfiguredBool _invokeGod;
        [AutoConfig]
        internal static ConfiguredBool _invokeStage1Pod;
        [AutoConfig]
        internal static ConfiguredBool _invokeNoMonsters;
        [AutoConfig]
        internal static ConfiguredBool _invoke100Dios;
        [AutoConfig]
        internal static ConfiguredBool _enableDebugToolkitBindings;
        [AutoConfig]
        internal static ConfiguredString _spawnAIParameters;
        internal static HashSet<DebugCommandBinding> _bindings = new HashSet<DebugCommandBinding>();

        private void SetDebugConfigs()
        {
            _enableStackLogging = new ConfiguredBool(false)
            {
                section = "Debugging",
                description = "When enabled, MSU will log a StackTrace whenever it logs a Warning, Error or a Fatal.",
                configFile = debugConfig,
            };

            _enableSelfConnect = new ConfiguredBool(false)
            {
                section = "Debugging",
                description = "Allows you to connect to yourself using a second instance of the game, by hosting a private game with one and opening the console on the other and typing \"connect localhost:7777\"\nYou need to exit the lobby for this to take effect.",
                configFile = debugConfig,
            }.WithConfigChange((b) =>
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

            _enableCommandInvoking = new ConfiguredBool(false)
            {
                section = "Command Invoking",
                description = "Invokes commands at run start, disabling this causes no commands to be invoked at run start.",
                configFile = debugConfig
            };

            _invokeGod = new ConfiguredBool(true)
            {
                section = "Command Invoking",
                description = "Invokes the command \"god\"",
                configFile = debugConfig,
                checkBoxConfig = new CheckBoxConfig
                {
                    checkIfDisabled = () => !_enableCommandInvoking
                }
            };

            _invokeStage1Pod = new ConfiguredBool(true)
            {
                section = "Command Invoking",
                description = "Invokes the command \"stage1_pod\" with the value 0",
                configFile = debugConfig,
                checkBoxConfig = new CheckBoxConfig
                {
                    checkIfDisabled = () => !_enableCommandInvoking
                }
            };

            _invokeNoMonsters = new ConfiguredBool(true)
            {
                section = "Command Invoking",
                description = "Invokes the command \"no_monsters\"",
                configFile = debugConfig,
                checkBoxConfig = new CheckBoxConfig
                {
                    checkIfDisabled = () => !_enableCommandInvoking
                }
            };

            _invoke100Dios = new ConfiguredBool(true)
            {
                section = "Command Invoking",
                description = "Invokes \"give_item\" with the params \"extralife 100\". Essentially gives 100 dios on run start",
                configFile = debugConfig,
                checkBoxConfig = new CheckBoxConfig
                {
                    checkIfDisabled = () => !_enableCommandInvoking
                }
            };

            _enableDebugToolkitBindings = new ConfiguredBool(true)
            {
                section = "Debug Toolkit Bindings",
                description = "Enables the usage of bindings with DebugToolkit. Requires DebugToolkit to be installed.",
                configFile = debugConfig,
                checkBoxConfig = new CheckBoxConfig
                {
                    checkIfDisabled = () => !MSUtil.debugToolkitInstalled
                }
            };


            GenerateDebugBinding(KeyCode.None, "NextStage Bind", "Binds the selected key to invoke \"next_stage\"", new DebugCommandBinding.Command("next_stage"));

            GenerateDebugBinding(KeyCode.None, "KillAll Bind", "Binds the selected key to invoke \"kill_all\"", new DebugCommandBinding.Command("kill_all"));

            GenerateDebugBinding(KeyCode.None, "SpawnAI Bind", "Binds the selected key to invoke \"spawn_ai\"", new DebugCommandBinding.Command("spawn_ai", "lemurian", "1"));
            _spawnAIParameters = new ConfiguredString("lemurian,1")
            {
                section = "Debug Toolkit Bindings",
                description = "The parameters for the SpawnAI Binding, The parametersmust be separated by \",\"",
                configFile = debugConfig,
                inputFieldConfig = new InputFieldConfig
                {
                    checkIfDisabled = () => !_enableCommandInvoking || !MSUtil.debugToolkitInstalled
                }
            }.WithConfigChange(s =>
            {
                DebugCommandBinding binding = _bindings.FirstOrDefault(x => x.description == "Binds the selected key to invoke \"spawn_ai\"");
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
        }

        private void DoNothing(On.RoR2.Networking.NetworkManagerSystem.orig_OnClientConnect orig, RoR2.Networking.NetworkManagerSystem self, UnityEngine.Networking.NetworkConnection conn)
        {
            //Intentionally do nothing
        }

        private void GenerateDebugBinding(KeyCode defaultVal, string key, string description, params DebugCommandBinding.Command[] commands)
        {
            var configurableKeyBind = configFactory.MakeConfiguredKeyBind(new KeyboardShortcut(defaultVal), c =>
            {
                c.section = "Debug Toolkit Bindings";
                c.description = description;
                c.configFile = debugConfig;
                c.key = key;
                c.keyBindConfig = new KeyBindConfig
                {
                    checkIfDisabled = () => !_enableDebugToolkitBindings || !MSUtil.debugToolkitInstalled
                };
            });
            configurableKeyBind.DoConfigure();
            DebugCommandBinding binding = new DebugCommandBinding
            {
                description = description,
                tiedKeyBind = configurableKeyBind,
                tiedCommands = commands
            };
            _bindings.Add(binding);
        }
#endif

        private void SetGeneralConfigs()
        {
            _maxOpacityForEventMessage = new ConfiguredFloat(0.75f)
            {
                section = "Gameplay Event Messages",
                description = "The Maximum opacity for the Gameplay Event Messages.",
                configFile = generalConfig,
                sliderType = ConfiguredFloat.SliderTypeEnum.Normal,
                sliderConfig = new SliderConfig
                {
                    min = 0f,
                    max = 1f,
                    FormatString = "{0:0.0%}",
                }
            };

            _eventMessageFontSize = new ConfiguredFloat(61f)
            {
                section = "Gameplay Event Messages",
                description = "The Size of the font used in the Gameplay Event Message.",
                configFile = generalConfig,
                sliderType = ConfiguredFloat.SliderTypeEnum.Normal,
                sliderConfig = new SliderConfig
                {
                    min = 0f,
                    max = 100f,

                    FormatString = "{0:0.0}",
                }
            };

            _eventMessageYOffset = new ConfiguredFloat(150f)
            {
                section = "Gameplay Event Messages",
                description = "The Y Offset for the Gameplay Event Message.",
                configFile = generalConfig,
                sliderType = ConfiguredFloat.SliderTypeEnum.Normal,
                sliderConfig = new SliderConfig
                {
                    min = 0f,
                    max = 3456,
                    FormatString = "{0:0.0}",
                }
            };

            _eventMessageXOffset = new ConfiguredFloat(0f)
            {
                section = "Gameplay Event Messages",
                description = "The X Offset for the Gameplay Event Message.",
                configFile = generalConfig,
                sliderType = ConfiguredFloat.SliderTypeEnum.Normal,
                sliderConfig = new SliderConfig
                {
                    min = -4096f,
                    max = 4096,
                    FormatString = "{0:0.0}",
                }
            };

            _familyEventUsesEventAnnouncementInsteadOfChatMessage = new ConfiguredBool(true)
            {
                section = "Gameplay Event Messages",
                description = "Setting this to True causes the family event chat message to display as a Gameplay Event announcement instead.",
                configFile = generalConfig,
            };
        }

        internal MSUConfig(BaseUnityPlugin bup)
        {
            configFactory = new ConfigFactory(bup, true);
            generalConfig = configFactory.CreateConfigFile(GENERAL, false);
            var icon = MSUMain.msuAssetBundle.LoadAsset<Sprite>("icon");

            ModSettingsManager.SetModIcon(icon, bup.Info.Metadata.GUID, bup.Info.Metadata.Name);
            ModSettingsManager.SetModDescription("An API focused with the intention of working in an editor enviroment using ThunderKit, MSU is a modular API system designed for ease of use and simplicity.", bup.Info.Metadata.GUID, bup.Info.Metadata.Name);
            SetGeneralConfigs();
#if DEBUG
            debugConfig = configFactory.CreateConfigFile(DEBUG, true);
            ModSettingsManager.SetModIcon(icon, bup.Info.Metadata.GUID + "." + DEBUG, bup.Info.Metadata.Name + "." + DEBUG);
            ModSettingsManager.SetModDescription("The debug configuration of Moonstorm Shared Utils", bup.Info.Metadata.GUID + "." + DEBUG, bup.Info.Metadata.Name + "." + DEBUG);
            SetDebugConfigs();
#endif
        }
    }
}