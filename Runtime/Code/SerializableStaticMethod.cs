using System;
using System.Reflection;
using UnityEngine;

namespace MSU
{
    [Serializable]
    public struct SerializableStaticMethod
    {
        public string assemblyQualifiedTypeName;
        public string methodName;

        public bool TryGetMethod(out MethodInfo methodInfo)
        {
            Type t = Type.GetType(assemblyQualifiedTypeName);
            methodInfo = t.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            return methodInfo != null;
        }

        public class RequiredArguments : PropertyAttribute
        {
            public Type returnType;
            public Type[] arguments;
            public RequiredArguments(Type returnType, params Type[] arguments)
            {
                this.returnType = returnType;
                this.arguments = arguments;
            }
        }

        /// <summary>
        /// This is used in combination with the TypeCache to easily find methods.
        /// </summary>
        public class MethodDetector : Attribute
        {

        }
    }
}