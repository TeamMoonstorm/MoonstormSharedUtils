#if DEBUG
using MSU.Config;
using RoR2;
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace MSU
{
    internal class DebugCommandBinding
    {
        public string description;
        public ConfiguredKeyBind tiedKeyBind;
        public Command[] tiedCommands;

        public class Command
        {
            public string commandName;
            public string[] parameters => _parameters == null ? generateParameters() : _parameters;
            private string[] _parameters;
            public Func<string[]> generateParameters;

            public Command(string commandName, params string[] parameters)
            {
                this.commandName = commandName;
                this._parameters = parameters;
                generateParameters = null;
            }
            public Command(string commandName, Func<string[]> generateParams)
            {
                this.commandName = commandName;
                _parameters = null;
                this.generateParameters = generateParams;
            }
        }
    }

    internal class MSUDebug : MonoBehaviour
    {
        private FieldInfo _noEnemiesField;

        private void Awake()
        {
            RoR2Application.onLoad += () =>
            {
                foreach (GameObject prefab in BodyCatalog.allBodyPrefabs)
                {
                    try
                    {
                        var charModel = prefab.GetComponentInChildren<CharacterModel>();

                        if (!charModel)
                            continue;

                        if (!charModel.itemDisplayRuleSet)
                            continue;

                        charModel.gameObject.EnsureComponent<ItemDisplayHelper>();
                    }
                    catch (Exception e)
                    {
                        MSULog.Error(e);
                    }
                }
            };
            if (MSUtil.debugToolkitInstalled)
                GetNoEnemiesField();
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private void GetNoEnemiesField()
        {
            Type t = Type.GetType("DebugToolkit.Commands.CurrentRun, DebugToolkit");
            if (t != null)
            {
                _noEnemiesField = t.GetField("noEnemies", BindingFlags.NonPublic | BindingFlags.Static);
            }
        }

        private void Start()
        {
            Run.onRunStartGlobal += InvokeCommands;
        }

        private void InvokeCommands(Run obj)
        {
            if (!MSUConfig._enableCommandInvoking)
                return;

            if (MSUConfig._invokeGod)
                MSUtil.InvokeCommand("god", "1");
            if (MSUConfig._invokeStage1Pod)
                MSUtil.InvokeCommand("stage1_pod", "0");
            if (MSUConfig._invokeNoMonsters)
            {
                if (_noEnemiesField != null)
                {
                    bool noEnemiesIsTrue = (bool)_noEnemiesField.GetValue(null);
                    if (!noEnemiesIsTrue)
                    {
                        MSUtil.InvokeCommand("no_enemies", "1");
                    }
                }
                else
                {
                    MSUtil.InvokeCommand("no_enemies", "1");
                }
            }
            if (MSUConfig._invoke100Dios)
                MSUtil.InvokeCommand("give_item", "extralife", "100", GetNetworkUser().ToString());
        }


        public static object GetNetworkUser()
        {
            var networkUser = NetworkUser.instancesList.FirstOrDefault();
            if (networkUser)
            {
                if (MSUConfig._enableSelfConnect)
                {
                    return NetworkUser.instancesList.IndexOf(networkUser);
                }
                return networkUser.userName;
            }
            return string.Empty;
        }

        private void Update()
        {
            if (!MSUConfig._enableDebugToolkitBindings)
                return;

            foreach (var binding in MSUConfig._bindings)
            {
                if (binding.tiedKeyBind.isDown)
                {
                    foreach (var command in binding.tiedCommands)
                    {
                        MSUtil.InvokeCommand(command.commandName, command.parameters);
                    }
                }
            }
        }
    }
}
#endif