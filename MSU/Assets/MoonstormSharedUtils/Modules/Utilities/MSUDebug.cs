using Moonstorm.Components;
using RoR2;
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
            #region No Enemies
            //These just make testing faster
            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("iHarbHD.DebugToolkit"))
            {
                Run.onRunStartGlobal += (connection) =>
                {
                    DebugToolkit.DebugToolkit.InvokeCMD(NetworkUser.instancesList[0], "stage1_pod", new string[] { "0" });
                    DebugToolkit.DebugToolkit.InvokeCMD(NetworkUser.instancesList[0], "no_enemies", new string[] { });
                };
            }
            #endregion
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
        }
    }
}
