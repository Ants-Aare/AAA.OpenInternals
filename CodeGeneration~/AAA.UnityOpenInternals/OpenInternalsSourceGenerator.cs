using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.VisualBasic;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace AAA.UnityOpenInternals
{
    [Generator]
    public class OpenInternalsSourceGenerator : ISourceGenerator
    {
        public static readonly StringBuilder StringBuilder = new StringBuilder();

        private static readonly DiagnosticDescriptor InvalidTypeNameWarning = new DiagnosticDescriptor(id: "OPENGEN001",
            title: "Couldn't find Type",
            messageFormat: "Couldn't find Full Type Name based on 'OpenInternalClass' attribute with parameter '{0}'.",
            category: "OpenInternalsGenerator",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new OpenInternalsSyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            StringBuilder.Clear();
            StringBuilder.Append("public partial class UnityOpenPackageDatabase{}\n/*\n");

            try
            {
                if (context.SyntaxReceiver is not OpenInternalsSyntaxReceiver syntaxReceiver)
                    return;

                var typeSymbols = new List<ITypeSymbol>();
                var dependencyTypes = new HashSet<ITypeSymbol>();

                foreach (var valuePair in syntaxReceiver.Attributes)
                {
                    var typeSymbol = context.Compilation.GetTypeByMetadataName(valuePair.Key);
                    if (typeSymbol is null)
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(InvalidTypeNameWarning, Location.None, valuePair.Key));
                        continue;
                    }
                    typeSymbols.Add(typeSymbol);
                    var dependencies = typeSymbol.GetDependenciesFromSymbol()
                        .SelectMany(x => x.GetRealDependencyTypes());

                    dependencyTypes.UnionWith(dependencies);

                    // ((INamedTypeSymbol)typeSymbol).DeclaredAccessibility == Accessibility.Internal
                    AddClassToSource(context, typeSymbol, true, valuePair.Value);
                }
    
                dependencyTypes.ExceptWith(typeSymbols);
                foreach (var dependencyType in dependencyTypes)
                {
                    AddClassToSource(context, dependencyType);
                }
            }
            catch (Exception e)
            {
                StringBuilder.Append($"\n \n \n{e}");
            }

            StringBuilder.Append("*/");
            context.AddSource("UserClass.Generated.cs", SourceText.From(StringBuilder.ToString(), Encoding.UTF8));
        }

        private static void AddClassToSource(GeneratorExecutionContext context, ITypeSymbol typeSymbol,
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
                        case IMethodSymbol { MethodKind: MethodKind.Ordinary } methodSymbol:
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
                .WithUsings(ClassGenerator.Usings)
                .WithMembers(SingletonList<MemberDeclarationSyntax>(classDeclaration))
                .NormalizeWhitespace();

            context.AddSource($"{className}.Generated.cs", compilationUnit.GetText(Encoding.UTF8));
        }
    }
}