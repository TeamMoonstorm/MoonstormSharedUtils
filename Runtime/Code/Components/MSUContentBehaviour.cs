using R2API;
using RoR2;
using System;
using System.Collections;
using UnityEngine;

namespace MSU
{
    /// <summary>
    /// Base MonoBehaviour of MSU, this MonoBehaviour is used to manage a plethora of Behaviour related interfaces for a Body.
    /// </summary>
    public sealed class MSUContentBehaviour : MonoBehaviour
    {
        /// <summary>
        /// Wether or not the body tied to this ContentBehaviour has a CharacterMaster
        /// </summary>
        public bool hasMaster { get; private set; }
        /// <summary>
        /// The body attached to this ContentBehaviour
        /// </summary>
        public CharacterBody body;
        /// <summary>
        /// The ContentBehaviour's Elite Behaviour, see also <see cref="MSUEliteBehaviour"/>
        /// </summary>
        public MSUEliteBehaviour eliteBehaviour;

        private IStatItemBehavior[] _statItemBehaviors = Array.Empty<IStatItemBehavior>();
        private IBodyStatArgModifier[] _bodyStatArgModifiers = Array.Empty<IBodyStatArgModifier>();

        private void Start()
        {
            hasMaster = body.master;
            body.onInventoryChanged += CheckEquipments;

            //This is done to ensure whatever "OnEquipmentObtained" logic runs when the body starts. since OnEquipmentObtained only gets called when the inventory changes, which doesnt happen at this time.
            var eqpDef = EquipmentCatalog.GetEquipmentDef(body.inventory ? body.inventory.GetEquipmentIndex() : EquipmentIndex.None);
            if (eqpDef && EquipmentModule.allMoonstormEquipments.TryGetValue(eqpDef, out var iEquipmentContentPiece))
            {
                iEquipmentContentPiece.OnEquipmentObtained(body);
            }
            StartGetInterfaces();
        }

        private void CheckEquipments()
        {
            StartGetInterfaces();
            if (!hasMaster)
                return;

            EquipmentDef def = EquipmentCatalog.GetEquipmentDef(body.inventory.GetEquipmentIndex());

            if (def && eliteBehaviour)
                CheckEliteBehaviour(def);
        }

        private void CheckEliteBehaviour(EquipmentDef def)
        {
            //Try removing elite qualities if thee incomming def isnt an Elite Equipment.
            if (!def.passiveBuffDef || !def.passiveBuffDef.isElite)
            {
                eliteBehaviour.AssignNewElite(EliteIndex.None);
                return;
            }
            eliteBehaviour.AssignNewElite(def.passiveBuffDef.eliteDef.eliteIndex);
        }

        /// <summary>
        /// Starts a Coroutine used for updating the interfaces that the body has
        /// </summary>
        public void StartGetInterfaces() => StartCoroutine(GetInterfaces());

        private IEnumerator GetInterfaces()
        {
            yield return new WaitForEndOfFrame();
            _statItemBehaviors = GetComponents<IStatItemBehavior>();
            _bodyStatArgModifiers = GetComponents<IBodyStatArgModifier>();
            body.healthComponent.onIncomingDamageReceivers = GetComponents<IOnIncomingDamageServerReceiver>();
            body.healthComponent.onTakeDamageReceivers = GetComponents<IOnTakeDamageServerReceiver>();
        }

        internal void GetStatCoefficients(RecalculateStatsAPI.StatHookEventArgs args)
        {
            for (int i = 0; i < _bodyStatArgModifiers.Length; i++)
            {
                _bodyStatArgModifiers[i].ModifyStatArguments(args);
            }
        }

        internal void RecalculateStatsStart()
        {
            for (int i = 0; i < _statItemBehaviors.Length; i++)
            {
                _statItemBehaviors[i].RecalculateStatsStart();
            }
        }

        internal void RecalculateStatsEnd()
        {
            for (int i = 0; i < _statItemBehaviors.Length; i++)
            {
                _statItemBehaviors[i].RecalculateStatsEnd();
            }
        }
    }
}
