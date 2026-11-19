using System;
using UnityEngine;

namespace MSU
{
    [Serializable]
    [Obsolete(OBSOLETE_MESSAGE)]
    public struct NullableRef<T> where T : class
    {
        public const string OBSOLETE_MESSAGE = "NullableRef is no longer supported and will be removed on MSU 3.0, Replace any usages of this struct with just the Reference Type as is. (IE: NullableRef<GameObject> -> GameObject)\r\nIf this is NullableRef is part of one of MSU's ContentPiece interfaces then you cant fix this until you begin upgrading to MSU 3.0";
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

        public bool hasValue => (_value is UnityEngine.Object obj) ? obj : _value != null;

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

        public static implicit operator T(NullableRef<T> o)
        {
            return o.value;
        }

        public static implicit operator bool(NullableRef<T> o)
        {
            return o.hasValue;
        }

        public static implicit operator NullableRef<T>(T obj)
        {
            return new NullableRef<T> { value = obj };
        }

        public override string ToString()
        {
            if (hasValue)
                return value.ToString();
            return $"NullableRef<{typeof(T).Name}>(null)";
        }

        public override int GetHashCode()
        {
            if (hasValue)
                return value.GetHashCode();
            return -1;
        }
    }
}
