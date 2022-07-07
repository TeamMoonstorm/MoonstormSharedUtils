using RoR2;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Moonstorm.Components
{
    /// <summary>
    /// The MoonstormContentManager manages the correct implementation of MSU's custom Behaviour interfaces
    /// </summary>
    public class MoonstormContentManager : MonoBehaviour
    {
        [Tooltip("Wether the current body tied to this content manager has a master, masterless bodies dont get inventory updates")]
        public bool hasMaster;

        [Tooltip("The body for this ContentManager")]
        public CharacterBody body;

        [Tooltip("The EliteBehaviour for this ContentManager")]
        public MoonstormEliteBehavior eliteBehavior;

        /// <summary>
        /// Contains all the current StatItemBehaviours for this body
        /// </summary>
        IStatItemBehavior[] statItemBehaviors = Array.Empty<IStatItemBehavior>();
        /// <summary>
        /// Contains all the BodyStatArgModifiers for this body
        /// </summary>
        IBodyStatArgModifier[] bodyStatArgModifiers = Array.Empty<IBodyStatArgModifier>();

        private void Start()
        {
            body.onInventoryChanged += CheckItemEquipments;
        }

        /// <summary>
        /// Checks and updates the EliteBehaviour and EquipmentBehaviour for this body.
        /// <para>Also updates interfaces</para>
        /// </summary>
        public void CheckItemEquipments()
        {
            if (!hasMaster)
                return;

            foreach(var equipment in EquipmentModuleBase.AllMoonstormEquipments)
            {
                if(body.inventory.GetEquipmentIndex() == equipment.Key.equipmentIndex)
                {
                    //this is stupid
                    var bod = body;
                    equipment.Value.AddBehavior(ref bod, 1);
                    break;
                }
            }

            if (eliteBehavior)
                CheckEliteBehavior();

            StartGetInterfaces();
        }

        /// <summary>
        /// When called, the next frame will be used for updating all interfaces of this body
        /// </summary>
        public void StartGetInterfaces() => StartCoroutine(GetInterfaces());

        private IEnumerator GetInterfaces()
        {
            yield return new WaitForEndOfFrame();
            statItemBehaviors = GetComponents<IStatItemBehavior>();
            bodyStatArgModifiers = GetComponents<IBodyStatArgModifier>();
            body.healthComponent.onIncomingDamageReceivers = GetComponents<IOnIncomingDamageServerReceiver>();
            body.healthComponent.onTakeDamageReceivers = GetComponents<IOnTakeDamageServerReceiver>();
        }

        private void CheckEliteBehavior()
        {
            bool isElite = false;
            foreach(var eliteEqp in EquipmentModuleBase.EliteMoonstormEquipments)
            {
                if(body.inventory.GetEquipmentIndex() == eliteEqp.Key.equipmentIndex)
                {
                    isElite = true;
                    break;
                }
            }
            if (!isElite)
                return;

            eliteBehavior.characterModel.UpdateOverlays();
            body.RecalculateStats();
            foreach(var eliteDef in EliteModuleBase.MoonstormElites)
            {
                if(body.isElite && eliteBehavior.characterModel.myEliteIndex == eliteDef.eliteIndex)
                {
                    eliteBehavior.SetNewElite(eliteDef);
                }
            }
        }

        /// <summary>
        /// Runs <see cref="IStatItemBehavior.RecalculateStatsStart"/>
        /// </summary>
        public void RunStatRecalculationsStart()
        {
            foreach (var statBehavior in statItemBehaviors)
                statBehavior.RecalculateStatsStart();
        }

        /// <summary>
        /// Runs <see cref="IStatItemBehavior.RecalculateStatsEnd"/>
        /// </summary>
        public void RunStatRecalculationsEnd()
        {
            foreach (var statBehavior in statItemBehaviors)
                statBehavior.RecalculateStatsEnd();
        }

        /// <summary>
        /// Runs <see cref="IBodyStatArgModifier.ModifyStatArguments(R2API.RecalculateStatsAPI.StatHookEventArgs)"/>
        /// </summary>
        public void RunStatHookEventModifiers(R2API.RecalculateStatsAPI.StatHookEventArgs args)
        {
            foreach(var statModifier in bodyStatArgModifiers)
            {
                statModifier.ModifyStatArguments(args);
            }
        }
        private void OnDestroy()
        {
            body.onInventoryChanged -= CheckItemEquipments;
        }
    }
}
