using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MSU
{
    [Serializable]
    public class NullableRef<T> where T : class
    {
        public T Value
        {
            get
            {
                if (!HasValue)
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

        public bool HasValue => (_value is UnityEngine.Object obj) ? obj : _value != null;

        public static implicit operator T(NullableRef<T> o)
        {
            return o?.Value;
        }

        public static implicit operator bool(NullableRef<T> o)
        {
            return o?.HasValue ?? false;
        }

        public static implicit operator NullableRef<T>(T obj)
        {
            return new NullableRef<T> { Value = obj };
        }
    }
}
