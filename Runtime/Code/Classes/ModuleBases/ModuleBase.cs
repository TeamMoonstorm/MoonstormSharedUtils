using R2API.ScriptableObjects;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moonstorm
{
    /// <summary>
    /// A class which all module bases derive from
    /// </summary>
    /// <typeparam name="T">The type of content base this module base manages</typeparam>
    public abstract class ModuleBase<T> where T : ContentBase
    {
        public virtual void Initialize() { }

        protected IEnumerable<T> GetContentClasses<T>(Type excludedType = null) where T : ContentBase
        {
            return GetType()
                            .Assembly
                            .GetTypes()
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