using RoR2.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Moonstorm
{
    /// <summary>
    /// A Class where all other ModuleBase classes inherit from
    /// </summary>
    public abstract class ModuleBase
    {

        /// <summary>
        /// Your Mod's Content Pack
        /// </summary>
        public virtual SerializableContentPack ContentPack { get; set; }

        /// <summary>
        /// Your Mod's AssetBundle
        /// </summary>
        public virtual AssetBundle AssetBundle { get; set; }

        public virtual void Init() { }

        /// <summary>
        /// Gets all the ContentClasses of type T that dont have the DisabledContent attribute
        /// </summary>
        /// <typeparam name="T">The type of content base to look for</typeparam>
        /// <param name="excludedType">A type of class that works as an extra filter. PickupsModuleBase uses this for filtering between Equipments and EliteEquipments</param>
        /// <returns></returns>
        internal protected IEnumerable<T> GetContentClasses<T>(Type excludedType = null) where T : ContentBase
        {
            var types = GetType().Assembly.GetTypes()
                                          .Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(T)));

            if (excludedType != null)
                types = types.Where(type => !type.IsSubclassOf(excludedType));

            return types.Where(type => !type.GetCustomAttributes(true)
                                            .Select(obj => obj.GetType())
                                            .Contains(typeof(DisabledContent)))
                        .Select(type => (T)Activator.CreateInstance(type));
        }
    }
}
