// using System;
// using System.Collections.Generic;
// using System.Linq;
// using Microsoft.CodeAnalysis;
//
// namespace AAA.UnityOpenInternals
// {
//     public static class SymbolUtility
//     {
//         private static INamespaceSymbol[] _allNamespaces;
//         public static INamespaceSymbol GetNamespaceByName(this GeneratorExecutionContext context, string namespaceName)
//         {
//             if (_allNamespaces == null)
//                 _allNamespaces = context.Compilation.GlobalNamespace.GetAllNameSpaces().ToArray();
//             var foundNamespace = _allNamespaces.FirstOrDefault(x => x.ToDisplayString() == namespaceName);
//             if (foundNamespace == null)
//                 throw new NullReferenceException($"No Namespace with name {namespaceName} was found.");
//             return foundNamespace;
//         }
//         
//         public static IEnumerable<INamedTypeSymbol> GetAllTypes(this INamespaceSymbol namespaceSymbol)
//         {
//             foreach (var type in namespaceSymbol.GetTypeMembers().Where(type => !type.IsAnonymousType))
//             {
//                 yield return type;
//             }
//
//             foreach (var nestedNamespace in namespaceSymbol.GetNamespaceMembers())
//             {
//                 foreach (var type in GetAllTypes(nestedNamespace))
//                 {
//                     yield return type;
//                 }
//             }
//         }
//
//         public static IEnumerable<INamespaceSymbol> GetAllNameSpaces(this INamespaceSymbol namespaceSymbol)
//         {
//             if (namespaceSymbol == null)
//                 yield break;
//
//             yield return namespaceSymbol;
//
//             foreach (var namespaceMember in namespaceSymbol.GetNamespaceMembers())
//             {
//                 foreach (var t in GetAllNameSpaces(namespaceMember))
//                 {
//                     yield return t;
//                 }
//             }
//         }
//     }
// }