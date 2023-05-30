using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace AAA.UnityOpenInternals
{
    public static class ClassGenerator
    {
        public static SyntaxList<UsingDirectiveSyntax> Usings = List(new[]
        {
            UsingDirective(IdentifierName("System")),
            UsingDirective(QualifiedName(IdentifierName("System"), IdentifierName("Linq"))),
            UsingDirective(QualifiedName(IdentifierName("System"), IdentifierName("Reflection"))),
            UsingDirective(QualifiedName(
                QualifiedName(QualifiedName(IdentifierName("AAA"), IdentifierName("UnityOpenInternals")),
                    IdentifierName("Runtime")), IdentifierName("Extensions"))),
            UsingDirective(QualifiedName(
                    QualifiedName(
                        QualifiedName(QualifiedName(IdentifierName("AAA"), IdentifierName("UnityOpenInternals")),
                            IdentifierName("Runtime")), IdentifierName("Extensions")),
                    IdentifierName("OpenInternalsExtensions")))
                .WithStaticKeyword(Token(SyntaxKind.StaticKeyword))
        });

        public static NamespaceDeclarationSyntax NamespaceDeclaration =
            NamespaceDeclaration(QualifiedName(QualifiedName(IdentifierName("AAA"), IdentifierName("OpenInternals")),
                IdentifierName("Generated")));

        static readonly SyntaxTokenList PublicStaticReadonlyModifiers =
            TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword),
                Token(SyntaxKind.ReadOnlyKeyword));

        static readonly SyntaxTokenList PrivateStaticModifiers =
            TokenList(Token(SyntaxKind.PrivateKeyword), Token(SyntaxKind.StaticKeyword));

        static readonly BlockSyntax CtorBody = Block(SingletonList<StatementSyntax>(ExpressionStatement(
            AssignmentExpression(
                SyntaxKind.SimpleAssignmentExpression, IdentifierName(ConstantStrings.OpenInternalObjectInstance),
                BinaryExpression(SyntaxKind.CoalesceExpression, IdentifierName("instance"),
                    ThrowExpression(ObjectCreationExpression(IdentifierName("NullReferenceException"))
                        .WithArgumentList(ArgumentList(SingletonSeparatedList(
                            Argument(LiteralExpression(SyntaxKind.StringLiteralExpression,
                                Literal("Instance must be specified."))))))))))));


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

        private static readonly FieldDeclarationSyntax OpenInternalMethodInfos
            = FieldDeclaration(
                VariableDeclaration(ArrayType(IdentifierName("MethodInfo")).WithRankSpecifiers(
                        SingletonList(ArrayRankSpecifier(
                            SingletonSeparatedList<ExpressionSyntax>(OmittedArraySizeExpression())))))
                    .WithVariables(SingletonSeparatedList(
                        VariableDeclarator(Identifier(ConstantStrings.OpenInternalMethods))
                            .WithInitializer(EqualsValueClause(InvocationExpression(MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                IdentifierName(ConstantStrings.OpenInternalType),
                                IdentifierName("GetMethods")))))))).WithModifiers(PublicStaticReadonlyModifiers);

        private static readonly MethodDeclarationSyntax GetType
            = MethodDeclaration(IdentifierName("Type"), Identifier("GetType"))
                .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.NewKeyword)))
                .WithExpressionBody(ArrowExpressionClause(IdentifierName(ConstantStrings.OpenInternalType)))
                .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));

        private static readonly FieldDeclarationSyntax OpenInternalObjectInstance = FieldDeclaration(
                VariableDeclaration(PredefinedType(Token(SyntaxKind.ObjectKeyword)))
                    .WithVariables(
                        SingletonSeparatedList(
                            VariableDeclarator(Identifier(ConstantStrings.OpenInternalObjectInstance)))))
            .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.ReadOnlyKeyword)));

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
                    4 => GetType,
                    _ => additionalMembers?[i - 5],
                };
            }

            return ClassDeclaration(className)
                .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.PartialKeyword)))
                .WithMembers(List(classMembers));
        }

        private static ConstructorDeclarationSyntax GetConstructor(string className)
        {
            return ConstructorDeclaration(Identifier(className))
                .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                .WithParameterList(ParameterList(SingletonSeparatedList(Parameter(Identifier("instance"))
                    .WithType(PredefinedType(Token(SyntaxKind.ObjectKeyword))))))
                .WithBody(CtorBody);
        }
    }
}