using System;
using System.Collections.Generic;
using System.Linq;

namespace Moonstorm
{
    public abstract class ModuleBase<T> where T : ContentBase
    {
        public virtual void Initialize() { }

        protected IEnumerable<T> GetContentClasses<T>(Type excludedType = null)
        {
            return GetType()
                            .Assembly
                            .GetTypesSafe()
                            .Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(T)))
                            .Where(type => excludedType != null ? !type.IsSubclassOf(excludedType) : true)
                            .Where(type => !type.GetCustomAttributes(true)
                                .Select(obj => obj.GetType())
                                .Contains(typeof(DisabledContentAttribute)))
                            .Select(type => (T)Activator.CreateInstance(type));
        }

        protected abstract void InitializeContent(T contentClass);
    }
}