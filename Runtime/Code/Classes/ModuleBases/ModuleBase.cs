using System;
using System.Collections.Generic;
using System.Linq;

namespace Moonstorm
{
    /// <summary>
    /// A class which all module bases derive from
    /// <para>A module base's main job is to get the classes from your assembly that inherit from <typeparamref name="T"/> that are not disabled, and create instances of them.</para>
    /// <para>All module bases have a ContentBase class they manage, denoted by the typeParam <typeparamref name="T"/></para>
    /// </summary>
    /// <typeparam name="T">The type of content base this module base manages</typeparam>
    public abstract class ModuleBase<T> where T : ContentBase
    {
        /// <summary>
        /// Overwrite this method to initialize your module
        /// </summary>
        public virtual void Initialize() { }

        /// <summary>
        /// Calling this method will scan your assembly for classes that inherit from <typeparamref name="T"/> and are not abstract
        /// <para>Once it finds them, it'll create instances of each using the <see cref="Activator.CreateInstance(Type)"/> method</para>
        /// </summary>
        /// <typeparam name="T">The type of content base to search for</typeparam>
        /// <param name="excludedType">
        ///     If specified, the method will search for types that ONLY inherit from <typeparamref name="T"/>
        ///     <para>An example would be the EquipmentModule, calling GetContentClasses{EquipmentBase}(typeof(EliteEquipmentBase) only collects and creates instances of classes that inherit from EquipmentBase and not classes that inherit from EliteEquipmentBase</para>
        /// </param>
        /// <returns></returns>
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

        /// <summary>
        /// A module inherit from module base must implement their own initialization process of the content class they manage
        /// </summary>
        /// <param name="contentClass">The content class to be initialized</param>
        protected abstract void InitializeContent(T contentClass);
    }
}