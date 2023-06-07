using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static AAA.UnityOpenInternals.DiagnosticWarnings;
using static AAA.UnityOpenInternals.StaticUtilitySyntaxes;

namespace AAA.UnityOpenInternals
{
    [Generator]
    public class OpenInternalsSourceGenerator : ISourceGenerator
    {
        public static readonly StringBuilder StringBuilder = new StringBuilder();

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new OpenInternalsSyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            StringBuilder.Clear();
            // StringBuilder.Append("public partial class UnityOpenPackageDatabase{}\n");
            StringBuilder.Append("/*\n");

            try
            {
                if (context.SyntaxReceiver is not OpenInternalsSyntaxReceiver syntaxReceiver)
                    return;

                RunGenerator(context, syntaxReceiver);
            }
            catch (Exception e)
            {
                StringBuilder.Append($"\n \n \n{e}");
            }

            StringBuilder.Append("*/");
            context.AddSource("UserClass.Generated.cs", SourceText.From(StringBuilder.ToString(), Encoding.UTF8));
        }

        private void RunGenerator(GeneratorExecutionContext context, OpenInternalsSyntaxReceiver syntaxReceiver)
        {
            var typeSymbols = new List<ITypeSymbol>();
            var dependencyTypes = new HashSet<ITypeSymbol>();

            foreach (var valuePair in syntaxReceiver.Attributes)
            {
                var typeSymbol = context.Compilation.GetTypeByMetadataName(valuePair.Key);
                switch (typeSymbol)
                {
                    case null:
                        context.ReportDiagnostic(
                            Diagnostic.Create(InvalidTypeNameWarning, Location.None, valuePair.Key));
                        StringBuilder.Append($"InvalidTypeNameWarning {valuePair.Key}");
                        continue;
                    case { DeclaredAccessibility: >= Accessibility.Public }:
                        context.ReportDiagnostic(Diagnostic.Create(TypeNotInternalWarning, Location.None,
                            typeSymbol.ToDisplayString(), typeSymbol.DeclaredAccessibility));
                        StringBuilder.Append(
                            $"Type not internal: {typeSymbol.ToDisplayString()}. Type Accessibility: {typeSymbol.DeclaredAccessibility}");
                        continue;
                    default:
                        typeSymbols.Add(typeSymbol);
                        var dependencies = typeSymbol.GetDependenciesFromSymbol()
                            .SelectMany(x => x.GetRealDependencyTypes());
                        dependencyTypes.UnionWith(dependencies);
                        AddClassToSource(context, typeSymbol, true, valuePair.Value);
                        continue;
                }
            }

            dependencyTypes.ExceptWith(typeSymbols);
            foreach (var dependencyType in dependencyTypes)
            {
                AddClassToSource(context, dependencyType);
            }
        }

        private void AddClassToSource(GeneratorExecutionContext context, ITypeSymbol typeSymbol,
            bool generateMembers = false, string targetName = null)
        {
            var list = new List<MemberDeclarationSyntax>();
            var className = typeSymbol.GetOITypeName(targetName);

            if (generateMembers)
            {
                foreach (var member in typeSymbol.GetMembers())
                {
                    switch (member)
                    {
                        case IFieldSymbol fieldSymbol:
                            break;
                        case IPropertySymbol propertySymbol:
                            break;
                        case IMethodSymbol { MethodKind: MethodKind.Constructor } constructorSymbol:
                            break;
                        case IMethodSymbol { MethodKind: MethodKind.Ordinary, IsGenericMethod: false} methodSymbol:
                            MethodGenerator.AddMethodToSyntaxList(list, methodSymbol);
                            break;
                    }
                }
            }

            var classDeclaration = ClassGenerator.GetClassDeclaration(
                sourceAssembly: typeSymbol.ContainingAssembly.Name,
                sourceType: typeSymbol.ToDisplayString(),
                className: className,
                additionalMembers: list
            );

            var compilationUnit = CompilationUnit()
                .WithUsings(Usings)
                .WithMembers(SingletonList<MemberDeclarationSyntax>(classDeclaration))
                .WithLeadingTrivia(TriviaList(
                    new[]
                    {
                        Comment("// <auto-generated>"),
                        Comment($"//     This code was generated by {GetType().FullName}"),
                        Comment(
                            "//     Changes to this file may cause incorrect behavior and will be lost if the code is regenerated."),
                        Comment("// </auto-generated>")
                    }))
                .NormalizeWhitespace();

            // StringBuilder.Append(compilationUnit.ToString());
            context.AddSource($"{className}.Generated.cs", compilationUnit.GetText(Encoding.UTF8));
        }
    }
}