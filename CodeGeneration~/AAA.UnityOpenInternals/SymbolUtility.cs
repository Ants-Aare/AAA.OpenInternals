using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace AAA.UnityOpenInternals
{
    public static class SymbolUtility
    {
        public static IEnumerable<ITypeSymbol> GetDependenciesFromSymbol(this INamedTypeSymbol typeSymbol)
        {
            foreach (var symbol in typeSymbol.GetMembers())
            {
                switch (symbol)
                {
                    case IEventSymbol eventSymbol:
                        yield return eventSymbol.Type;
                        break;
                    case IFieldSymbol fieldSymbol:
                        yield return fieldSymbol.Type;
                        break;
                    case IMethodSymbol { MethodKind: MethodKind.Ordinary } methodSymbol:
                        yield return methodSymbol.ReturnType;
                        foreach (var parameterType in methodSymbol.Parameters.Select(x => x.Type))
                            yield return parameterType;
                        break;
                    case IPropertySymbol propertySymbol:
                        yield return propertySymbol.Type;
                        break;
                }
            }
        }

        public static IEnumerable<ITypeSymbol> GetRealDependencyTypes(this ITypeSymbol typeSymbol)
        {
            switch (typeSymbol)
            {
                case IArrayTypeSymbol arrayTypeSymbol:
                    foreach (var t in GetRealDependencyTypes(arrayTypeSymbol.ElementType))
                        yield return t;
                    break;
                case INamedTypeSymbol { SpecialType: SpecialType.None, IsGenericType: true } namedTypeSymbol:
                    foreach (var t in namedTypeSymbol.TypeArguments.SelectMany(x => x.GetRealDependencyTypes()))
                        yield return t;
                    break;
                case INamedTypeSymbol { SpecialType: SpecialType.None }:
                    yield return typeSymbol;
                    break;
            }
        }

        private static readonly Dictionary<ITypeSymbol, string> OpenInternalTypeNames = new();

        public static string GetOITypeName(this ITypeSymbol typeSymbol, string overrideName = null)
            => OpenInternalTypeNames.TryGetValue(typeSymbol, out var name)
                ? name
                : OpenInternalTypeNames[typeSymbol] = overrideName ?? $"OI_{typeSymbol.Name}";

        private static readonly Dictionary<ITypeSymbol, string> OpenInternalParameterTypeNames = new();
        public static string GetOIParameterTypeName(this ITypeSymbol typeSymbol)
        {
            if (OpenInternalParameterTypeNames.TryGetValue(typeSymbol, out var name))
                return name;

            var stringBuilder = new StringBuilder(typeSymbol.ToDisplayString());
            foreach (var dependencyType in typeSymbol.GetRealDependencyTypes())
                stringBuilder.Replace(dependencyType.ToDisplayString(), dependencyType.GetOITypeName());
            
            return OpenInternalParameterTypeNames[typeSymbol] = stringBuilder.ToString();
        }
    }
}