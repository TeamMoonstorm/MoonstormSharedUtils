using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MSU
{
    internal static class ReflectionCache
    {
        private static Dictionary<Assembly, Type[]> assemblyToTypes = new Dictionary<Assembly, Type[]>();
        public static Type[] GetTypes(Assembly assembly)
        {
            if (assemblyToTypes.ContainsKey(assembly))
                return assemblyToTypes[assembly];

            Type[] types = assembly.GetTypes();
            assemblyToTypes.Add(assembly, types);
            return types;
        }
    }
}