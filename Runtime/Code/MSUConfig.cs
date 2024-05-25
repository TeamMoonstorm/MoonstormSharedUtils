using BepInEx;
using BepInEx.Configuration;
using MSU.Config;
using RiskOfOptions;
using RiskOfOptions.OptionConfigs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
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
        public static ConfigFactory ConfigFactory { get; private set; }

        /// <summary>
        /// The General Config File, it's identifier is <see cref="GENERAL"/>
        /// </summary>
        public static ConfigFile GeneralConfig { get; private set; }

#if DEBUG
        public static ConfigFile DebugConfig { get; private set; }

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
                Section = "Debugging",
                Description = "When enabled, MSU will log a StackTrace whenever it logs a Warning, Error or a Fatal.",
                ConfigFile = DebugConfig,
            };

            _enableSelfConnect = new ConfiguredBool(true)
            {
                Section = "Debugging",
                Description = "Allows you to connect to yourself using a second instance of the game, by hosting a private game with one and opening the console on the other and typing \"connect localhost:7777\"\nYou need to exit the lobby for this to take effect.",
                ConfigFile = DebugConfig,
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

            _enableCommandInvoking = new ConfiguredBool(true)
            {
                Section = "Command Invoking",
                Description = "Invokes commands at run start, disabling this causes no commands to be invoked at run start.",
                ConfigFile = DebugConfig
            };

            _invokeGod = new ConfiguredBool(true)
            {
                Section = "Command Invoking",
                Description = "Invokes the command \"god\"",
                ConfigFile = DebugConfig,
                CheckBoxConfig = new CheckBoxConfig
                {
                    checkIfDisabled = () => !_enableCommandInvoking
                }
            };

            _invokeStage1Pod = new ConfiguredBool(true)
            {
                Section = "Command Invoking",
                Description = "Invokes the command \"stage1_pod\" with the value 0",
                ConfigFile = DebugConfig,
                CheckBoxConfig = new CheckBoxConfig
                {
                    checkIfDisabled = () => !_enableCommandInvoking
                }
            };

            _invokeNoMonsters = new ConfiguredBool(true)
            {
                Section = "Command Invoking",
                Description = "Invokes the command \"no_monsters\"",
                ConfigFile = DebugConfig,
                CheckBoxConfig = new CheckBoxConfig
                {
                    checkIfDisabled = () => !_enableCommandInvoking
                }
            };

            _invoke100Dios = new ConfiguredBool(true)
            {
                Section = "Command Invoking",
                Description = "Invokes \"give_item\" with the params \"extralife 100\". Essentially gives 100 dios on run start",
                ConfigFile = DebugConfig,
                CheckBoxConfig = new CheckBoxConfig
                {
                    checkIfDisabled = () => !_enableCommandInvoking
                }
            };

            _enableDebugToolkitBindings = new ConfiguredBool(true)
            {
                Section = "Debug Toolkit Bindings",
                Description = "Enables the usage of bindings with DebugToolkit. Requires DebugToolkit to be installed.",
                ConfigFile = DebugConfig,
                CheckBoxConfig = new CheckBoxConfig
                {
                    checkIfDisabled = () => !MSUtil.DebugToolkitInstalled
                }
            };


            GenerateDebugBinding(KeyCode.None, "NextStage Bind", "Binds the selected key to invoke \"next_stage\"", new DebugCommandBinding.Command("next_stage"));

            GenerateDebugBinding(KeyCode.None, "KillAll Bind", "Binds the selected key to invoke \"kill_all\"", new DebugCommandBinding.Command("kill_all"));

            GenerateDebugBinding(KeyCode.None, "SpawnAI Bind", "Binds the selected key to invoke \"spawn_ai\"", new DebugCommandBinding.Command("spawn_ai", "lemurian", "1"));
            _spawnAIParameters = new ConfiguredString("lemurian,1")
            {
                Section = "Debug Toolkit Bindings",
                Description = "The parameters for the SpawnAI Binding, The parametersmust be separated by \",\"",
                ConfigFile = DebugConfig,
                InputFieldConfig = new InputFieldConfig
                {
                    checkIfDisabled = () => !_enableCommandInvoking || !MSUtil.DebugToolkitInstalled
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
            var configurableKeyBind = ConfigFactory.MakeConfiguredKeyBind(new KeyboardShortcut(defaultVal), c =>
            {
                c.Section = "Debug Toolkit Bindings";
                c.Description = description;
                c.ConfigFile = DebugConfig;
                c.Key = key;
                c.KeyBindConfig = new KeyBindConfig
                {
                    checkIfDisabled = () => !_enableDebugToolkitBindings || !MSUtil.DebugToolkitInstalled
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

        internal MSUConfig(BaseUnityPlugin bup)
        {
            ConfigFactory = new ConfigFactory(bup, true);
            GeneralConfig = ConfigFactory.CreateConfigFile(GENERAL, false);
            var icon = MSUMain.MSUAssetBundle.LoadAsset<Sprite>("icon");

#if DEBUG
            DebugConfig = ConfigFactory.CreateConfigFile(DEBUG, true);
            ModSettingsManager.SetModIcon(icon, bup.Info.Metadata.GUID + "." + DEBUG, bup.Info.Metadata.Name + "." + DEBUG);
            ModSettingsManager.SetModDescription("The debug configuration of Moonstorm Shared Utils", bup.Info.Metadata.GUID + "." + DEBUG, bup.Info.Metadata.Name + "." + DEBUG);
            SetDebugConfigs();
#endif
        }
    }
}