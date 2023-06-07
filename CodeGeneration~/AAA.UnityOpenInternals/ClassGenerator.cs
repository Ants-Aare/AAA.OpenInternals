using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static AAA.UnityOpenInternals.StaticUtilitySyntaxes;

namespace AAA.UnityOpenInternals
{
    public static class ClassGenerator
    {
        public static ClassDeclarationSyntax GetClassDeclaration(string sourceAssembly,
            string sourceType,
            string className,
            List<MemberDeclarationSyntax> additionalMembers)
        {
            var classMembers = new MemberDeclarationSyntax[additionalMembers is null ? 5 : additionalMembers.Count + 5];

            for (var i = 0; i < classMembers.Length; i++)
            {
                classMembers[i] = i switch
                {
                    0 => GetOpenInternalType(sourceAssembly, sourceType),
                    1 => OpenInternalMethodInfos,
                    2 => OpenInternalObjectInstance,
                    3 => GetConstructor(className),
                    4 => GetTypeMethod,
                    _ => additionalMembers?[i - 5],
                };
            }

            return ClassDeclaration(className)
                .WithModifiers(PublicPartialModifiers)
                .WithMembers(List(classMembers));
        }

        private static ConstructorDeclarationSyntax GetConstructor(string className)
            => ConstructorDeclaration(Identifier(className))
                .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                .WithParameterList(ParameterList(SingletonSeparatedList(Parameter(Identifier("instance"))
                    .WithType(PredefinedType(Token(SyntaxKind.ObjectKeyword))))))
                .WithBody(CtorBody);

        private static FieldDeclarationSyntax GetOpenInternalType(string sourceAssembly, string sourceType)
            => FieldDeclaration(VariableDeclaration(IdentifierName("Type")).WithVariables(
                    SingletonSeparatedList(VariableDeclarator(Identifier(ConstantStrings.OpenInternalType))
                        .WithInitializer(EqualsValueClause(InvocationExpression(MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                InvocationExpression(IdentifierName("GetTargetAssembly")).WithArgumentList(
                                    ArgumentList(SingletonSeparatedList(
                                        Argument(LiteralExpression(SyntaxKind.StringLiteralExpression,
                                            Literal(sourceAssembly)))))), IdentifierName("GetType")))
                            .WithArgumentList(ArgumentList(SingletonSeparatedList(
                                Argument(LiteralExpression(SyntaxKind.StringLiteralExpression,
                                    Literal(sourceType)))))))))))
                .WithModifiers(PublicStaticReadonlyModifiers);
    }
}