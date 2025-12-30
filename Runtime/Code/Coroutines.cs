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


    [Obsolete("Utilize HG.Coroutines.ParallelCoroutine instead.")]
    public class ParallelCoroutine : IEnumerator
    {
        private readonly List<IEnumerator> _coroutinesList = new List<IEnumerator>();

        private IEnumerator _internalCoroutine;

        public bool isDone => this.IsDone();

        public object Current => _internalCoroutine.Current;

        public ParallelCoroutine()
        {
            _internalCoroutine = InternalCoroutine();
        }

        public void Add(IEnumerator coroutine)
        {
            _coroutinesList.Add(coroutine);
        }

        public bool MoveNext()
        {
            return _internalCoroutine.MoveNext();
        }

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

    /// <summary>
    /// A <see cref="CoroutineWithResult"/> is a Coroutine that, upon completion, yield returns a result.
    /// <br></br>
    /// It's utilized as a way to retrieve values asynchronously using a Coroutine. Once the coroutine passed on <see cref="CoroutineWithResult.CoroutineWithResult(IEnumerator)"/> completes, the final yield return value will be stored in <see cref="boxedResult"/>.
    /// <br></br>
    /// See also <see cref="CoroutineWithResult{T}"/>
    /// </summary>
    public class CoroutineWithResult : IEnumerator
    {
        /// <summary>
        /// Special method for returning an instance of <see cref="CoroutineWithResult"/> with a computed result.
        /// <br></br>
        /// This can be useful in scenarios where a result was cached and as such there's nothing to wait.
        /// </summary>
        /// <param name="result">The result itself.</param>
        /// <returns>An instance of <see cref="CoroutineWithResult"/>, that immediatly returns <paramref name="result"/></returns>
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

        /// <summary>
        /// The result of the internal coroutine, you should only access this once <see cref="MoveNext"/> returns false.
        /// </summary>
        public object boxedResult { get; protected set; }

        /// <summary>
        /// Processes the running coroutine
        /// </summary>
        /// <returns>True if there's still more work to be done, otherwise false.</returns>
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

        /// <summary>
        /// Reutilizes this CoroutineWithResult instance, by running <paramref name="coroutineThatEventuallyYieldsAResult"/>
        /// </summary>
        /// <param name="coroutineThatEventuallyYieldsAResult">A Coroutine, that eventually yields a result.</param>
        public void StartNew(IEnumerator coroutineThatEventuallyYieldsAResult)
        {
            _runningCoroutine = coroutineThatEventuallyYieldsAResult;
        }

        void IEnumerator.Reset()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Constructor for <see cref="CoroutineWithResult"/>
        /// </summary>
        /// <param name="coroutineThatEventuallyYieldsAResult">The coroutine, which on the last yield return, returns the result of the operation.</param>
        public CoroutineWithResult(IEnumerator coroutineThatEventuallyYieldsAResult)
        {
            _runningCoroutine = coroutineThatEventuallyYieldsAResult;
        }
    }

    /// <summary>
    /// Generic version of <see cref="CoroutineWithResult"/>
    /// <inheritdoc cref="CoroutineWithResult"/>
    /// </summary>
    /// <typeparam name="T">The type of result</typeparam>
    public sealed class CoroutineWithResult<T> : CoroutineWithResult, IEnumerator<T>
    {
        /// <summary>
        /// Generic version of <see cref="CoroutineWithResult.CompletedResult(object)"/>
        /// <br></br>
        /// Special method for returning an instance of <see cref="CoroutineWithResult{T}"/> with a computed result.
        /// <br></br>
        /// This can be useful in scenarios where a result was cached and as such there's nothing to wait.
        /// </summary>
        /// <param name="result">The result itself.</param>
        /// <returns>An instance of <see cref="CoroutineWithResult{T}"/>, that immediatly returns <paramref name="result"/></returns>
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

        /// <summary>
        /// <inheritdoc cref="CoroutineWithResult.boxedResult"/>
        /// </summary>
        public T result { get; private set; }

        /// <summary>
        /// <inheritdoc cref="CoroutineWithResult.MoveNext"/>
        /// </summary>
        /// <returns><inheritdoc cref="CoroutineWithResult.MoveNext"/></returns>
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

        /// <summary>
        /// <inheritdoc cref="CoroutineWithResult.StartNew(IEnumerator)"/>
        /// </summary>
        /// <param name="coroutineThatEventuallyYieldsAResult"></param>
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

        /// <summary>
        /// <inheritdoc cref="CoroutineWithResult.CoroutineWithResult(IEnumerator)"/>
        /// </summary>
        /// <param name="coroutineThatEventuallyYieldsAResult"><inheritdoc cref="CoroutineWithResult.CoroutineWithResult(IEnumerator)"/></param>
        public CoroutineWithResult(IEnumerator<T> coroutineThatEventuallyYieldsAResult) : base(coroutineThatEventuallyYieldsAResult)
        {
            _runningCoroutine = coroutineThatEventuallyYieldsAResult;
        }
    }
}