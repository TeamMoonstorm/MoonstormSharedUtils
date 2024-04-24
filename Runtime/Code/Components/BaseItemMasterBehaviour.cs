using HG;
using JetBrains.Annotations;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Networking;
using UnityEngine;
using RoR2.Items;
using System.Reflection;

namespace MSU
{
    /// <summary>
    /// BaseItemMasterBehaviour is a subclass of a MonoBehaviour thats used for managing a custom behaviour that's applied to a CharacterMaster when it's inventory has a specific Item.
    /// <para>It has a lot of similarities to the base game's <see cref="BaseItemBodyBehavior"/>, but instead of being applied to the CharacterBody game object, its applied to the CharacterMaster game object.</para>
    /// </summary>
    public abstract class BaseItemMasterBehaviour : MonoBehaviour
    {
        /// <summary>
        /// The CharacterMaster that's tied to this BaseItemMasterBehaviour
        /// </summary>
        public CharacterMaster Master { get; private set; }

        protected virtual void Awake()
        {
            this.Master = BaseItemMasterBehaviour.earlyAssignmentMaster;
            BaseItemMasterBehaviour.earlyAssignmentMaster = null;
        }

        [SystemInitializer(new Type[]
        {
            typeof(ItemCatalog)
        })]
        private static void Init()
        {
            List<BaseItemBodyBehavior.ItemTypePair> server = new List<BaseItemBodyBehavior.ItemTypePair>();
            List<BaseItemBodyBehavior.ItemTypePair> client = new List<BaseItemBodyBehavior.ItemTypePair>();
            List<BaseItemBodyBehavior.ItemTypePair> shared = new List<BaseItemBodyBehavior.ItemTypePair>();
            List<BaseItemMasterBehaviour.ItemDefAssociationAttribute> attributeList = new List<BaseItemMasterBehaviour.ItemDefAssociationAttribute>();
            HG.Reflection.SearchableAttribute.GetInstances<BaseItemMasterBehaviour.ItemDefAssociationAttribute>(attributeList);

            Type masterBehaviourType = typeof(BaseItemMasterBehaviour);
            Type itemDefType = typeof(ItemDef);
            foreach (BaseItemMasterBehaviour.ItemDefAssociationAttribute itemDefAssociationAttribute in attributeList)
            {
                MethodInfo methodInfo;
                if ((methodInfo = (itemDefAssociationAttribute.target as MethodInfo)) == null)
                {
                    MSULog.Error("ItemDefAssociationAttribute cannot be applied to object of type '" + ((itemDefAssociationAttribute != null) ? itemDefAssociationAttribute.GetType().FullName : null) + "'");
                }
                else if (!methodInfo.IsStatic)
                {
                    MSULog.Error(string.Concat(new string[]
                    {
                        "ItemDefAssociationAttribute cannot be applied to method ",
                        methodInfo.DeclaringType.FullName,
                        ".",
                        methodInfo.Name,
                        ": Method is not static."
                    }));
                }
                else
                {
                    Type type = itemDefAssociationAttribute.behaviorTypeOverride ?? methodInfo.DeclaringType;
                    if (!masterBehaviourType.IsAssignableFrom(type))
                    {
                        MSULog.Error(string.Concat(new string[]
                        {
                            "ItemDefAssociationAttribute cannot be applied to method ",
                            methodInfo.DeclaringType.FullName,
                            ".",
                            methodInfo.Name,
                            ": ",
                            methodInfo.DeclaringType.FullName,
                            " does not derive from ",
                            masterBehaviourType.FullName,
                            "."
                        }));
                    }
                    else if (type.IsAbstract)
                    {
                        MSULog.Error(string.Concat(new string[]
                        {
                            "ItemDefAssociationAttribute cannot be applied to method ",
                            methodInfo.DeclaringType.FullName,
                            ".",
                            methodInfo.Name,
                            ": ",
                            methodInfo.DeclaringType.FullName,
                            " is an abstract type."
                        }));
                    }
                    else if (!itemDefType.IsAssignableFrom(methodInfo.ReturnType))
                    {
                        string format = "{0} cannot be applied to method {1}.{2}: {3}.{4} returns type '{5}' instead of {6}.";
                        object[] array = new object[7];
                        array[0] = "ItemDefAssociationAttribute";
                        array[1] = methodInfo.DeclaringType.FullName;
                        array[2] = methodInfo.Name;
                        array[3] = methodInfo.DeclaringType.FullName;
                        array[4] = methodInfo;
                        int num = 5;
                        Type returnType = methodInfo.ReturnType;
                        array[num] = (((returnType != null) ? returnType.FullName : null) ?? "void");
                        array[6] = itemDefType.FullName;
                        MSULog.Error(string.Format(format, array));
                    }
                    else if (methodInfo.GetGenericArguments().Length != 0)
                    {
                        MSULog.Error(string.Format("{0} cannot be applied to method {1}.{2}: {3}.{4} must take no arguments.", new object[]
                        {
                            "ItemDefAssociationAttribute",
                            methodInfo.DeclaringType.FullName,
                            methodInfo.Name,
                            methodInfo.DeclaringType.FullName,
                            methodInfo
                        }));
                    }
                    else
                    {
                        ItemDef itemDef = (ItemDef)methodInfo.Invoke(null, Array.Empty<object>());
                        if (!itemDef)
                        {
                            MSULog.Error(methodInfo.DeclaringType.FullName + "." + methodInfo.Name + " returned null.");
                        }
                        else if (itemDef.itemIndex < (ItemIndex)0)
                        {
                            MSULog.Error(string.Format("{0}.{1} returned an ItemDef that's not registered in the ItemCatalog. result={2}", methodInfo.DeclaringType.FullName, methodInfo.Name, itemDef));
                        }
                        else
                        {
                            if (itemDefAssociationAttribute.useOnServer)
                            {
                                server.Add(new BaseItemBodyBehavior.ItemTypePair
                                {
                                    itemIndex = itemDef.itemIndex,
                                    behaviorType = type
                                });
                            }
                            if (itemDefAssociationAttribute.useOnClient)
                            {
                                client.Add(new BaseItemBodyBehavior.ItemTypePair
                                {
                                    itemIndex = itemDef.itemIndex,
                                    behaviorType = type
                                });
                            }
                            if (itemDefAssociationAttribute.useOnServer || itemDefAssociationAttribute.useOnClient)
                            {
                                shared.Add(new BaseItemBodyBehavior.ItemTypePair
                                {
                                    itemIndex = itemDef.itemIndex,
                                    behaviorType = type
                                });
                            }
                        }
                    }
                }
            }
            BaseItemMasterBehaviour.server.SetItemTypePairs(server);
            BaseItemMasterBehaviour.client.SetItemTypePairs(client);
            BaseItemMasterBehaviour.shared.SetItemTypePairs(shared);
            On.RoR2.CharacterMaster.Awake += CharacterMaster_Awake;
            On.RoR2.CharacterMaster.OnDestroy += CharacterMaster_OnDestroy;
            On.RoR2.CharacterMaster.OnInventoryChanged += CharacterMaster_OnInventoryChanged;
        }

        private static void CharacterMaster_Awake(On.RoR2.CharacterMaster.orig_Awake orig, CharacterMaster self)
        {
            BaseItemMasterBehaviour[] value = BaseItemMasterBehaviour.GetNetworkContext().behaviorArraysPool.Request();
            //SS2Log.Info("adding " + self + " of value " + value + " in charmast awake");
            BaseItemMasterBehaviour.masterToItemBehaviors.Add(self, value);
            orig(self);
        }

        private static void CharacterMaster_OnDestroy(On.RoR2.CharacterMaster.orig_OnDestroy orig, CharacterMaster self)
        {
            orig(self);
            BaseItemMasterBehaviour[] array = BaseItemMasterBehaviour.masterToItemBehaviors[self];
            for (int i = 0; i < array.Length; i++)
            {
                Destroy(array[i]);
            }
            BaseItemMasterBehaviour.masterToItemBehaviors.Remove(self);
            if (NetworkServer.active || NetworkClient.active)
            {
                BaseItemMasterBehaviour.GetNetworkContext().behaviorArraysPool.Return(array);
            }
        }

        private static void CharacterMaster_OnInventoryChanged(On.RoR2.CharacterMaster.orig_OnInventoryChanged orig, CharacterMaster self)
        {
            orig(self);
            BaseItemMasterBehaviour.UpdateMasterItemBehaviorStacks(self);

        }

        private static ref BaseItemMasterBehaviour.NetworkContextSet GetNetworkContext()
        {
            bool networkActive = NetworkServer.active;
            bool clientActive = NetworkClient.active;
            if (networkActive)
            {
                if (clientActive)
                {
                    return ref BaseItemMasterBehaviour.shared;
                }
                return ref BaseItemMasterBehaviour.server;
            }
            else
            {
                if (clientActive)
                {
                    return ref BaseItemMasterBehaviour.client;
                }
                throw new InvalidOperationException("Neither server nor client is running.");
            }
        }

        private static void UpdateMasterItemBehaviorStacks(CharacterMaster master)
        {
            ref BaseItemMasterBehaviour.NetworkContextSet networkContext = ref BaseItemMasterBehaviour.GetNetworkContext();
            //SS2Log.Info("Calling problem line");
            BaseItemMasterBehaviour[] arr;
            bool success = BaseItemMasterBehaviour.masterToItemBehaviors.TryGetValue(master, out arr);
            if (!success)
            {
                //SS2Log.Info("Failed to Find"); //My understanding is this gets called post-master being destroyed therefore the master it's looking for is null -> original function throws an error?
                return;							 //since it would just error here in the past and the game still functioned i think i can just return
            }
            //BaseItemMasterBehavior[] arr = BaseItemMasterBehavior.masterToItemBehaviors.TryGetValue(master);
            //BaseItemMasterBehavior[] array = BaseItemMasterBehavior.masterToItemBehaviors[master]; // problem line
            BaseItemBodyBehavior.ItemTypePair[] itemTypePairs = networkContext.itemTypePairs;
            Inventory inventory = master.inventory;
            if (inventory)
            {
                for (int i = 0; i < itemTypePairs.Length; i++)
                {
                    BaseItemBodyBehavior.ItemTypePair itemTypePair = itemTypePairs[i];
                    ref BaseItemMasterBehaviour behavior = ref arr[i];
                    BaseItemMasterBehaviour.SetItemStack(master, ref behavior, itemTypePair.behaviorType, inventory.GetItemCount(itemTypePair.itemIndex));
                }
                return;
            }
            for (int j = 0; j < itemTypePairs.Length; j++)
            {
                ref BaseItemMasterBehaviour ptr = ref arr[j];
                if (ptr != null)
                {
                    Destroy(ptr);
                    ptr = null;
                }
            }
        }

        private static void SetItemStack(CharacterMaster master, ref BaseItemMasterBehaviour behavior, Type behaviorType, int stack)
        {
            if (behavior == null != stack <= 0)
            {
                if (stack <= 0)
                {
                    behavior.stack = 0;
                    Destroy(behavior);
                    behavior = null;
                }
                else
                {
                    BaseItemMasterBehaviour.earlyAssignmentMaster = master;
                    behavior = (BaseItemMasterBehaviour)master.gameObject.AddComponent(behaviorType);
                    BaseItemMasterBehaviour.earlyAssignmentMaster = null;
                }
            }
            if (behavior != null)
            {
                behavior.stack = stack;
            }
        }

        /// <summary>
        /// The amount of items the behaviour has
        /// </summary>
        public int stack;

        private static BaseItemMasterBehaviour.NetworkContextSet server;

        private static BaseItemMasterBehaviour.NetworkContextSet client;

        private static BaseItemMasterBehaviour.NetworkContextSet shared;

        private static CharacterMaster earlyAssignmentMaster = null;

        private static Dictionary<UnityObjectWrapperKey<CharacterMaster>, BaseItemMasterBehaviour[]> masterToItemBehaviors = new Dictionary<UnityObjectWrapperKey<CharacterMaster>, BaseItemMasterBehaviour[]>();

        private struct NetworkContextSet
        {
            public void SetItemTypePairs(List<BaseItemBodyBehavior.ItemTypePair> itemTypePairs)
            {
                this.itemTypePairs = itemTypePairs.ToArray();
                this.behaviorArraysPool = new FixedSizeArrayPool<BaseItemMasterBehaviour>(this.itemTypePairs.Length);
            }

            public BaseItemBodyBehavior.ItemTypePair[] itemTypePairs;

            public FixedSizeArrayPool<BaseItemMasterBehaviour> behaviorArraysPool;
        }

        /// <summary>
        /// Attribute used to find the ItemDef tied to a BaseItemMasterBehaviour.
        /// <br>This attribute should be used to decorate a public static method that returns an ItemDef and does not accept any arguments.</br>
        /// </summary>
        [MeansImplicitUse]
        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
        public class ItemDefAssociationAttribute : HG.Reflection.SearchableAttribute
        {
            public Type behaviorTypeOverride;

            public bool useOnServer = true;

            public bool useOnClient = true;
        }
    }
}
