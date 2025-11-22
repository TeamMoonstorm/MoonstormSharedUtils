using System;
using System.Reflection;
using UnityEngine;

namespace MSU
{
    /// <summary>
    /// A structure that's used to serialize a Static Method within the editor.
    /// <para></para>
    /// Methods can be restricted by applying a <see cref="RequiredArgumentsAttribute"/> attribute to a field of <see cref="SerializableStaticMethod"/>.
    /// <para></para>
    /// For the method to be selected in the editor, the method must be decorated with a <see cref="MethodDetectorAttribute"/>
    /// </summary>
    [Serializable]
    public struct SerializableStaticMethod
    {
        /// <summary>
        /// The AssemblyQualifiedTypeName of the Type that contains the method
        /// </summary>
        public string assemblyQualifiedTypeName;

        /// <summary>
        /// The name of the method
        /// </summary>
        public string methodName;

        /// <summary>
        /// Tries to get the Method stored in this SerializableStaticMethod
        /// </summary>
        /// <param name="methodInfo">The retrieved MethodInfo</param>
        /// <returns>True if the method was obtained succesfully, otherwise false.</returns>
        public bool TryGetMethod(out MethodInfo methodInfo)
        {
            methodInfo = null;

            Type t = Type.GetType(assemblyQualifiedTypeName);
            if (t == null)
                return false;

            methodInfo = t.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            return methodInfo != null;
        }

        /// <summary>
        /// An attribute utilized on fields of <see cref="SerializableStaticMethod"/> that can specify what kind of Arguments the method must have
        /// </summary>
        public class RequiredArgumentsAttribute : PropertyAttribute
        {
            /// <summary>
            /// The return type of the method
            /// </summary>
            public Type returnType;

            /// <summary>
            /// What Arguments the method has
            /// </summary>
            public Type[] arguments;

            /// <summary>
            /// Constructor for <see cref="RequiredArgumentsAttribute"/>
            /// </summary>
            /// <param name="returnType">The return type of the method</param>
            /// <param name="arguments">What arguments the method has</param>
            public RequiredArgumentsAttribute(Type returnType, params Type[] arguments)
            {
                this.returnType = returnType;
                this.arguments = arguments;
            }
        }

        /// <summary>
        /// This is used in combination with the TypeCache to easily find methods.
        /// </summary>
        public class MethodDetectorAttribute : Attribute
        {

        }
    }
}