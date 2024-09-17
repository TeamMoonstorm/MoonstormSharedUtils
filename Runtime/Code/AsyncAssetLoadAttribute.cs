using BepInEx;
using HG.Reflection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MSU
{
    /// <summary>
    /// The AsyncAssetLoadAttribute is a <see cref="SearchableAttribute"/> that can be used to decorate static methods that work as Coroutines (IE: returns an IEnumerator using yields to asynchronously execute code).
    /// <para>Utilizing <see cref="CreateCoroutineForMod(BaseUnityPlugin)"/>, you can then collect all of your mod's AsyncASsetLoadAttributes that will be added to a <see cref="ParallelMultiStartCoroutine"/>, which then will be able to Start and await, running all the methods decorated with this attribute as a Parallelized Coroutine.</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class AsyncAssetLoadAttribute : SearchableAttribute
    {
        /// <summary>
        /// The MethodInfo that's attached to this Attribute
        /// </summary>
        public MethodInfo targetMethodInfo => target as MethodInfo;

        /// <summary>
        /// Creates a <see cref="ParallelMultiStartCoroutine"/> that contains all of your mod's instances of <see cref="AsyncAssetLoadAttribute"/>, which then can be called using <see cref="ParallelMultiStartCoroutine.Start"/> and all the methods decorated with the attribute will execute, which then can be awaited as needed.
        /// </summary>
        /// <param name="plugin">The plugin to use for the scanning process</param>
        /// <returns>A ParallelMultiStartCoroutine that can be started and then Awaited.</returns>
        public static ParallelMultiStartCoroutine CreateCoroutineForMod(BaseUnityPlugin plugin)
        {
            var instances = SearchableAttribute.GetInstances<AsyncAssetLoadAttribute>();
            if (instances == null)
            {
#if DEBUG
                MSULog.Info($"No instances of AsyncAssetLoadAttribute found in the current runtime context.");
#endif
                return new ParallelMultiStartCoroutine();
            }

            List<AsyncAssetLoadAttribute> attributesForMod = instances.Where(IsValid).Where(att => IsFromMod((AsyncAssetLoadAttribute)att, plugin)).Cast<AsyncAssetLoadAttribute>().ToList();

            return CreateMultiStartCoroutineForMod(attributesForMod);
        }

        private static ParallelMultiStartCoroutine CreateMultiStartCoroutineForMod(List<AsyncAssetLoadAttribute> attributes)
        {
            var result = new ParallelMultiStartCoroutine();
            foreach (var attribute in attributes)
            {
                result.Add((Func<IEnumerator>)attribute.targetMethodInfo.CreateDelegate(typeof(Func<IEnumerator>)));
            }
            return result;
        }

        private static bool IsValid(SearchableAttribute _att)
        {
            if (!(_att is AsyncAssetLoadAttribute attribute))
                return false;

            var methodInfo = attribute.targetMethodInfo;

            if (!methodInfo.IsStatic)
            {
                MSULog.Info($"{attribute} is not applied to a Static method");
            }

            var returnType = methodInfo.ReturnType;
            if (returnType == null || returnType == typeof(void))
            {
#if DEBUG
                MSULog.Info($"{attribute}'s method return type is not IEnumerator");
#endif
                return false;
            }

            if (!returnType.IsSameOrSubclassOf(typeof(IEnumerator)))
            {
#if DEBUG
                MSULog.Info($"{attribute}'s method return type is not IEnumerator");
                return false;
#endif
            }

            var parameters = methodInfo.GetGenericArguments();

            if (parameters.Length != 0)
            {
#if DEBUG
                MSULog.Info($"{attribute}'s method signatures contains parameters, this is not allowed.");
#endif
                return false;
            }

            return true;
        }

        private static bool IsFromMod(AsyncAssetLoadAttribute attribute, BaseUnityPlugin plugin)
        {
            var methodInfo = attribute.targetMethodInfo;

            var declaringType = methodInfo.DeclaringType;
            return declaringType.Assembly == plugin.GetType().Assembly;
        }

        /// <summary>
        /// Returns a readable representation of this AsyncAssetLoadAttribute
        /// </summary>
        public override string ToString()
        {
            return $"AsyncAssetLoadAttribute(TargetMethod={targetMethodInfo.DeclaringType.FullName}.{targetMethodInfo.Name}())";
        }
    }
}
