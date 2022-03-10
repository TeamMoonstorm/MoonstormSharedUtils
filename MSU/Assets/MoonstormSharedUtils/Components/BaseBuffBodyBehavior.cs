using HG;
using JetBrains.Annotations;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Moonstorm.Components
{
    //Most of this code is from the baseGame's BaseItemBodyBehavior, but modified to work with Buffs.
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
                this.buffTypePairs = buffTypePairs.ToArray();
                behaviorArraysPool = new FixedSizeArrayPool<BaseBuffBodyBehavior>(this.buffTypePairs.Length);
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

        protected void Awake()
        {
            body = earlyAssignmentBody;
            earlyAssignmentBody = null;
        }

        [SystemInitializer(typeof(BuffCatalog))]
        private static void Init()
        {
            List<BuffTypePair> serverList = new List<BuffTypePair>();
            List<BuffTypePair> clientList = new List<BuffTypePair>();
            List<BuffTypePair> sharedList = new List<BuffTypePair>();

            List<BuffDefAssociationAttribute> attributes = new List<BuffDefAssociationAttribute>();
            HG.Reflection.SearchableAttribute.GetInstances(attributes);

            Type typeFromHandle = typeof(BaseBuffBodyBehavior);
            Type typeFromHandle2 = typeof(BuffDef);

            foreach(BuffDefAssociationAttribute attribute in attributes)
            {
                MethodInfo methodInfo;
                if((methodInfo = attribute.target as MethodInfo) == null)
                {
                    MSULog.Error($"BuffDefAssociationAttribute cannot be applied to object of type '{attribute?.GetType().FullName}'");
                    continue;
                }
                if(!methodInfo.IsStatic)
                {
                    MSULog.Error($"BuffDefAssociationAttribute cannot be applied to method '{methodInfo.DeclaringType.FullName}.{methodInfo.Name}': Method is not static.");
                    continue;
                }
                Type type = attribute.behaviorTypeOverride ?? methodInfo.DeclaringType;
                if(!typeFromHandle.IsAssignableFrom(type))
                {
                    MSULog.Error($"BuffDefAssociationAttribute cannot be applied to method '{methodInfo.DeclaringType.FullName}.{methodInfo.Name}': {methodInfo.DeclaringType.FullName} does not derive from {typeFromHandle.FullName}.");
                    continue;
                }
                if(type.IsAbstract)
                {
                    MSULog.Error($"BuffDefAssociationAttribute cannot be applied to method '{methodInfo.DeclaringType.FullName}.{methodInfo.Name}': {methodInfo.DeclaringType.FullName} is an abstract type.");
                    continue;
                }
                if(!typeFromHandle2.IsAssignableFrom(methodInfo.ReturnType))
                {
                    MSULog.Error($"BuffDefAssociationAttribute cannot be applied to method '{methodInfo.DeclaringType.FullName}.{methodInfo.Name}': {methodInfo.DeclaringType.FullName}.{methodInfo.Name} returns type '{methodInfo.ReturnType?.FullName ?? "void"} instead of {typeFromHandle2.FullName}");
                    continue;
                }
                if(methodInfo.GetGenericArguments().Length != 0)
                {
                    MSULog.Error($"BuffDefAssociationAttribute cannot be applied to method '{methodInfo.DeclaringType.FullName}.{methodInfo.Name}': {methodInfo.DeclaringType.FullName}.{methodInfo.Name} must take no arguments.");
                    continue;
                }
                BuffDef buffDef = (BuffDef)methodInfo.Invoke(null, Array.Empty<object>());
                if(!buffDef)
                {
                    MSULog.Error($"{methodInfo.DeclaringType.FullName}.{methodInfo.Name} returned null.");
                    continue;
                }
                if(buffDef.buffIndex < (BuffIndex)0)
                {
                    MSULog.Error($"{methodInfo.DeclaringType.FullName}.{methodInfo.Name} returned a BuffDef that's not registered in the Buffcatalog. result={buffDef}");
                    continue;
                }
                BuffTypePair buff;
                if(attribute.useOnServer)
                {
                    buff = new BuffTypePair
                    {
                        buffIndex = buffDef.buffIndex,
                        behaviorType = type
                    };
                    serverList.Add(buff);
                }
                if(attribute.useOnClient)
                {
                    buff = new BuffTypePair
                    {
                        buffIndex = buffDef.buffIndex,
                        behaviorType = type
                    };
                    clientList.Add(buff);
                }
                if (attribute.useOnServer || attribute.useOnClient)
                {
                    buff = new BuffTypePair
                    {
                        buffIndex = buffDef.buffIndex,
                        behaviorType = type
                    };
                    sharedList.Add(buff);
                }
            }
            server.SetBuffTypePairs(serverList);
            client.SetBuffTypePairs(clientList);
            shared.SetBuffTypePairs(sharedList);

            //Hooks
            CharacterBody.onBodyAwakeGlobal += OnBodyAwakeGlobal;
            CharacterBody.onBodyDestroyGlobal += OnBodyDestroyGlobal;
            On.RoR2.CharacterBody.SetBuffCount += OnSetBuffCount;
        }


        private static ref NetworkContextSet GetNetworkContext()
        {
            bool serverActive = NetworkServer.active;
            bool clientActive = NetworkClient.active;

            if(serverActive)
            {
                if(clientActive)
                {
                    return ref shared;
                }
                return ref server;
            }
            if(clientActive)
            {
                return ref client;
            }
            throw new InvalidOperationException("Neither server nor client is running.");
        }
        private static void OnBodyAwakeGlobal(CharacterBody obj)
        {
            BaseBuffBodyBehavior[] value = GetNetworkContext().behaviorArraysPool.Request();
            bodyToBuffBehaviors.Add(obj, value);
        }

        private static void OnBodyDestroyGlobal(CharacterBody body)
        {
            BaseBuffBodyBehavior[] behaviors = bodyToBuffBehaviors[body];
            for(int i = 0; i < behaviors.Length; i++)
            {
                UnityEngine.Object.Destroy(behaviors[i]);
            }
            bodyToBuffBehaviors.Remove(body);
            if(NetworkServer.active || NetworkClient.active)
            {
                GetNetworkContext().behaviorArraysPool.Return(behaviors);
            }
        }
        private static void OnSetBuffCount(On.RoR2.CharacterBody.orig_SetBuffCount orig, CharacterBody self, BuffIndex buffType, int newCount)
        {
            orig(self, buffType, newCount);
            UpdateBodyBuffBehaviorStacks(self, newCount);
        }

        private static void UpdateBodyBuffBehaviorStacks(CharacterBody body, int buffStacks)
        {
            ref NetworkContextSet networkContext = ref GetNetworkContext();
            BaseBuffBodyBehavior[] array = bodyToBuffBehaviors[body];
            BuffTypePair[] buffTypePairs = networkContext.buffTypePairs;
            if(body)
            {
                for(int i = 0; i < buffTypePairs.Length; i++)
                {
                    BuffTypePair buffTypePair = buffTypePairs[i];
                    SetBuffStack(body, ref array[i], buffTypePair.behaviorType, buffStacks);
                }
                return;
            }
            for(int j = 0; j < buffTypePairs.Length; j++)
            {
                ref BaseBuffBodyBehavior reference = ref array[j];
                if(reference != null)
                {
                    Destroy(reference);
                    reference = null;
                }
            }
        }
        private static void SetBuffStack(CharacterBody body, ref BaseBuffBodyBehavior behavior, Type behaviorType, int stacks)
        {
            if(behavior == null != stacks <= 0)
            {
                if(stacks <= 0)
                {
                    Destroy(behavior);
                    behavior = null;
                }
                else
                {
                    earlyAssignmentBody = body;
                    behavior = (BaseBuffBodyBehavior)body.gameObject.AddComponent(behaviorType);
                    earlyAssignmentBody = null;
                }
            }
            if(behavior != null)
            {
                behavior.buffStacks = stacks;
            }
        }
    }
}
