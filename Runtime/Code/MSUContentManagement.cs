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
        private static Dictionary<CharacterBody, MSUContentBehaviour> bodyToContentBehaviour = new Dictionary<CharacterBody, MSUContentBehaviour>();
        private static Dictionary<CharacterBody, Dictionary<BuffIndex, BuffBehaviour>> bodyToBuffBehaviours = new Dictionary<CharacterBody, Dictionary<BuffIndex, BuffBehaviour>>();
        private static Dictionary<CharacterBody, GameObject> bodyToBuffHolder = new Dictionary<CharacterBody, GameObject>();
        private static Dictionary<BuffIndex, Type> buffToBehaviourType = new Dictionary<BuffIndex, Type>();
        [SystemInitializer(typeof(BodyCatalog), typeof(BuffCatalog))]
        private static void SystemInit()
        {
            On.RoR2.CharacterBody.Awake += AddToDictionary;
            On.RoR2.CharacterBody.OnDestroy += RemoveFromDictionary;
            On.RoR2.CharacterBody.SetBuffCount += SetBuffBehaviourCount;

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

                if (buffDefType.IsAssignableFrom(methodInfo.ReturnType))
                    continue;

                if (methodInfo.GetGenericArguments().Length != 0)
                    continue;

                BuffDef buffDef = (BuffDef)methodInfo.Invoke(null, Array.Empty<object>());
                if (!buffDef)
                    continue;

                if (buffDef.buffIndex < 0)
                    continue;

                buffToBehaviourType.Add(buffDef.buffIndex, type);
            }
        }

        private static void AddToDictionary(On.RoR2.CharacterBody.orig_Awake orig, CharacterBody self)
        {
            orig(self);
            var buffHolder = new GameObject("BuffHolder");
            buffHolder.transform.SetParent(self.transform);

            bodyToContentBehaviour.Add(self, self.GetComponent<MSUContentBehaviour>());
            bodyToBuffHolder.Add(self, buffHolder);
            bodyToBuffBehaviours.Add(self, new Dictionary<BuffIndex, BuffBehaviour>());
        }
        private static void RemoveFromDictionary(On.RoR2.CharacterBody.orig_OnDestroy orig, CharacterBody self)
        {
            bodyToContentBehaviour.Remove(self);
            bodyToBuffBehaviours.Remove(self);
            bodyToBuffHolder.Remove(self);
            orig(self);
        }

        private static void SetBuffBehaviourCount(On.RoR2.CharacterBody.orig_SetBuffCount orig, CharacterBody self, BuffIndex buffType, int newCount)
        {
            if(!buffToBehaviourType.ContainsKey(buffType))
            {
                return;
            }

            var buffHolder = bodyToBuffHolder[self];
            var bodyBuffBehaviours = bodyToBuffBehaviours[self];
            if(!bodyBuffBehaviours.ContainsKey(buffType))
            {
                var newBehaviour = (BuffBehaviour)buffHolder.AddComponent(buffToBehaviourType[buffType]);
                newBehaviour.BuffCount = newCount;
                newBehaviour.CharacterBody = self;
                var manager = self.GetComponent<MSUContentBehaviour>();
                if (manager)
                {
                    manager.StartGetInterfaces();
                }
                return;
            }
            bodyBuffBehaviours[buffType].BuffCount = newCount;
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
        public abstract BuffDef TiedBuffDef { get; }
        public int BuffCount
        {
            get
            {
                return _buffCount;
            }
            internal set
            {
                _buffCount = value;
                if(_buffCount <= 0)
                {
                    base.enabled = false;
                }
            }
        }
        private int _buffCount;

        public CharacterBody CharacterBody { get; internal set; }

        public new bool enabled => BuffCount > 0;

        public new Component GetComponent(string type) => CharacterBody.GetComponent(type);
        public new Component GetComponent(Type type) => CharacterBody.GetComponent(type);
        public new T GetComponent<T>() => CharacterBody.GetComponent<T>();
        public new T GetComponentInChildren<T>() => CharacterBody.GetComponentInChildren<T>();
        public new T GetComponentInChildren<T>(bool includeInactive) => CharacterBody.GetComponentInChildren<T>(includeInactive);
        public new Component GetComponentInChildren(Type type) => CharacterBody.GetComponentInChildren(type);
        public new Component GetComponentInChildren(Type type, bool includeInactive) => CharacterBody.GetComponentInChildren(type, includeInactive);
        public new Component GetComponentInParent(Type type) => CharacterBody.GetComponentInParent(type);
        public new T GetComponentInParent<T>() => CharacterBody.GetComponentInParent<T>();

        public new Component[] GetComponents(Type type) => CharacterBody.GetComponents(type);
        public new void GetComponents(Type type, List<Component> results) => CharacterBody.GetComponents(type, results);
        public new T[] GetComponents<T>() => CharacterBody.GetComponents<T>();
        public new void GetComponents<T>(List<T> results) => CharacterBody.GetComponents<T>(results);
        public new Component[] GetComponentsInChildren(Type type) => CharacterBody.GetComponentsInChildren(type);
        public new Component[] GetComponentsInChildren(Type type, bool includeInactive) => CharacterBody.GetComponentsInChildren(type, includeInactive);
        public new T[] GetComponentsInChildren<T>() => CharacterBody.GetComponentsInChildren<T>();
        public new T[] GetComponentsInChildren<T>(bool includeInactive) => CharacterBody.GetComponentsInChildren<T>(includeInactive);
        public new void GetComponentsInChildren<T>(List<T> results) => CharacterBody.GetComponentsInChildren(results);
        public new void GetComponentsInChildren<T>(bool includeInactive, List<T> results) => CharacterBody.GetComponentsInChildren<T>(includeInactive, results);
        public new Component[] GetComponentsInParent(Type type) => CharacterBody.GetComponentsInParent(type);
        public new Component[] GetComponentsInParent(Type type, bool includeInactive) => CharacterBody.GetComponentsInParent(type, includeInactive);
        public new T[] GetComponentsInParent<T>() => CharacterBody.GetComponentsInParent<T>();
        public new T[] GetComponentsInParent<T>(bool includeInactive) => CharacterBody.GetComponentsInParent<T>(includeInactive);
        public new void GetComponentsInParent<T>(bool includeInactive, List<T> results) => CharacterBody.GetComponentsInParent<T>(includeInactive, results);

        public new bool TryGetComponent<T>(out T component) => CharacterBody.TryGetComponent<T>(out component);
        public new bool TryGetComponent(Type type, out Component component) => CharacterBody.TryGetComponent(type, out component);

        internal struct BuffBehaviourBuffDefComparer : IEqualityComparer<BuffBehaviour>
        {
            public bool Equals(BuffBehaviour x, BuffBehaviour y)
            {
                return x.TiedBuffDef.buffIndex == y.TiedBuffDef.buffIndex;
            }

            public int GetHashCode(BuffBehaviour obj)
            {
                return obj.TiedBuffDef.buffIndex.GetHashCode();
            }
        }
    }
}