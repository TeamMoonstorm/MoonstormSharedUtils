using HG;
using R2API;
using RoR2;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace MSU
{
    internal static class MSUContentManagement
    {
        private static Dictionary<UnityObjectWrapperKey<CharacterBody>, MSUContentBehaviour> bodyToContentBehaviour = new Dictionary<UnityObjectWrapperKey<CharacterBody>, MSUContentBehaviour>();

        private static Dictionary<BuffIndex, Type> _buffToBehaviour = new Dictionary<BuffIndex, Type>();
        private static Dictionary<UnityObjectWrapperKey<CharacterBody>, Dictionary<BuffIndex, BuffBehaviour>> _bodyToBuffBehaviourDictionary = new Dictionary<UnityObjectWrapperKey<CharacterBody>, Dictionary<BuffIndex, BuffBehaviour>>();
        [SystemInitializer(typeof(BodyCatalog), typeof(BuffCatalog))]
        private static void SystemInit()
        {
            On.RoR2.CharacterBody.SetBuffCount += SetBuffBehaviourCount;
            CharacterBody.onBodyAwakeGlobal += OnBodyAwakeGlobal;
            CharacterBody.onBodyDestroyGlobal += OnBodyDestroyedGlobal;

            if(MSUtil.HolyDLLInstalled)
            {
#if DEBUG
                MSULog.Info("Holy installed, using custom RecalculateStats support");
#endif
                On.RoR2.CharacterBody.RecalculateStats += Holy_RecalculateStats;
                RecalculateStatsAPI.GetStatCoefficients += Holy_GetStatsCoefficients;
            }
            else
            {
                On.RoR2.CharacterBody.RecalculateStats += RecalculateStats;
                RecalculateStatsAPI.GetStatCoefficients += GetStatsCoefficients;
            }

            InitMSUContentBehaviourSystem();
            InitBuffBehaviourSystem();
        }

        private static void GetStatsCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            bodyToContentBehaviour[sender].GetStatCoefficients(args);
        }

        private static void RecalculateStats(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self)
        {
            var behaviour = bodyToContentBehaviour[self];
            behaviour.RecalculateStatsStart();
            orig(self);
            behaviour.RecalculateStatsEnd();
        }

        private static void Holy_GetStatsCoefficients(CharacterBody body, RecalculateStatsAPI.StatHookEventArgs args)
        {
            var behaviour = bodyToContentBehaviour[body];
            behaviour.RecalculateStatsStart();
            behaviour.GetStatCoefficients(args);
        }

        private static void Holy_RecalculateStats(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self)
        {
            orig(self);
            bodyToContentBehaviour[self].RecalculateStatsEnd();
        }

        private static void InitMSUContentBehaviourSystem()
        {
            for(int i = 0; i < BodyCatalog.bodyPrefabs.Length; i++)
            {
                try
                {
                    GameObject bodyPrefab = BodyCatalog.bodyPrefabs[i];

                    var manager = bodyPrefab.AddComponent<MSUContentBehaviour>();
                    manager.body = bodyPrefab.GetComponent<CharacterBody>();

                    /*var modelLocator = bodyPrefab.GetComponent<ModelLocator>();
                    if (!modelLocator)
                        continue;

                    if (!modelLocator.modelTransform)
                        continue;

                    if (!modelLocator.modelTransform.TryGetComponent<CharacterModel>(out var model))
                        continue;

                    var eliteBehaviour = bodyPrefab.AddComponent<MSUEliteBehaviour>();
                    eliteBehaviour.characterModel = model;
                    eliteBehaviour.body = manager.body;
                    manager.eliteBehaviour = eliteBehaviour;*/

                    BodyCatalog.bodyPrefabs[i] = bodyPrefab;
                }
                catch(Exception e)
                {
                    MSULog.Error(e);
                }
            }
        }
        private static void InitBuffBehaviourSystem()
        {
            List<BuffBehaviour.BuffDefAssociation> attributes = new List<BuffBehaviour.BuffDefAssociation>();
            HG.Reflection.SearchableAttribute.GetInstances(attributes);

            Type buffBehaviourType = typeof(BuffBehaviour);
            Type buffDefType = typeof(BuffDef);
            foreach(BuffBehaviour.BuffDefAssociation attribute in attributes)
            {
                MethodInfo methodInfo = (MethodInfo)attribute.target;
                if (!methodInfo.IsStatic)
                    continue;

                var type = methodInfo.DeclaringType;
                if (!buffBehaviourType.IsAssignableFrom(type))
                    continue;

                if (type.IsAbstract)
                    continue;

                if (!buffDefType.IsAssignableFrom(methodInfo.ReturnType))
                    continue;

                if (methodInfo.GetGenericArguments().Length != 0)
                    continue;

                BuffDef buffDef = (BuffDef)methodInfo.Invoke(null, Array.Empty<object>());
                if (!buffDef)
                    continue;

                if (buffDef.buffIndex < 0)
                    continue;

                _buffToBehaviour.Add(buffDef.buffIndex, type);
            }
        }

        private static void SetBuffBehaviourCount(On.RoR2.CharacterBody.orig_SetBuffCount orig, CharacterBody self, BuffIndex buffType, int newCount)
        {
            orig(self, buffType, newCount);
            if (!_buffToBehaviour.ContainsKey(buffType))
                return;

            var bodyBuffBehaviours = _bodyToBuffBehaviourDictionary[self];
            if(!bodyBuffBehaviours.ContainsKey(buffType))
            {
                var newBehaviour = (BuffBehaviour)self.gameObject.AddComponent(_buffToBehaviour[buffType]);
                newBehaviour.BuffCount = newCount;
                newBehaviour.CharacterBody = self;
                bodyBuffBehaviours.Add(buffType, newBehaviour);
                var manager = bodyToContentBehaviour[self];
                manager.StartGetInterfaces();
                return;
            }
            bodyBuffBehaviours[buffType].BuffCount = newCount;
        }


        private static void OnBodyDestroyedGlobal(CharacterBody obj)
        {
            bodyToContentBehaviour.Remove(obj);
            _bodyToBuffBehaviourDictionary.Remove(obj);
        }

        private static void OnBodyAwakeGlobal(CharacterBody obj)
        {
            bodyToContentBehaviour.Add(obj, obj.GetComponent<MSUContentBehaviour>());
            _bodyToBuffBehaviourDictionary.Add(obj, new Dictionary<BuffIndex, BuffBehaviour>());
        }
    }

    public sealed class MSUContentBehaviour : MonoBehaviour
    {
        public bool HasMaster { get; private set; }
        public CharacterBody body;

        private EquipmentIndex _assignedEliteEquipmentIndex;
        private IStatItemBehavior[] _statItemBehaviors = Array.Empty<IStatItemBehavior>();
        private IBodyStatArgModifier[] _bodyStatArgModifiers = Array.Empty<IBodyStatArgModifier>();

        private void Start()
        {
            HasMaster = body.master;
            body.onInventoryChanged += CheckEquipments;
        }

        private void CheckEquipments()
        {
            StartGetInterfaces();
            if (!HasMaster)
                return;

            EquipmentDef def = EquipmentCatalog.GetEquipmentDef(body.inventory.GetEquipmentIndex());
            
            if (!def)
                return;

            if(EquipmentModule.AllMoonstormEquipments.TryGetValue(def, out IEquipmentContentPiece equipmentContent))
            {
                equipmentContent.OnEquipmentObtained(body);
                if (!def.passiveBuffDef)
                    return;

                if(_assignedEliteEquipmentIndex != def.passiveBuffDef?.eliteDef?.eliteEquipmentDef?.equipmentIndex)
                {
                    _assignedEliteEquipmentIndex = (EquipmentIndex)def.passiveBuffDef?.eliteDef?.eliteEquipmentDef?.equipmentIndex;
                    AssignElite(def.passiveBuffDef.eliteDef);
                }
            }
        }

        private void AssignElite(EliteDef eliteDef)
        {
            if(!(eliteDef is ExtendedEliteDef eed))
            {
                return;
            }


        }
        public void StartGetInterfaces() => StartCoroutine(GetInterfaces());

        private IEnumerator GetInterfaces()
        {
            yield return new WaitForEndOfFrame();
            _statItemBehaviors = GetComponents<IStatItemBehavior>();
            _bodyStatArgModifiers = GetComponents<IBodyStatArgModifier>();
            body.healthComponent.onIncomingDamageReceivers = GetComponents<IOnIncomingDamageServerReceiver>();
            body.healthComponent.onTakeDamageReceivers = GetComponents<IOnTakeDamageServerReceiver>();
        }

        public void GetStatCoefficients(RecalculateStatsAPI.StatHookEventArgs args)
        {
            for(int i = 0; i < _bodyStatArgModifiers.Length; i++)
            {
                _bodyStatArgModifiers[i].ModifyStatArguments(args);
            }
        }

        public void RecalculateStatsStart()
        {
            for(int i = 0; i < _statItemBehaviors.Length; i++)
            {
                _statItemBehaviors[i].RecalculateStatsStart();
            }
        }

        public void RecalculateStatsEnd()
        {
            for (int i = 0; i < _statItemBehaviors.Length; i++)
            {
                _statItemBehaviors[i].RecalculateStatsEnd();
            }
        }
    }
    public abstract class BuffBehaviour : MonoBehaviour
    {
        [AttributeUsage(AttributeTargets.Method)]
        public class BuffDefAssociation : HG.Reflection.SearchableAttribute
        {
        }
        public int BuffCount
        {
            get
            {
                return _buffCount;
            }
            internal set
            {
                if (_buffCount == value)
                    return;

                var previous = _buffCount;
                _buffCount = value;

                if(previous == 0 && _buffCount > 0)
                {
                    enabled = true;
                    OnFirstStackGained();
                }
                if(previous > 0 && _buffCount == 0)
                {
                    enabled = false;
                    OnAllStacksLost();
                }
            }
        }
        private int _buffCount;

        public CharacterBody CharacterBody { get; internal set; }

        public bool HasAnyStacks => _buffCount > 0;

        protected virtual void OnFirstStackGained() { }
        protected virtual void OnAllStacksLost() { }
    }
}