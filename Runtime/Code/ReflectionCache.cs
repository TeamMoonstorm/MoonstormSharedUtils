using System;
using System.Collections.Generic;
using System.Reflection;

namespace MSU
{
    internal static class ReflectionCache
    {
        private static Dictionary<Assembly, Type[]> _assemblyToTypes = new Dictionary<Assembly, Type[]>();
        public static Type[] GetTypes(Assembly assembly)
        {
            if (_assemblyToTypes.ContainsKey(assembly))
                return _assemblyToTypes[assembly];

            Type[] types = assembly.GetTypes();
            _assemblyToTypes.Add(assembly, types);
            return types;
        }
    }
}