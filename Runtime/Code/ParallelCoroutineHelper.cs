using R2API.Utils;
using System;
using System.Collections;
using System.Collections.Generic;

namespace MSU
{   
    public class ParallelCoroutineHelper
    {
        private List<Wrapper> _wrappers = new List<Wrapper>();

        public void Add(Func<IEnumerator> func)
        {
            _wrappers.Add(new Wrapper
            {
                @delegate = func
            });
        }

        public void Add(Delegate @delegate, object[] args)
        {
            if (!IsSafe(@delegate, args))
                return;

            _wrappers.Add(new Wrapper
            {
                @delegate = @delegate,
                args = args,
            });
        }

        private bool IsSafe(Delegate @delegate, object[] args)
        {
            var methodInfo = @delegate.Method;

            var returnType = methodInfo.ReturnType;
            if (returnType == null || returnType == typeof(void))
                return false;

            if (!methodInfo.ReturnType.IsSameOrSubclassOf<IEnumerator>())
            {
                return false;
            }

            var parameters = methodInfo.GetParameters();
            if (parameters.Length != args.Length)
                return false;

            for(int i = 0; i < parameters.Length; i++)
            {
                var argType = args[i].GetType();
                if (!(parameters[i].ParameterType == argType || parameters[i].ParameterType.IsSubclassOf(argType)))
                {
                    return false;
                }
            }

            return true;
        }

        public void Start()
        {
            foreach (Wrapper wrapper in _wrappers)
            {
                wrapper.Start();
            }
        }

        public bool IsDone()
        {
            foreach (Wrapper wrapper in _wrappers)
            {
                if (!wrapper.IsDone)
                    return false;
            }
            return true;
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