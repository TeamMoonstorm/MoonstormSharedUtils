using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace MSU
{
    /// <summary>
    /// A class for declaring multiple coroutine based methods and then executing them in parallel.
    /// </summary>
    public class ParallelCoroutineHelper
    {
        private List<Wrapper> _wrappers = new List<Wrapper>();

        public void Add(Func<IEnumerator> func)
        {
            _wrappers.Add(new Wrapper
            {
                coroutineMethod = func
            });
        }

        public void Start()
        {
            foreach(Wrapper wrapper in _wrappers)
            {
                wrapper.Start();
            }
        }

        public bool IsDone()
        {
            foreach(Wrapper wrapper in _wrappers)
            {
                if (!wrapper.IsDone)
                    return false;
            }
            return true;
        }

        private class Wrapper
        {
            public Func<IEnumerator> coroutineMethod;
            public IEnumerator coroutine;

            public void Start()
            {
                coroutine = coroutineMethod.Invoke();
            }

            public bool IsDone
            {
                get
                {
                    if (coroutine == null)
                        return true;

                    if (!coroutine.MoveNext())
                    {
                        return true;
                    }
                    return false;
                }
            }
        }
    }
}