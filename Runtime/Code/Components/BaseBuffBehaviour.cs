using MSU;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MSU
{
    //https://discord.com/channels/562704639141740588/562704639569428506/1227030308759801866
    /// <summary>
    /// BaseBuffBehaviour is a subclass of a MonoBehaviour that's used for managing a custom behaviour that's applied to a body when it obtains a specific Buff/Debuff
    /// <para>Due to the nature of Buffs being Ephemeral, this Behaviour is not destroyed when the buff is removed, but instead it becomes disabled. Due to this reason, most, if not all behaviour interfaces will still get called even if the behaviour itself is disabled. For this reason it is recommended to break from the function early by checking if <see cref="HasAnyStacks"/> is false</para>
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
        public BuffIndex BuffIndex { get; internal set; }

        /// <summary>
        /// The CharacterBody that got this BaseBuffBehaviour
        /// </summary>
        public CharacterBody CharacterBody { get; internal set; }

        /// <summary>
        /// Wether or not this BaseBuffBehaviour has any stacks
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
