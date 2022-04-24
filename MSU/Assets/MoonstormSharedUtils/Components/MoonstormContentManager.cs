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
        public bool HasMaster { get; internal set; }

        public CharacterBody Body { get; internal set; }

        public MoonstormEliteBehavior EliteBehavior { get; internal set; }

        IStatItemBehavior[] statItemBehaviors = Array.Empty<IStatItemBehavior>();
        IBodyStatArgModifier[] bodyStatArgModifiers = Array.Empty<IBodyStatArgModifier>();

        private void Start()
        {
            Body.onInventoryChanged += CheckItemEquipments;
        }

        public void CheckItemEquipments()
        {
            if (!HasMaster)
                return;

            foreach(var equipment in EquipmentModuleBase.AllMoonstormEquipments)
            {
                if(Body.inventory.GetEquipmentIndex() == equipment.Key.equipmentIndex)
                {
                    //this is stupid
                    var bod = Body;
                    equipment.Value.AddBehavior(ref bod, 1);
                    break;
                }
            }

            if (EliteBehavior)
                CheckEliteBehavior();

            StartGetInterfaces();
        }

        public void StartGetInterfaces() => StartCoroutine(GetInterfaces());

        private IEnumerator GetInterfaces()
        {
            yield return new WaitForEndOfFrame();
            statItemBehaviors = GetComponents<IStatItemBehavior>();
            bodyStatArgModifiers = GetComponents<IBodyStatArgModifier>();
            Body.healthComponent.onIncomingDamageReceivers = GetComponents<IOnIncomingDamageServerReceiver>();
            Body.healthComponent.onTakeDamageReceivers = GetComponents<IOnTakeDamageServerReceiver>();
        }

        private void CheckEliteBehavior()
        {
            bool isElite = false;
            foreach(var eliteEqp in EquipmentModuleBase.EliteMoonstormEquipments)
            {
                if(Body.inventory.GetEquipmentIndex() == eliteEqp.Key.equipmentIndex)
                {
                    isElite = true;
                    break;
                }
            }
            if (!isElite)
                return;

            EliteBehavior.CharacterModel.UpdateOverlays();
            Body.RecalculateStats();
            foreach(var eliteDef in EliteModuleBase.MoonstormElites)
            {
                if(Body.isElite && EliteBehavior.CharacterModel.myEliteIndex == eliteDef.eliteIndex)
                {
                    EliteBehavior.SetNewElite(eliteDef);
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
            Body.onInventoryChanged -= CheckItemEquipments;
        }
    }
}
