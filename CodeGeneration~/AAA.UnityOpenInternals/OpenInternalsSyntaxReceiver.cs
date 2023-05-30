using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AAA.UnityOpenInternals
{
    public class OpenInternalsSyntaxReceiver : ISyntaxReceiver
    {
        public readonly Dictionary<string, string> Attributes = new();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is not AttributeSyntax
                {
                    Name: IdentifierNameSyntax { Identifier: { Text: "OpenInternalClass" } },
                    ArgumentList: { Arguments: { } attributeArguments }
                } attributeSyntax)
                return;

            var targetTypeName = (attributeArguments.FirstOrDefault()?.Expression as LiteralExpressionSyntax)
                ?.Token.ValueText;

            if (targetTypeName == null)
                return;

            var classNameSyntax = attributeSyntax.Parent?
                .Parent?
                .ChildTokens()
                .FirstOrDefault(x => x.Kind() == SyntaxKind.IdentifierToken);

            var className = classNameSyntax?.ValueText ?? ""; 
            
            Attributes[targetTypeName] = className;
        }
    }
}