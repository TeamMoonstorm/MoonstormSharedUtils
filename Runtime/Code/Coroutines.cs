using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MSU
{
    [Obsolete("Use HG.Coroutines.ParallelCoroutine instead, calling all the enumerator methods at the same frame makes no difference in terms of code execution")]
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
    /// A Coroutine that's used to process multiple coroutines in parallel.
    /// <para></para>
    /// This is identical to <see cref="HG.Coroutines.ParallelCoroutine"/>, however this version is pooled, new instances can be requested via <see cref="ParallelCoroutine.RequestFromPool"/> and they can be returned via <see cref="ParallelCoroutine.ReturnToPool(ParallelCoroutine)"/>
    /// </summary>
    public class ParallelCoroutine : IEnumerator
    {
        private static Stack<ParallelCoroutine> _pool = new Stack<ParallelCoroutine>();

        /// <summary>
        /// Requests a ParallelCoroutine from the pool
        /// </summary>
        public static ParallelCoroutine RequestFromPool()
        {
            if(_pool.TryPop(out var coroutine))
            {
                return coroutine;
            }
#pragma warning disable CS0618 // Type or member is obsolete
            return new ParallelCoroutine();
#pragma warning restore CS0618 // Type or member is obsolete
        }

        /// <summary>
        /// Returns a ParallelCoroutine to the pool
        /// </summary>
        /// <param name="coroutine"></param>
        public static void ReturnToPool(ParallelCoroutine coroutine)
        {
            coroutine.Reset();
            _pool.Push(coroutine);
        }

        private readonly List<IEnumerator> _coroutinesList = new List<IEnumerator>();

        private IEnumerator _internalCoroutine;

        /// <summary>
        /// Returns true once all coroutines are completed
        /// </summary>
        public bool isDone => this.IsDone();

        /// <summary>
        /// Returns the current object stored in the internal coroutine
        /// </summary>
        public object Current => _internalCoroutine.Current;

        /// <summary>
        /// This method should not be called, call <see cref="ParallelCoroutine.RequestFromPool"/> instead
        /// </summary>
        [Obsolete("Instances of ParallelCoroutine should not be created manually, utilize the pool instead")]
        public ParallelCoroutine()
        {
            _internalCoroutine = InternalCoroutine();
        }

        /// <summary>
        /// Adds a new coroutine to process
        /// </summary>
        public void Add(IEnumerator coroutine)
        {
            _coroutinesList.Add(coroutine);
        }

        /// <summary>
        /// Processes the coroutine. Returns true if there's more work to be done
        /// </summary>
        public bool MoveNext()
        {
            return _internalCoroutine.MoveNext();
        }

        /// <summary>
        /// Resets this class, resetting the internal coroutine and clearing it's internal coroutine list
        /// </summary>
        public void Reset()
        {
            _internalCoroutine = InternalCoroutine();
            _coroutinesList.Clear();
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

    [Obsolete("Utilize HG.Coroutines.CoroutineWithResult instead", error: true)]
    public class CoroutineWithResult : IEnumerator
    {
        public static CoroutineWithResult CompletedResult(object result)
        {
            IEnumerator YieldResultASAP(object result)
            {
                yield return result;
            }

            return new CoroutineWithResult(YieldResultASAP(result));
        }
        object IEnumerator.Current => throw new NotImplementedException();

        protected IEnumerator _runningCoroutine;

        public object boxedResult { get; protected set; }

        public virtual bool MoveNext()
        {
            bool moveNextValue = _runningCoroutine.MoveNext();
            //Only retrieve the result if moveNext is true.
            if (moveNextValue)
            {
                boxedResult = _runningCoroutine.Current;
            }
            return moveNextValue;
        }

        public void StartNew(IEnumerator coroutineThatEventuallyYieldsAResult)
        {
            _runningCoroutine = coroutineThatEventuallyYieldsAResult;
        }

        void IEnumerator.Reset()
        {
            throw new NotSupportedException();
        }

        public CoroutineWithResult(IEnumerator coroutineThatEventuallyYieldsAResult)
        {
            _runningCoroutine = coroutineThatEventuallyYieldsAResult;
        }
    }

    [Obsolete("Utilize HG.Coroutines.CoroutineWithResult<T> instead", error: true)]
    public sealed class CoroutineWithResult<T> : CoroutineWithResult, IEnumerator<T>
    {
        public static CoroutineWithResult<T> CompletedResult(T result)
        {
            IEnumerator<T> YieldResultASAP(T result)
            {
                yield return result;
            }

            return new CoroutineWithResult<T>(YieldResultASAP(result));
        }
        T IEnumerator<T>.Current => throw new NotImplementedException();

        object IEnumerator.Current => throw new NotImplementedException();

        new private IEnumerator<T> _runningCoroutine;

        public T result { get; private set; }

        public override bool MoveNext()
        {
            bool moveNextValue = _runningCoroutine.MoveNext();
            //Only retrieve the result if moveNext is true.
            if(moveNextValue)
            {
                boxedResult = _runningCoroutine.Current;
                result = _runningCoroutine.Current;
            }
            return moveNextValue;
        }

        public void StartNew(IEnumerator<T> coroutineThatEventuallyYieldsAResult)
        {
            _runningCoroutine = coroutineThatEventuallyYieldsAResult;
            base._runningCoroutine = coroutineThatEventuallyYieldsAResult;
        }

        void IEnumerator.Reset()
        {
            throw new NotSupportedException();
        }

        void IDisposable.Dispose()
        {
            throw new NotSupportedException();
        }

        public CoroutineWithResult(IEnumerator<T> coroutineThatEventuallyYieldsAResult) : base(coroutineThatEventuallyYieldsAResult)
        {
            _runningCoroutine = coroutineThatEventuallyYieldsAResult;
        }
    }
}