using R2API.Utils;
using System;
using System.Collections;
using System.Collections.Generic;

namespace MSU
{   
    /// <summary>
    /// A class used for running multiple Coroutine methods in parallel
    /// </summary>
    public class ParallelCoroutineHelper
    {
        private List<Wrapper> _wrappers = new List<Wrapper>();

        public bool IsDone()
        {
            foreach(Wrapper wrapper in _wrappers)
            {
                if (!wrapper.IsDone)
                    return false;
            }
            return true;
            
        }

        #region ADD
        public void Add(Func<IEnumerator> func)
        {
            _wrappers.Add(new Wrapper
            {
                @delegate = func
            });
        }

        public void Add<T1>(Func<T1, IEnumerator> func, T1 arg)
        {
            _wrappers.Add(new Wrapper
            {
                @delegate = func,
                args = new object[] { arg }
            });
        }

        public void Add<T1, T2>(Func<T1, T2, IEnumerator> func, T1 arg1, T2 arg2)
        {
            _wrappers.Add(new Wrapper
            {
                @delegate = func,
                args = new object[] { arg1, arg2 }
            });
        }

        public void Add<T1, T2, T3>(Func<T1, T2, T3, IEnumerator> func, T1 arg1, T2 arg2, T3 arg3)
        {
            _wrappers.Add(new Wrapper
            {
                @delegate = func,
                args = new object[] { arg1, arg2, arg3 }
            });
        }

        public void Add<T1, T2, T3, T4>(Func<T1, T2, T3, T4, IEnumerator> func, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            _wrappers.Add(new Wrapper
            {
                @delegate = func,
                args = new object[] { arg1, arg2, arg3, arg4 }
            });
        }
        public void Add<T1, T2, T3, T4, T5>(Func<T1, T2, T3, T4, T5, IEnumerator> func, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            _wrappers.Add(new Wrapper
            {
                @delegate = func,
                args = new object[] { arg1, arg2, arg3, arg4, arg5 }
            });
        }
        #endregion


        public void Start()
        {
            foreach (Wrapper wrapper in _wrappers)
            {
                wrapper.Start();
            }
        }

        private class Wrapper
        {
            public Delegate @delegate;
            public object[] args;
            public IEnumerator coroutine;

            public void Start()
            {
                coroutine = (IEnumerator)@delegate.DynamicInvoke(args);
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