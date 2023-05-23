// using System.Linq;
// using Microsoft.CodeAnalysis.CSharp.Syntax;
//
// namespace AAA.UnityOpenInternals
// {
//     public class AnalysisUtility
//     {
//         public static AttributeSyntax GetAttributeOrNull(BaseTypeDeclarationSyntax component, string attributeName)
//         {
//             return component.AttributeLists.SelectMany(x => x.Attributes)
//                 .FirstOrDefault(syntax => syntax.Name.ToString() == attributeName);
//         }
//
//         public static string GetNamespace(BaseTypeDeclarationSyntax syntax)
//         {
//             string nameSpace = string.Empty;
//
//             var potentialNamespaceParent = syntax.Parent;
//
//             while (potentialNamespaceParent != null && !(potentialNamespaceParent is NamespaceDeclarationSyntax))
//             {
//                 potentialNamespaceParent = potentialNamespaceParent.Parent;
//             }
//
//             if (potentialNamespaceParent is NamespaceDeclarationSyntax namespaceParent)
//             {
//                 nameSpace = namespaceParent.Name.ToString();
//
//                 while (true)
//                 {
//                     if (!(namespaceParent.Parent is NamespaceDeclarationSyntax parent))
//                     {
//                         break;
//                     }
//
//                     nameSpace = $"{namespaceParent.Name}.{nameSpace}";
//                     namespaceParent = parent;
//                 }
//             }
//
//             return nameSpace;
//         }
//     }
// }