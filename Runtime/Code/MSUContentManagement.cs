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
        private static Dictionary<UnityObjectWrapperKey<CharacterBody>, Dictionary<BuffIndex, BaseBuffBehaviour>> _bodyToBuffBehaviourDictionary = new Dictionary<UnityObjectWrapperKey<CharacterBody>, Dictionary<BuffIndex, BaseBuffBehaviour>>();
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
            if (sender.bodyIndex == BodyIndex.None)
                return;

            bodyToContentBehaviour[sender].GetStatCoefficients(args);
        }

        private static void RecalculateStats(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self)
        {
            if (self.bodyIndex == BodyIndex.None)
            {
                orig(self);
                return;
            }

            var behaviour = bodyToContentBehaviour[self];
            behaviour.RecalculateStatsStart();
            orig(self);
            behaviour.RecalculateStatsEnd();
        }

        private static void Holy_GetStatsCoefficients(CharacterBody body, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (body.bodyIndex == BodyIndex.None)
                return;

            var behaviour = bodyToContentBehaviour[body];
            behaviour.RecalculateStatsStart();
            behaviour.GetStatCoefficients(args);
        }

        private static void Holy_RecalculateStats(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self)
        {
            if (self.bodyIndex == BodyIndex.None)
            {
                orig(self);
                return;
            }

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

                    var characterModel = bodyPrefab.GetComponentInChildren<CharacterModel>();
                    if(characterModel)
                    {
                        var eliteBehaviour = characterModel.gameObject.AddComponent<MSUEliteBehaviour>();
                        eliteBehaviour.characterModel = characterModel;
                        eliteBehaviour.body = manager.body;
                        manager.eliteBehaviour = eliteBehaviour;
                    }

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
            List<BaseBuffBehaviour.BuffDefAssociation> attributes = new List<BaseBuffBehaviour.BuffDefAssociation>();
            HG.Reflection.SearchableAttribute.GetInstances(attributes);

            Type buffBehaviourType = typeof(BaseBuffBehaviour);
            Type buffDefType = typeof(BuffDef);
            foreach(BaseBuffBehaviour.BuffDefAssociation attribute in attributes)
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

            if (self.bodyIndex == BodyIndex.None)
                return;

            if (!_buffToBehaviour.ContainsKey(buffType) || !self)
                return;

            var bodyBuffBehaviours = _bodyToBuffBehaviourDictionary[self];
            if(!bodyBuffBehaviours.ContainsKey(buffType))
            {
                var newBehaviour = (BaseBuffBehaviour)self.gameObject.AddComponent(_buffToBehaviour[buffType]);
                newBehaviour.BuffIndex = buffType;
                newBehaviour.BuffCount = newCount;
                bodyBuffBehaviours.Add(buffType, newBehaviour);
                var manager = bodyToContentBehaviour[self];
                manager.StartGetInterfaces();
                return;
            }
            bodyBuffBehaviours[buffType].BuffCount = newCount;
        }


        private static void OnBodyDestroyedGlobal(CharacterBody obj)
        {
            if (obj.bodyIndex == BodyIndex.None)
                return;

            bodyToContentBehaviour.Remove(obj);
            _bodyToBuffBehaviourDictionary.Remove(obj);
        }

        private static void OnBodyAwakeGlobal(CharacterBody obj)
        {
            if (obj.bodyIndex == BodyIndex.None)
                return;

            bodyToContentBehaviour.Add(obj, obj.GetComponent<MSUContentBehaviour>());
            _bodyToBuffBehaviourDictionary.Add(obj, new Dictionary<BuffIndex, BaseBuffBehaviour>());
        }

        internal static void OnBuffBehaviourDestroyed(CharacterBody body, BuffIndex buffIndex)
        {
            if (body.bodyIndex == BodyIndex.None)
                return;

            if (_bodyToBuffBehaviourDictionary.TryGetValue(body, out var innerDict))
            {
                innerDict.Remove(buffIndex);
            }
        }
    }
}