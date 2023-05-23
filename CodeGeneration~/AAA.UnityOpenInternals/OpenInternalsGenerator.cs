// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Text;
// using Microsoft.CodeAnalysis;
// using Microsoft.CodeAnalysis.CSharp.Syntax;
// using Microsoft.CodeAnalysis.Text;
//
// namespace AAA.UnityOpenInternals
// {
//     [Generator]
//     public class OpenInternalsGenerator : ISourceGenerator
//     {
//         const string AttributeName = "OpenInternalClass";
//         private readonly StringBuilder _stringBuilder = new StringBuilder();
//
//         public void Initialize(GeneratorInitializationContext context)
//         {
//         }
//
//         public void Execute(GeneratorExecutionContext context)
//         {
//             _stringBuilder.Clear();
//             _stringBuilder.Append(@"
// public partial class UnityOpenPackageDatabase{}
// /*");
//
//             try
//             {
//                 var targetTypes = new HashSet<TypeData>();
//                 GetTargetTypeNames(context, targetTypes);
//                 GetTargetTypes(context, targetTypes);
//                 GetMembersForTargetTypes(targetTypes);
//                 GetDependenciesForTargetTypes(targetTypes);
//
//                 var dependencyTypes = targetTypes
//                     .SelectMany(x => x.DependencyTypes);
//                     // .Distinct()
//                     // .Except()
//                     // .Where(x=> targetTypes.)
//                     // .Select(x => new TypeData(x));
//
//
//                 foreach (var dependencyType in dependencyTypes)
//                 {
//                     _stringBuilder.Append($"{dependencyType.TypeSymbol.ToDisplayString()}: \n\n\n\n");
//                     _stringBuilder.Append(ClassGenerator.GenerateClass(context, dependencyType));
//                 }
//
//                 foreach (var targetTypeData in targetTypes)
//                 {
//                     _stringBuilder.Append($"{targetTypeData.TypeSymbol.ToDisplayString()}: \n\n\n\n");
//                     _stringBuilder.Append(ClassGenerator.GenerateClass(
//                         context: context,
//                         typeData: targetTypeData,
//                         generateFieldFunc: null,
//                         generatePropertyFunc: null,
//                         generateEventFunc: null,
//                         generateMethodFunc: MethodGenerator.GenerateMethod)
//                     );
//                 }
//             }
//             catch (Exception e)
//             {
//                 _stringBuilder.Append(e);
//             }
//
//             _stringBuilder.Append("*/");
//             context.AddSource("UnityOpenPackageDatabase", SourceText.From(_stringBuilder.ToString(), Encoding.UTF8));
//         }
//
//         private void GetDependenciesForTargetTypes(HashSet<TypeData> typeDatas)
//         {
//             foreach (var typeData in typeDatas)
//             {
//                 foreach (var methodData in typeData.MethodDatas)
//                 {
//                     typeData.TryAddDependencyType(methodData.MethodSymbol.ReturnType);
//                     foreach (var typeSymbol in methodData.MethodSymbol.Parameters.Select(x => x.Type))
//                     {
//                         typeData.TryAddDependencyType(typeSymbol);
//                     }
//                 }
//
//                 foreach (var propertyData in typeData.PropertyDatas)
//                 {
//                     typeData.TryAddDependencyType(propertyData.PropertySymbol.Type);
//                 }
//
//                 foreach (var fieldData in typeData.FieldDatas)
//                 {
//                     typeData.TryAddDependencyType(fieldData.FieldSymbol.Type);
//                 }
//
//                 foreach (var eventData in typeData.EventDatas)
//                 {
//                     typeData.TryAddDependencyType(eventData.EventSymbol.Type);
//                 }
//             }
//         }
//
//         private void GetTargetTypes(GeneratorExecutionContext context, HashSet<TypeData> targetTypeNames)
//         {
//             var typesByNamespaceDictionary = new Dictionary<string, INamedTypeSymbol[]>();
//             var foundNamespaces = new Dictionary<string, INamespaceSymbol>();
//
//             foreach (var targetTypeData in targetTypeNames)
//             {
//                 if (!typesByNamespaceDictionary.TryGetValue(targetTypeData.SourceNamespaceName, out var allTypes))
//                 {
//                     if (!foundNamespaces.TryGetValue(targetTypeData.SourceNamespaceName, out var namespaceSymbol))
//                     {
//                         namespaceSymbol = context.GetNamespaceByName(targetTypeData.SourceNamespaceName);
//                         foundNamespaces.Add(targetTypeData.SourceNamespaceName, namespaceSymbol);
//                     }
//
//                     allTypes = namespaceSymbol.GetAllTypes().ToArray();
//                     typesByNamespaceDictionary.Add(targetTypeData.SourceNamespaceName, allTypes);
//                 }
//
//                 var typeSymbol = allTypes.FirstOrDefault(x => x.Name == targetTypeData.SourceTypeName);
//                 targetTypeData.TypeSymbol = typeSymbol;
//             }
//         }
//
//         private void GetMembersForTargetTypes(HashSet<TypeData> targetTypes)
//         {
//             foreach (var targetType in targetTypes)
//             {
//                 foreach (var symbol in targetType.TypeSymbol.GetMembers())
//                 {
//                     switch (symbol)
//                     {
//                         case IEventSymbol eventSymbol:
//                             targetType.EventDatas.Add(new EventData(eventSymbol));
//                             break;
//                         case IFieldSymbol fieldSymbol:
//                             targetType.FieldDatas.Add(new FieldData(fieldSymbol));
//                             break;
//                         case IMethodSymbol methodSymbol:
//                             if (methodSymbol.MethodKind != MethodKind.Ordinary)
//                                 continue;
//                             targetType.MethodDatas.Add(new MethodData(methodSymbol));
//                             break;
//                         case IPropertySymbol propertySymbol:
//                             targetType.PropertyDatas.Add(new PropertyData(propertySymbol));
//                             break;
//                         default:
//                             _stringBuilder.Append($"Unexpected member found in {targetType.FullSourceTypeName}: ({symbol.GetType()}) {symbol.ToDisplayString()}");
//                             break;
//                     }
//                 }
//             }
//         }
//
//         private static void GetTargetTypeNames(GeneratorExecutionContext context,
//             HashSet<TypeData> targetTypeDatas)
//         {
//             // var targetTypes = new HashSet<string>();
//             var syntaxTrees = context.Compilation.SyntaxTrees;
//             foreach (var syntaxTree in syntaxTrees)
//             {
//                 var descendantNodes = syntaxTree.GetRoot().DescendantNodes();
//                 foreach (var node in descendantNodes)
//                 {
//                     if (!(node is BaseTypeDeclarationSyntax typeDeclaration))
//                         continue;
//
//                     var attribute = AnalysisUtility.GetAttributeOrNull(typeDeclaration, AttributeName);
//                     if (attribute == null)
//                         continue;
//
//                     var argument = attribute.ArgumentList?.Arguments.First();
//                     var name = argument?.GetText().ToString().Replace(@"""", String.Empty);
//                     targetTypeDatas.Add(new TypeData(name));
//                 }
//             }
//         }
//     }
// }