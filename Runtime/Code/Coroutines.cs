using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MSU
{
    [Obsolete("Use ParallelCoroutine instead, calling all the enumerator methods at the same frame makes no difference in terms of code execution")]
    public class ParallelMultiStartCoroutine : IEnumerator
    {
        private List<Wrapper> _wrappers = new List<Wrapper>();

        public bool isDone
        {
            get
            {
                if (_internalCoroutine == null)
                    Start();

                return _internalCoroutine.IsDone();
            }
        }

        object IEnumerator.Current => _internalCoroutine.Current;

        private IEnumerator _internalCoroutine;

        public void Start()
        {
            for (int i = 0; i < _wrappers.Count; i++)
            {
                var wrapper = _wrappers[i];
                wrapper.coroutine = (IEnumerator)(wrapper.coroutineDelegate?.DynamicInvoke(wrapper.args));
                _wrappers[i] = wrapper;
            }
            _internalCoroutine = InternalCoroutine();
        }

        #region ADD
        public void Add(Func<IEnumerator> func)
        {
            _wrappers.Add(new Wrapper
            {
                coroutineDelegate = func
            });
        }

        public void Add<T1>(Func<T1, IEnumerator> func, T1 arg)
        {
            _wrappers.Add(new Wrapper
            {
                coroutineDelegate = func,
                args = new object[] { arg }
            });
        }

        public void Add<T1, T2>(Func<T1, T2, IEnumerator> func, T1 arg1, T2 arg2)
        {
            _wrappers.Add(new Wrapper
            {
                coroutineDelegate = func,
                args = new object[] { arg1, arg2 }
            });
        }

        public void Add<T1, T2, T3>(Func<T1, T2, T3, IEnumerator> func, T1 arg1, T2 arg2, T3 arg3)
        {
            _wrappers.Add(new Wrapper
            {
                coroutineDelegate = func,
                args = new object[] { arg1, arg2, arg3 }
            });
        }

        public void Add<T1, T2, T3, T4>(Func<T1, T2, T3, T4, IEnumerator> func, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            _wrappers.Add(new Wrapper
            {
                coroutineDelegate = func,
                args = new object[] { arg1, arg2, arg3, arg4 }
            });
        }

        public void Add<T1, T2, T3, T4, T5>(Func<T1, T2, T3, T4, T5, IEnumerator> func, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            _wrappers.Add(new Wrapper
            {
                coroutineDelegate = func,
                args = new object[] { arg1, arg2, arg3, arg4, arg5 }
            });
        }
        #endregion

        private IEnumerator InternalCoroutine()
        {
            yield return null;

            bool encounteredUnfinished = true;
            while (encounteredUnfinished)
            {
                encounteredUnfinished = false;
                int i = _wrappers.Count - 1;
                while (i >= 0)
                {
                    Wrapper wrapper = _wrappers[i];
                    if (!wrapper.coroutine.IsDone())
                    {
                        encounteredUnfinished = true;
                        yield return wrapper.coroutine.Current;
                    }
                    else
                    {
                        _wrappers.RemoveAt(i);
                    }
                    i--;
                }
            }
        }

        bool IEnumerator.MoveNext()
        {
            if (_internalCoroutine == null)
                Start();

            return _internalCoroutine?.MoveNext() ?? false;
        }

        void IEnumerator.Reset()
        {
            if (_internalCoroutine == null)
                Start();

            _internalCoroutine?.MoveNext();
        }

        private struct Wrapper
        {
            public Delegate coroutineDelegate;
            public object[] args;

            public IEnumerator coroutine;
        }
    }

    /// <summary>
    /// A version of RoR2's <see cref="HG.Coroutines.ParallelProgressCoroutine"/> which does not have a progress receiver.
    /// </summary>
    public class ParallelCoroutine : IEnumerator
    {
        private readonly List<IEnumerator> _coroutinesList = new List<IEnumerator>();

        private IEnumerator _internalCoroutine;

        /// <summary>
        /// returns true if all the coroutines have finished executing.
        /// </summary>
        public bool isDone => this.IsDone();

        /// <summary>
        /// The current object that was yielded
        /// </summary>
        public object Current => _internalCoroutine.Current;

        /// <summary>
        /// Constructor for Parallel Coroutine
        /// </summary>
        public ParallelCoroutine()
        {
            _internalCoroutine = InternalCoroutine();
        }

        /// <summary>
        /// Adds a new coroutine to process
        /// </summary>
        /// <param name="coroutine">The coroutine to process</param>
        public void Add(IEnumerator coroutine)
        {
            _coroutinesList.Add(coroutine);
        }

        /// <summary>
        /// Processes the coroutines
        /// </summary>
        /// <returns>True if the coroutines are NOT finished, false if the coroutines ARE finished</returns>
        public bool MoveNext()
        {
            return _internalCoroutine.MoveNext();
        }

        /// <summary>
        /// resets the internal coroutine
        /// </summary>
        public void Reset()
        {
            _internalCoroutine.Reset();
        }

        private IEnumerator InternalCoroutine()
        {
            yield return null;
            bool encounteredUnfinished = true;
            while (encounteredUnfinished)
            {
                encounteredUnfinished = false;
                int i = _coroutinesList.Count - 1;
                while (i >= 0)
                {
                    IEnumerator coroutine = _coroutinesList[i];
                    if (coroutine.MoveNext())
                    {
                        encounteredUnfinished = true;
                        yield return coroutine.Current;
                    }
                    else
                    {
                        _coroutinesList.RemoveAt(i);
                    }
                    int num = i - 1;
                    i = num;
                }
            }
        }
    }
}