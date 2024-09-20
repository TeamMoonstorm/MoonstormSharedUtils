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
        private static Dictionary<UnityObjectWrapperKey<CharacterBody>, MSUContentBehaviour> _bodyToContentBehaviour = new Dictionary<UnityObjectWrapperKey<CharacterBody>, MSUContentBehaviour>();

        private static Dictionary<BuffIndex, Type> _buffToBehaviour = new Dictionary<BuffIndex, Type>();
        private static Dictionary<UnityObjectWrapperKey<CharacterBody>, Dictionary<BuffIndex, BaseBuffBehaviour>> _bodyToBuffBehaviourDictionary = new Dictionary<UnityObjectWrapperKey<CharacterBody>, Dictionary<BuffIndex, BaseBuffBehaviour>>();

        [SystemInitializer(typeof(BodyCatalog), typeof(BuffCatalog))]
        private static IEnumerator SystemInit()
        {
            MSULog.Info("Initializing the generalized MSU Content Management system...");
            yield return null;

            On.RoR2.CharacterBody.OnBuffFirstStackGained += CallEliteBehaviourFirstStackMethod;
            On.RoR2.CharacterBody.OnBuffFinalStackLost += CallEliteBehaviourFinalStackMethod;
            On.RoR2.CharacterBody.SetBuffCount += SetBuffBehaviourCount;
            CharacterBody.onBodyAwakeGlobal += OnBodyAwakeGlobal;
            CharacterBody.onBodyDestroyGlobal += OnBodyDestroyedGlobal;

            if (MSUtil.holyDLLInstalled)
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

            ParallelMultiStartCoroutine coroutine = new ParallelMultiStartCoroutine();
            coroutine.Add(InitMSUContentBehaviourSystem);
            coroutine.Add(InitBuffBehaviourSystem);

            coroutine.Start();
            while (!coroutine.isDone)
                yield return null;
        }

        private static void CallEliteBehaviourFinalStackMethod(On.RoR2.CharacterBody.orig_OnBuffFinalStackLost orig, CharacterBody self, BuffDef buffDef)
        {
            orig(self, buffDef);
            if (self.bodyIndex == BodyIndex.None)
                return;

            if (!buffDef.isElite)
                return;

            if (buffDef.eliteDef is not ExtendedEliteDef eed)
                return;

            var behaviour = _bodyToContentBehaviour[self];
            if (!behaviour.eliteBehaviour)
                return;

            behaviour.eliteBehaviour.OnEliteBuffFinalStackLost(buffDef.buffIndex, eed);
        }

        private static void CallEliteBehaviourFirstStackMethod(On.RoR2.CharacterBody.orig_OnBuffFirstStackGained orig, CharacterBody self, BuffDef buffDef)
        {
            orig(self, buffDef);
            if (self.bodyIndex == BodyIndex.None)
                return;

            if (!buffDef.isElite)
                return;

            if (buffDef.eliteDef is not ExtendedEliteDef eed)
                return;

            var behaviour = _bodyToContentBehaviour[self];
            if (!behaviour.eliteBehaviour)
                return;

            behaviour.eliteBehaviour.OnEliteBuffFirstStackGained(buffDef.buffIndex, eed);
        }

        private static void GetStatsCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender.bodyIndex == BodyIndex.None)
                return;

            _bodyToContentBehaviour[sender].GetStatCoefficients(args);
        }

        private static void RecalculateStats(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self)
        {
            if (self.bodyIndex == BodyIndex.None)
            {
                orig(self);
                return;
            }

            var behaviour = _bodyToContentBehaviour[self];
            behaviour.RecalculateStatsStart();
            orig(self);
            behaviour.RecalculateStatsEnd();
        }

        private static void Holy_GetStatsCoefficients(CharacterBody body, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (body.bodyIndex == BodyIndex.None)
                return;

            var behaviour = _bodyToContentBehaviour[body];
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
            _bodyToContentBehaviour[self].RecalculateStatsEnd();
        }

        private static IEnumerator InitMSUContentBehaviourSystem()
        {
            for (int i = 0; i < BodyCatalog.bodyPrefabs.Length; i++)
            {
                yield return null;
                try
                {
                    GameObject bodyPrefab = BodyCatalog.bodyPrefabs[i];

                    var manager = bodyPrefab.AddComponent<MSUContentBehaviour>();
                    manager.body = bodyPrefab.GetComponent<CharacterBody>();

                    var characterModel = bodyPrefab.GetComponentInChildren<CharacterModel>();
                    if (characterModel)
                    {
                        var eliteBehaviour = characterModel.gameObject.AddComponent<MSUEliteBehaviour>();
                        eliteBehaviour.characterModel = characterModel;
                        eliteBehaviour.body = manager.body;
                        manager.eliteBehaviour = eliteBehaviour;
                    }

                    BodyCatalog.bodyPrefabs[i] = bodyPrefab;
                }
                catch (Exception e)
                {
                    MSULog.Error(e);
                }
            }
        }
        private static IEnumerator InitBuffBehaviourSystem()
        {
            List<BaseBuffBehaviour.BuffDefAssociation> attributes = new List<BaseBuffBehaviour.BuffDefAssociation>();
            HG.Reflection.SearchableAttribute.GetInstances(attributes);

            Type buffBehaviourType = typeof(BaseBuffBehaviour);
            Type buffDefType = typeof(BuffDef);
            foreach (BaseBuffBehaviour.BuffDefAssociation attribute in attributes)
            {
                yield return null;
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
            if (!bodyBuffBehaviours.ContainsKey(buffType))
            {
                var newBehaviour = (BaseBuffBehaviour)self.gameObject.AddComponent(_buffToBehaviour[buffType]);
                newBehaviour.buffIndex = buffType;
                newBehaviour.buffCount = newCount;
                bodyBuffBehaviours.Add(buffType, newBehaviour);
                var manager = _bodyToContentBehaviour[self];
                manager.StartGetInterfaces();
                return;
            }
            bodyBuffBehaviours[buffType].buffCount = newCount;
        }


        private static void OnBodyDestroyedGlobal(CharacterBody obj)
        {
            if (obj.bodyIndex == BodyIndex.None)
                return;

            _bodyToContentBehaviour.Remove(obj);
            _bodyToBuffBehaviourDictionary.Remove(obj);
        }

        private static void OnBodyAwakeGlobal(CharacterBody obj)
        {
            if (obj.bodyIndex == BodyIndex.None)
                return;

            _bodyToContentBehaviour.Add(obj, obj.GetComponent<MSUContentBehaviour>());
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