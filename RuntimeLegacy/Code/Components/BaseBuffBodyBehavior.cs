using HG;
using JetBrains.Annotations;
using RoR2;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;

namespace Moonstorm.Components
{
    public abstract class BaseBuffBodyBehavior : MonoBehaviour
    {
        private struct BuffTypePair
        {
            public BuffIndex buffIndex;
            public Type behaviorType;
        }

        private struct NetworkContextSet
        {
            public BuffTypePair[] buffTypePairs;

            public FixedSizeArrayPool<BaseBuffBodyBehavior> behaviorArraysPool;

            public void SetBuffTypePairs(List<BuffTypePair> buffTypePairs)
            {
                throw new System.NotImplementedException();
            }
        }

        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
        [MeansImplicitUse]
        public class BuffDefAssociationAttribute : HG.Reflection.SearchableAttribute
        {
            public Type behaviorTypeOverride;

            public bool useOnServer = true;

            public bool useOnClient = true;
        }

        public int buffStacks;

        private static NetworkContextSet server;

        private static NetworkContextSet client;

        private static NetworkContextSet shared;

        private static CharacterBody earlyAssignmentBody = null;

        private static Dictionary<UnityObjectWrapperKey<CharacterBody>, BaseBuffBodyBehavior[]> bodyToBuffBehaviors = new Dictionary<UnityObjectWrapperKey<CharacterBody>, BaseBuffBodyBehavior[]>();

        public CharacterBody body { get; private set; }

        protected virtual void Awake()
        {
            throw new System.NotImplementedException();
        }

        private static void Init()
        {
            throw new System.NotImplementedException();
        }


        private static ref NetworkContextSet GetNetworkContext()
        {
            throw new System.NotImplementedException();
        }
        private static void OnBodyAwakeGlobal(CharacterBody obj)
        {
            throw new System.NotImplementedException();
        }

        private static void OnBodyDestroyGlobal(CharacterBody body)
        {
            throw new System.NotImplementedException();
        }
        private static void OnSetBuffCount(On.RoR2.CharacterBody.orig_SetBuffCount orig, CharacterBody self, BuffIndex buffType, int newCount)
        {
            throw new System.NotImplementedException();
        }

        private static void UpdateBodyBuffBehaviorStacks(CharacterBody body, int buffStacks, BuffIndex index)
        {
            throw new System.NotImplementedException();
        }
        private static void SetBuffStack(CharacterBody body, ref BaseBuffBodyBehavior behavior, Type behaviorType, int stacks)
        {
            throw new System.NotImplementedException();
        }
    }
}
