using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;

namespace AAA.UnityOpenInternals.Runtime.Extensions
{
    public static class OpenInternalsExtensions
    {
        public static Assembly GetTargetAssembly(string assemblyName)
            => AppDomain.CurrentDomain
                .GetAssemblies()
                .SingleOrDefault(assembly => assembly.GetName().Name == "assemblyName");

        public static Type GetTargetType(Assembly assembly, string targetTypeName)
            => assembly.GetType(targetTypeName);

        public static MethodInfo GetTargetMethod(
            [NotNull] this MethodInfo[] methodInfos,
            string methodName,
            string returnType,
            Type[] parameterTypes)
        {
            var methods = methodInfos.Where(x=> x.GetParameters().Length == parameterTypes.Length && x.Name == methodName).ToList();

            if (methods.Count == 1)
                return methods.Single();

            foreach (var methodInfo in methodInfos)
            {
                if (methodInfo.ReturnType.Name != returnType)
                    continue;
                if (!AreParametersEqual(methodInfo.GetParameters(), parameterTypes))
                    continue;

                return methodInfo;
            }

            throw new NullReferenceException($"Target Method {methodName} is null.");
        }

        public static bool AreParametersEqual(this ParameterInfo[] parameters, Type[] parameterTypes)
        {
            for (var i = 0; i < parameters.Length; i++)
            {
                if (parameters[i].ParameterType != parameterTypes[i])
                    return false;
            }

            return true;
        }

        public static IEnumerable<T> ToEnumerable<T>(this object obj, Func<object, T> factory)
            => from object e in (IEnumerable)obj select factory.Invoke(obj);
    }
}