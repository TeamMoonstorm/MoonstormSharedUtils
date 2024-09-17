using RoR2;
using System;
using UnityEngine;

namespace MSU
{
    //https://discord.com/channels/562704639141740588/562704639569428506/1227030308759801866
    /// <summary>
    /// BaseBuffBehaviour is a subclass of a MonoBehaviour that's used for managing a custom behaviour that's applied to a body when it obtains a specific Buff/Debuff
    /// <para>Due to the nature of Buffs being Ephemeral, this Behaviour is not destroyed when the buff is removed, but instead it becomes disabled. Due to this reason, most, if not all behaviour interfaces will still get called even if the behaviour itself is disabled. For this reason it is recommended to break from the function early by checking if <see cref="hasAnyStacks"/> is false. The following code block is an exmaple.</para>
    /// <para>
    /// <code>
    /// public class MyBuffBehaviour : BaseBuffBehaviour, IOnKilledServerReceiver, IBodyStatArgModifier
    /// {
    ///     [BuffDefAssociation]
    ///     private static BuffDef GetBuffDef() => MyModContent.Buffs.MyBuff;
    ///     
    ///     //Avoids adding damage multiplier if the buff behaviour is disabled.
    ///     public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
    ///     {
    ///         if(!HasAnyStacks)
    ///             return;
    ///             
    ///         args.damageMultAdd += BuffCount;
    ///     }
    ///     
    ///     //Avoids killing the attacker if the victim doesnt have any buff stacks.
    ///     public void OnKilledServer(DamageReport damageReport)
    ///     {
    ///         if(!HasAnyStacks)
    ///             return;
    ///             
    ///         damageReport?.attackerBody?.healthComponent?.Suicide();
    ///     }
    /// }
    /// </code>
    /// </para>
    /// </summary>
    public abstract class BaseBuffBehaviour : MonoBehaviour
    {
        /// <summary>
        /// Attribute used to find the BuffDef tied to a BaseBuffBehaviour.
        /// <br>This attribute should be used to decorate a public static method that returns a BuffDef and does not accept any arguments.</br>
        /// </summary>
        [AttributeUsage(AttributeTargets.Method)]
        public class BuffDefAssociation : HG.Reflection.SearchableAttribute
        {
        }

        /// <summary>
        /// Gets the total stacks of the BuffDef associated with this BaseBuffBehaviour.
        /// <br>This property is automatically updated whenever the Buff's count changes, once the value. Once the behaviour is added, it gets enabled or disabled as needed depending on the incoming value.</br>
        /// </summary>
        public int buffCount
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

                if (previous == 0 && _buffCount > 0)
                {
                    enabled = true;
                    OnFirstStackGained();
                }
                if (previous > 0 && _buffCount == 0)
                {
                    enabled = false;
                    OnAllStacksLost();
                }
            }
        }
        private int _buffCount;

        /// <summary>
        /// The BuffIndex tied to this BaseBuffBehaviour
        /// </summary>
        public BuffIndex buffIndex { get; internal set; }

        /// <summary>
        /// The CharacterBody that got this BaseBuffBehaviour
        /// </summary>
        public CharacterBody characterBody { get; internal set; }

        /// <summary>
        /// Wether or not this BaseBuffBehaviour has any stacks
        /// </summary>
        public bool hasAnyStacks => _buffCount > 0;

        /// <summary>
        /// Called when this buff behaviour is Initialized and obtains a new Stack value when the previous stack was 0. <para>This is basically syntaxis sugar, as "OnEnabled" can also be used for this effect.</para>
        /// </summary>
        protected virtual void OnFirstStackGained() { }

        /// <summary>
        /// Called when this buff behaviour looses all of it's Stacks and it's previous stack was greater than 0.
        /// <para>This is basically syntaxis sugar, as "OnDisabled" can also be used for this effect.</para>
        /// </summary>
        protected virtual void OnAllStacksLost() { }

        /// <summary>
        /// Awake method for BaseBuffBehaviour
        /// 
        /// <br>Call the base method so that the <see cref="characterBody"/> property is properly initialized.</br>
        /// </summary>
        protected virtual void Awake()
        {
            characterBody = GetComponent<CharacterBody>();
        }

        /// <summary>
        /// OnDestroy method for BaseBuffBehaviour
        /// 
        /// <br>Call the base method so that the buff behaviour gets properly destroyed and resources are freed.</br>
        /// </summary>
        protected virtual void OnDestroy()
        {
            MSUContentManagement.OnBuffBehaviourDestroyed(characterBody, buffIndex);
        }
    }
}
