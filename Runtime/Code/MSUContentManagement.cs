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
                newBehaviour.CharacterBody = self;
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
            bodyToContentBehaviour.Remove(obj);
            _bodyToBuffBehaviourDictionary.Remove(obj);
        }

        private static void OnBodyAwakeGlobal(CharacterBody obj)
        {
            bodyToContentBehaviour.Add(obj, obj.GetComponent<MSUContentBehaviour>());
            _bodyToBuffBehaviourDictionary.Add(obj, new Dictionary<BuffIndex, BuffBehaviour>());
        }

        internal static void OnBuffBehaviourDestroyed(CharacterBody body, BuffIndex buffIndex)
        {
            if(_bodyToBuffBehaviourDictionary.TryGetValue(body, out var innerDict))
            {
                innerDict.Remove(buffIndex);
            }
        }
    }

    /// <summary>
    /// Base MonoBehaviour of MSU, this MonoBehaviour is used to manage a plethora of Behaviour related interfaces for a Body.
    /// </summary>
    public sealed class MSUContentBehaviour : MonoBehaviour
    {
        /// <summary>
        /// Wether or not the body tied to this ContentBehaviour has a CharacterMaster
        /// </summary>
        public bool HasMaster { get; private set; }
        /// <summary>
        /// The body attached to this ContentBehaviour
        /// </summary>
        public CharacterBody body;
        /// <summary>
        /// The ContentBehaviour's Elite Behaviour, see also <see cref="MSUEliteBehaviour"/>
        /// </summary>
        public MSUEliteBehaviour eliteBehaviour;

        private EquipmentIndex _assignedEliteEquipmentIndex;
        private IStatItemBehavior[] _statItemBehaviors = Array.Empty<IStatItemBehavior>();
        private IBodyStatArgModifier[] _bodyStatArgModifiers = Array.Empty<IBodyStatArgModifier>();

        private IEquipmentContentPiece _equipmentContentPiece;

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
                _equipmentContentPiece?.OnEquipmentLost(body);
                _equipmentContentPiece = equipmentContent;
                _equipmentContentPiece.OnEquipmentObtained(body);
            }

            if (eliteBehaviour)
                CheckEliteBehaviour(def);
        }

        private void CheckEliteBehaviour(EquipmentDef def)
        {
            //Try removing elite qualities if thee incomming def isnt an Elite Equipment.
            if(!def.passiveBuffDef || !def.passiveBuffDef.isElite)
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
            for(int i = 0; i < _bodyStatArgModifiers.Length; i++)
            {
                _bodyStatArgModifiers[i].ModifyStatArguments(args);
            }
        }

        internal void RecalculateStatsStart()
        {
            for(int i = 0; i < _statItemBehaviors.Length; i++)
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

    /// <summary>
    /// The EliteBehaviour is used for managing the extra metadata found in a <see cref="ExtendedEliteDef"/> and applying said metadata to a CharacterBodys
    /// </summary>
    public sealed class MSUEliteBehaviour : MonoBehaviour
    {
        /// <summary>
        /// The CharacterBody assigned to this EliteBehaviour
        /// </summary>
        public CharacterBody body;
        /// <summary>
        /// The <see cref="body"/>'s CharacterModel
        /// </summary>
        public CharacterModel characterModel;

        private GameObject effectInstance;

        internal void AssignNewElite(EliteIndex eliteIndex)
        {
            //Incoming index is none, or the incoming index is not an ExtendedEliteDef, destroy effect instance if needed.
            if(eliteIndex == EliteIndex.None || !(EliteCatalog.GetEliteDef(eliteIndex) is ExtendedEliteDef eed))
            {
                if (effectInstance)
                    Destroy(effectInstance);
                return;
            }

            if (!eed || !eed.effect)
                return;

            effectInstance = Instantiate(eed.effect, body.aimOriginTransform, false);

        }
    }

//https://discord.com/channels/562704639141740588/562704639569428506/1227030308759801866
    /// <summary>
    /// BuffBehaviour is a subclass of a MonoBehaviour that's used for managing a custom behaviour that's applied to a body when it obtains a specific Buff/Debuff
    /// <para>Due to the nature of Buffs being Ephemeral, this Behaviour is not destroyed when the buff is removed, but instead it becomes disabled. Due to this reason, most, if not all behaviour interfaces will still get called even if the behaviour itself is disabled. For this reason it is recommended to break from the function early by checking if <see cref="BuffBehaviour.HasAnyStacks"/> is false</para>
    /// </summary>
    public abstract class BuffBehaviour : MonoBehaviour
    {
        /// <summary>
        /// Attribute used to find the BuffDef tied to a BuffBehaviour.
        /// <br>This attribute should be used to decorate a public static method that returns a BuffDef and does not accept any arguments.</br>
        /// </summary>
        [AttributeUsage(AttributeTargets.Method)]
        public class BuffDefAssociation : HG.Reflection.SearchableAttribute
        {
        }

        /// <summary>
        /// Gets the total stacks of the BuffDef associated with this BuffBehaviour.
        /// <br>This property is automatically updated whenever the Buff's count changes, once the value. Once the behaviour is added, it gets enabled or disabled as needed depending on the incoming value.</br>
        /// </summary>
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

        /// <summary>
        /// The BuffIndex tied to this BuffBehaviour
        /// </summary>
        public BuffIndex BuffIndex { get; internal set; }

        /// <summary>
        /// The CharacterBody that got this BuffBehaviour
        /// </summary>
        public CharacterBody CharacterBody { get; internal set; }

        /// <summary>
        /// Wether or not this BuffBehaviour has any stacks
        /// </summary>
        public bool HasAnyStacks => _buffCount > 0;

        /// <summary>
        /// Called when this buff behaviour is Initialized and obtains a new Stack value when the previous stack was 0. This is a replacement for unity's "OnEnable" method, which should not be used.
        /// </summary>
        protected virtual void OnFirstStackGained() { }

        /// <summary>
        /// Called when this buff behaviour looses all of it's Stacks and it's previous stack was greater than 0. This is a replacement for unity's OnDisable method, which should not be used.
        /// </summary>
        protected virtual void OnAllStacksLost() { }

        /// <summary>
        /// Do not use this method, utilize <see cref="OnFirstStackGained"/> which has the same functionality.
        /// 
        /// <para>The reason behind this is because OnEnable gets called before <see cref="CharacterBody"/> and <see cref="BuffIndex"/> are set.</para>
        /// </summary>
        [Obsolete("Do not use \"OnEnable\", utilize \"OnFirstStackGained\" which has the same functionality", true)]
        protected virtual void OnEnable()
        {

        }

        /// <summary>
        /// Do not use this method, utilize <see cref="OnAllStacksLost"/>, which has the same functionality
        /// </summary>
        [Obsolete("Do not use \"OnDisable\", utilize \"OnAllStacksLost\" which has the same functionality", true)]
        protected virtual void OnDisable()
        {

        }

        protected virtual void OnDestroy()
        {
            MSUContentManagement.OnBuffBehaviourDestroyed(CharacterBody, BuffIndex);
        }
    }
}