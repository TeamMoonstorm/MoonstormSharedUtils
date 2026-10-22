using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MSU
{
    /// <summary>
    /// A Coroutine that's used to process multiple coroutines in parallel.
    /// <para></para>
    /// This is identical to <see cref="HG.Coroutines.ParallelCoroutine"/>, however this version is pooled, new instances can be requested via <see cref="ParallelCoroutine.RequestFromPool"/> and they can be returned via <see cref="ParallelCoroutine.ReturnToPool(ParallelCoroutine)"/>
    /// </summary>
    public sealed class ParallelCoroutine : IEnumerator, IDisposable
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

            return new ParallelCoroutine();
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

        private ParallelCoroutine()
        {
            _internalCoroutine = InternalCoroutine();
        }

        ~ParallelCoroutine()
        {
            if(_coroutinesList.Count > 0)
            {
                MSULog.Error($"A ParallelCoroutine with coroutines in it has been collected by the GarbageCollector. This means it wasnt disposed. Consider disposing your ParallelCoroutine once you're done with it to have less strain on the GarbageCollector");
            }
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

        /// <summary>
        /// Returns this parallel coroutine to the pool
        /// </summary>
        public void Dispose()
        {
            ReturnToPool(this);
        }
    }
}