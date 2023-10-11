#if DEBUG
using Moonstorm.Components;
using Moonstorm.Config;
using RoR2;
using RoR2.UI;
using System;
using System.Linq;
using UnityEngine;

namespace Moonstorm
{
    internal class DebugCommandBinding
    {
        public class Command
        {
            public string commandName;
            public string[] Parameters => parameters == null ? generateParams() : parameters;
            private string[] parameters;
            public Func<string[]> generateParams;
            public Command(string commandName, params string[] parameters)
            {
                this.commandName = commandName;
                this.parameters = parameters;
                generateParams = null;
            }
            public Command(string commandName, Func<string[]> generateParams)
            {
                this.commandName = commandName;
                parameters = null;
                this.generateParams = generateParams;
            }
        }
        public string description;
        public ConfigurableKeyBind tiedKeyBind;
        public Command[] tiedCommands;
    }

    internal class MSUDebug : MonoBehaviour
    {
        private void Awake()
        {
            #region Item display helper adder
            //Adds the item display helper to all the character bodies.
            RoR2Application.onLoad += () =>
            {
                foreach (GameObject prefab in BodyCatalog.allBodyPrefabs)
                {
                    try
                    {
                        var charModel = prefab.GetComponentInChildren<CharacterModel>();
                        if (charModel == null)
                            continue;

                        if (charModel.itemDisplayRuleSet == null)
                            continue;

                        charModel.gameObject.EnsureComponent<MoonstormIDH>();
                    }
                    catch (Exception e) { MSULog.Error(e); }
                }
            };
            #endregion
        }

        private void Start()
        {
            Run.onRunStartGlobal += OnRunStart;
        }

        private void OnRunStart(Run obj)
        {
            #region Command Invoking
            if (!MSUConfig.enableCommandInvoking)
                return;

            if (MSUConfig.invokeGod)
                MSUtil.InvokeCommand("god", "1");
            if (MSUConfig.invokeStage1Pod)
                MSUtil.InvokeCommand("stage1_pod", "0");
            if (MSUConfig.invokeNoMonsters)
                MSUtil.InvokeCommand("no_enemies");
            if (MSUConfig.invokeMSEnableEventLogging)
                MSUtil.InvokeCommand("ms_enable_event_logging", "1");
            if (MSUConfig.invoke100Dios)
                MSUtil.InvokeCommand("give_item", "extralife", "100", GetNetworkUser().ToString());
            #endregion
        }

        public static object GetNetworkUser()
        {
            var networkUser = NetworkUser.instancesList.FirstOrDefault();
            if (networkUser)
            {
                if (MSUConfig.enableSelfConnect)
                {
                    return NetworkUser.instancesList.IndexOf(networkUser);
                }
                return networkUser.userName;
            }
            return string.Empty;
        }

        private void Update()
        {
            var input0 = MSUConfig.printDebugEventMessage.IsDown;
            if (input0)
            {
                var go = EventHelpers.AnnounceEvent(new EventHelpers.EventAnnounceInfo(MoonstormSharedUtils.MSUAssetBundle.LoadAsset<EventCard>("DummyEventCard"), 15, true) { fadeOnStart = false });
                go.GetComponent<HGTextMeshProUGUI>().alpha = 1f;
            }
            if (!MSUConfig.enableDebugToolkitBindings)
                return;

            foreach (var binding in MSUConfig.bindings)
            {
                if (binding.tiedKeyBind.IsDown)
                {
                    foreach (var command in binding.tiedCommands)
                    {
                        MSUtil.InvokeCommand(command.commandName, command.Parameters);
                    }
                }
            }
        }
    }
}
#endif