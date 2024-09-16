using HG;
using R2API.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace MSU
{   
    /// <summary>
    /// Class used for wrapping multiple coroutine methods, which then can be started on parallel and subsecuently awaited on parallel
    /// 
    /// <para>See also <see cref="ParallelCoroutine"/></para>
    /// </summary>
    public class ParallelMultiStartCoroutine : IEnumerator
    {
        private List<Wrapper> _wrappers = new List<Wrapper>();

        /// <summary>
        /// Returns true if the execution of all the coroutines has finished. False otherwise
        /// </summary>
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

        /// <summary>
        /// Iterates thru the wrapped coroutine methods and calls them, effectively beginning the parallel coroutine
        /// </summary>
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

        //This dumb thing does not work
        /*
        /// <summary>
        /// Adds a new method to be wrapped and eventually called with <see cref="Start"/>
        /// </summary>
        /// <param name="_delegate">The method itself. The method must return IEnumerator.</param>
        /// <param name="args">The arguments for the method specified in <paramref name="_delegate"/></param>
        public void AddMethod(Delegate _delegate, params object[] args)
        {
            ValidateIncomingMethod(_delegate, args);
            _wrappers.Add(new Wrapper
            {
                args = args,
                coroutineDelegate = _delegate
            });
        }
        
         private void ValidateIncomingMethod(Delegate _delegate, object[] args)
        {
            var methodInfo = _delegate.Method;

            var returnType = methodInfo.ReturnType;

            if (returnType == null || returnType == typeof(void))
            {
                throw new NullReferenceException($"Delegate's return type is null or void. (Delegate={BuildBestName()})");
            }

            if(!returnType.IsSameOrSubclassOf<IEnumerator>())
            {
                throw new NullReferenceException($"Delegate's return type is not of type IEnumerator. (Delegate={BuildBestName()})");
            }

            var parameters = methodInfo.GetParameters();

            if(parameters.Length != args.Length)
            {
                throw new ArgumentException($"Object array length does not match delegate's argument length. (Delegate={BuildBestName()})");
            }

            for(int i = 0; i < args.Length; i++)
            {
                var paramType = parameters[i].ParameterType;

                if (!args[i].GetType().IsSameOrSubclassOf(paramType))
                {
                    throw new ArgumentException($"Argument at index {i} does not match the method's {i} argument type. (Delegate={BuildBestName()})");
                }
            }

            string BuildBestName()
            {
                StringBuilder stringBuilder = new StringBuilder();

                stringBuilder.Append(returnType.Name);
                stringBuilder.Append(' ');
                stringBuilder.Append(methodInfo.DeclaringType.FullName);
                stringBuilder.Append(".");
                stringBuilder.Append(methodInfo.Name);
                stringBuilder.Append("(");
                if(args.Length > 0)
                {
                    foreach(var arg in args)
                    {
                        stringBuilder.Append(arg.GetType().FullName);
                        stringBuilder.Append(", ");
                    }
                }
                stringBuilder.Append(")");

                return stringBuilder.ToString();
            }
        }*/

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
            while(encounteredUnfinished)
            {
                encounteredUnfinished = false;
                int i = _wrappers.Count - 1;
                while(i >= 0)
                {
                    Wrapper wrapper = _wrappers[i];
                    if(!wrapper.coroutine.IsDone())
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
            return _internalCoroutine?.MoveNext() ?? false;
        }

        void IEnumerator.Reset()
        {
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
    /// 
    /// <para>See also <see cref="ParallelMultiStartCoroutine"/></para>
    /// </summary>
    public class ParallelCoroutine : IEnumerator
    {
        private readonly List<IEnumerator> _coroutinesList = new List<IEnumerator>();

        private IEnumerator internalCoroutine;

        public object Current => internalCoroutine.Current;

        public ParallelCoroutine()
        {
            internalCoroutine = InternalCoroutine();
        }

        public void Add(IEnumerator coroutine)
        {
            _coroutinesList.Add(coroutine);
        }

        public bool MoveNext()
        {
            return internalCoroutine.MoveNext();
        }

        public void Reset()
        {
            internalCoroutine.Reset();
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