using Moonstorm.Components;
using RoR2;
using RoR2.UI;
using System;
using System.Linq;
using UnityEngine;

namespace Moonstorm.Utilities
{
    internal class MSUDebug : MonoBehaviour
    {
        private void Awake()
        {
            #region networking
            //you can connect to yourself with a second instance of the game by hosting a private game with one and opening the console on the other and typing connect localhost:7777
            On.RoR2.Networking.NetworkManagerSystem.OnClientConnect += (self, user, t) => { };
            #endregion networking
        }

        private void Start()
        {
            Run.onRunStartGlobal += OnRunStart;
            #region Item display helper adder
            //Adds the item display helper to all the character bodies.
            RoR2Application.onLoad += () =>
            {
                foreach(GameObject prefab in BodyCatalog.allBodyPrefabs)
                {
                    try
                    {
                        var modelLocator = gameObject.GetComponent<ModelLocator>();
                        if (!modelLocator)
                            continue;

                        var mdlPrefab = modelLocator.modelTransform.gameObject;
                        if (!mdlPrefab)
                            continue;

                        var charModel = mdlPrefab.GetComponent<CharacterModel>();
                        if (!charModel)
                            continue;

                        if (charModel.itemDisplayRuleSet == null)
                            continue;

                        mdlPrefab.EnsureComponent<MoonstormIDH>();
                    }
                    catch(Exception e) { MSULog.Error(e); }
                }
            };
            #endregion
        }

        private void OnRunStart(Run obj)
        {
            #region Command Invoking
            if (MSUtil.IsModInstalled("iHarbHD.DebugToolkit"))
            {
                InvokeCommand("stage1_pod", "0");
                InvokeCommand("no_enemies");
                InvokeCommand("enable_event_logging", "1");
            }
            #endregion
        }

        private void InvokeCommand(string commandName, params string[] arguments) => DebugToolkit.DebugToolkit.InvokeCMD(NetworkUser.instancesList[0], commandName, arguments);

        private void Update()
        {
            var input0 = Input.GetKeyDown(MSUConfig.InstantiateMaterialTester.Value);
            //add more if necessary
            #region materialTester
            if (input0 && Run.instance)
            {
                var position = Vector3.zero;
                var quaternion = Quaternion.identity;
                var inputBank = PlayerCharacterMasterController.instances[0].master.GetBodyObject().GetComponent<InputBankTest>();
                position = inputBank.aimOrigin + inputBank.aimDirection * 5;
                quaternion = Quaternion.LookRotation(inputBank.GetAimRay().direction, Vector3.up);
                var materialTester = MoonstormSharedUtils.MSUAssetBundle.LoadAsset<GameObject>("MaterialTester");
                Instantiate(materialTester, position, quaternion);
            }
            #endregion
            /*var input1 = Input.GetKeyDown(KeyCode.KeypadMinus);
            if(input1)
            {
                var go = EventHelpers.AnnounceEvent(new EventHelpers.EventAnnounceInfo(MoonstormSharedUtils.MSUAssetBundle.LoadAsset<EventCard>("TestEventState"), 15, true) { fadeOnStart = false });
                go.GetComponent<HGTextMeshProUGUI>().alpha = 1f;
            }*/
        }
    }
}
