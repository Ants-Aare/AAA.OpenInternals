// using System.Linq;
// using System.Text;
// using Microsoft.CodeAnalysis;
//
// namespace AAA.UnityOpenInternals
// {
//     public static class MethodGenerator
//     {
//         private const string MethodTemplate = @"
//         MethodInfo $[METHODNAME]MethodInfo$[INDEX];
//         $[ACCESSIBILITY] $[ASYNC]$[RETURNTYPE] $[METHODNAME]($[PARAMETERS])
//         {$[BODY]
//         }
// ";
//
//         private const string BodyTemplate = @"
//             var targetMethod = $[METHODNAME]MethodInfo$[INDEX] ?? _methods.GetTargetMethod(methodName: ""$[METHODNAME]"", returnType: ""$[RETURNTYPE]"",parameterTypes: new Type[] {$[PARAMETERS]});
// ";
//
//         private static int _methodIndex;
//
//         public static string GenerateMethod(MethodData methodData)
//         {
//             var methodSymbol = methodData.MethodSymbol;
//
//             var parameterSymbols = methodSymbol.Parameters;
//             var parameterDeclarations = new StringBuilder();
//             var parametersAsTypeNames = new StringBuilder();
//             var parametersAsObjects = new StringBuilder();
//
//             for (var i = 0; i < parameterSymbols.Length; i++)
//             {
//                 var parameterSymbol = parameterSymbols[i];
//                 // var modifiers = Enumerable.Aggregate(parameterSymbol.CustomModifiers, "",
//                 //     (current, modifier) => current + $"{modifier.Modifier.ToDisplayString()} ");
//
//                 var typeName = parameterSymbol.Type.ToDisplayString();
//                 var parameterDeclaration = $"{typeName} {parameterSymbol.Name}";
//
//                 if (i != 0)
//                     parameterDeclarations.Append(", ");
//                 parameterDeclarations.Append(parameterDeclaration);
//
//                 parametersAsTypeNames.Append($@"""{parameterSymbol.Type.ToDisplayString()}"", ");
//                 parametersAsObjects.Append($@"{parameterSymbol.Name}.OpenInternalObjectInstance, ");
//             }
//
//
//             var accessibility = methodSymbol.DeclaredAccessibility.ToString().ToLowerInvariant();
//             var asyncKeyword = methodSymbol.IsAsync ? "async " : string.Empty;
//             var returnType = methodSymbol.ReturnType.ToDisplayString();
//
//             var methodInvocation = $@"targetMethod.Invoke(OpenInternalObjectInstance, new object[] {{{parametersAsObjects}}})";
//
//             var returnStatement = methodSymbol.ReturnsVoid
//                 ? $"{methodInvocation};"
//                 : methodSymbol.ReturnType.SpecialType == SpecialType.None
//                     ? $"return ({returnType}) {methodInvocation};"
//                     : $"return new {returnType}({methodInvocation});";
//
//             _methodIndex++;
//             var body = GenerateMethodBody(
//                 returnType: methodSymbol.ReturnType.Name,
//                 parameters: parametersAsTypeNames.ToString(),
//                 returnStatement: returnStatement);
//
//             var method = GenerateFullMethod(
//                 body: body.ToString(),
//                 methodName: methodSymbol.Name,
//                 accessibility: accessibility,
//                 asyncKeyword: asyncKeyword,
//                 returnType: returnType,
//                 parameters: parameterDeclarations.ToString());
//
//             return method.ToString();
//         }
//
//         private static StringBuilder GenerateFullMethod(string body,
//             string methodName,
//             string accessibility,
//             string asyncKeyword,
//             string returnType,
//             string parameters)
//         {
//             return new StringBuilder(MethodTemplate)
//                 .Replace("$[BODY]", body)
//                 .Replace("$[METHODNAME]", methodName)
//                 .Replace("$[INDEX]", _methodIndex.ToString())
//                 .Replace("$[ACCESSIBILITY]", accessibility)
//                 .Replace("$[ASYNC]", asyncKeyword)
//                 .Replace("$[RETURNTYPE]", returnType)
//                 .Replace("$[PARAMETERS]", parameters);
//         }
//
//         private static StringBuilder GenerateMethodBody(string returnType, string parameters, string returnStatement) =>
//             new StringBuilder(BodyTemplate)
//                 .Replace("$[RETURNTYPE]", returnType)
//                 .Replace("$[PARAMETERS]", parameters)
//                 .Replace("$[RETURNSTATEMENT]", returnStatement);
//     }
// }