using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MSU
{
    /// <summary>
    /// A wrapper struct to mark that a Reference type may be null.
    /// <para>Due to the nature of unity and using FrankenMono, Nullable Reference Types are not allowed in the unity editor and as such it throws exceptions when unity's compiler tries to compile the code.</para>
    /// <para>Thus, this struct exists.</para>
    /// </summary>
    /// <typeparam name="T">The type contained by this nullable ref</typeparam>
    [Serializable]
    public struct NullableRef<T> where T : class
    {
        /// <summary>
        /// The value stored by this NullableRef
        /// <para>Throws a NullReferenceException if no value is stored.</para>
        /// </summary>
        public T value
        {
            get
            {
                if (!hasValue)
                    throw new NullReferenceException();
                return _value;
            }
            set
            {
                _value = value;
            }
        }
        [SerializeField]
        private T _value;

        /// <summary>
        /// Checks if this NullableRef has any Value that's not Null
        /// </summary>
        public bool hasValue => (_value is UnityEngine.Object obj) ? obj : _value != null;

        /// <summary>
        /// Tries to get the Value stored in this NullableRef
        /// </summary>
        /// <param name="value">An outgoing variable that'll contain the NullableRef's value</param>
        /// <returns>True if the value was obtained safely, false otherwise</returns>
        public bool TryGetValue(out T value)
        {
            if (hasValue)
            {
                value = this.value;
                return true;
            }
        
            value = default(T);
            return false;
        }

        /// <summary>
        /// Casts a NullableRef to <typeparamref name="T"/>. Throws an exception if the NullableRef does not have a value
        /// </summary>
        public static implicit operator T(NullableRef<T> o)
        {
            return o.value;
        }

        /// <summary>
        /// Casts a NullableRef to a boolean value, said boolean value equals wether the NullableRef has a Value or not
        /// </summary>
        public static implicit operator bool(NullableRef<T> o)
        {
            return o.hasValue;
        }

        /// <summary>
        /// Encapsulates a reference value into a NullableRef
        /// </summary>
        public static implicit operator NullableRef<T>(T obj)
        {
            return new NullableRef<T> { value = obj };
        }

        /// <summary>
        /// Returns a readable representation of this NullableRef
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (hasValue)
                return value.ToString();
            return $"NullableRef<{typeof(T).Name}>(null)";
        }

        /// <summary>
        /// Returns a HashCode for this NullableRef using it's underlying value.
        /// </summary>
        /// <returns>A valid HashCode if <see cref="hasValue"/> is true, otherwise it returns -1</returns>
        public override int GetHashCode()
        {
            if (hasValue)
                return value.GetHashCode();
            return -1;
        }
    }
}
