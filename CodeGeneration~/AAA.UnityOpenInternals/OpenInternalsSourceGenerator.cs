using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

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
            StringBuilder.Append(@"public partial class UnityOpenPackageDatabase{}\n/*\n");
            try
            {
                if (context.SyntaxReceiver is not OpenInternalsSyntaxReceiver syntaxReceiver)
                    return;

                foreach (var dictionaryEntry in syntaxReceiver.Attributes)
                {
                    StringBuilder.Append($"{dictionaryEntry.Key}: {dictionaryEntry.Value.ToFullString()}\n");
                }

                INamedTypeSymbol typeSymbol = context.Compilation.GetTypeByMetadataName("GetComponentAttribute");
            }
            catch (Exception e)
            {
                StringBuilder.Append(e);
                throw;
            }

            StringBuilder.Append("*/");
            context.AddSource("UserClass.Generated.cs",
                SourceText.From(StringBuilder.ToString(), Encoding.UTF8));
        }
    }

    public class OpenInternalsSyntaxReceiver : ISyntaxReceiver
    {
        public readonly Dictionary<string, AttributeSyntax> Attributes = new Dictionary<string, AttributeSyntax>();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (!(syntaxNode is AttributeSyntax
                {
                    Name: IdentifierNameSyntax { Identifier: { Text: "OpenInternalClass" } },
                    ArgumentList: { Arguments: { } attributeArguments }
                } attributeSyntax))
                return;

            var targetTypeName = (attributeArguments.FirstOrDefault()?.Expression as LiteralExpressionSyntax)
                ?.Token.ValueText;

            if (targetTypeName == null)
                return;

            Attributes[targetTypeName] = attributeSyntax;
        }
    }
}