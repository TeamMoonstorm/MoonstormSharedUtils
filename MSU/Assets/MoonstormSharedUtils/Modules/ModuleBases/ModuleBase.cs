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

        protected void AddSafelyToDict<TKey, TValue>(ref Dictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            try
            {
                if (dictionary.ContainsKey(key))
                    throw new InvalidOperationException($"Cannot add {key} as it's already present within the dictionary {dictionary}!");

                dictionary.Add(key, value);
            }
            catch(Exception e) { MSULog.Error(e); }
        }

        protected void AddSafelyToList<TObject>(ref List<TObject> list, TObject obj)
        {
            try
            {
                if (list.Contains(obj))
                    throw new InvalidOperationException($"Cannot add {obj} as its already present within the dictionary {list}");

                list.Add(obj);
            }
            catch
            {

            }
        }

        protected abstract bool InitializeContent(T contentClass);

        protected static void ThrowModuleNotInitialized(string triedAction, Type module)
        {
            try
            {
                throw new InvalidOperationException($"Cannot {triedAction} because {module.Name} is not initialized.");
            }
            catch (Exception e) { MSULog.Error(e); }
        }

        protected static void ThrowModuleInitialized(string triedAction, Type module)
        {
            try
            {
                throw new InvalidOperationException($"Cannot {triedAction} because {module.Name} has already initialized.");
            }
            catch (Exception e) { MSULog.Error(e); }
        }
    }
}