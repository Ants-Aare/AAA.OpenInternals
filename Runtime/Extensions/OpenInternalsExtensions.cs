using System;
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

        public static MethodInfo GetTargetMethod([NotNull] this MethodInfo[] methodInfos, string methodName, string returnType,
            string[] parameterTypes)
        {
            foreach (var methodInfo in methodInfos)
            {
                var parameters = methodInfo.GetParameters();
                if (parameters.Length != parameterTypes.Length)
                    continue;
                if (methodInfo.Name != methodName)
                    continue;
                if(methodInfo.ReturnType.Name != returnType)
                    continue;
                if (!AreParametersEqual(parameters, parameterTypes))
                    continue;
                    
                return methodInfo;
            }

            throw new NullReferenceException($"Target Method {methodName} is null.");
        }

        public static bool AreParametersEqual(this ParameterInfo[] parameters, string[] parameterTypes)
        {
            for (var i = 0; i < parameters.Length; i++)
            {
                if (parameters[i].ParameterType.FullName != parameterTypes[i])
                    return false;
            }
            return true;
        }
    }
}