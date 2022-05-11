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
    public class MoonstormContentManager : MonoBehaviour
    {
        public bool hasMaster;

        public CharacterBody body;

        public MoonstormEliteBehavior eliteBehavior;

        IStatItemBehavior[] statItemBehaviors = Array.Empty<IStatItemBehavior>();
        IBodyStatArgModifier[] bodyStatArgModifiers = Array.Empty<IBodyStatArgModifier>();

        private void Start()
        {
            body.onInventoryChanged += CheckItemEquipments;
        }

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

        public void RunStatRecalculationsStart()
        {
            foreach (var statBehavior in statItemBehaviors)
                statBehavior.RecalculateStatsStart();
        }

        public void RunStatRecalculationsEnd()
        {
            foreach (var statBehavior in statItemBehaviors)
                statBehavior.RecalculateStatsEnd();
        }

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
