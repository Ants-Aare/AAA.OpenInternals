using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace AAA.UnityOpenInternals
{
    public static class StaticUtilitySyntaxes
    {
        public static SyntaxList<UsingDirectiveSyntax> Usings = List(new[]
        {
            UsingDirective(IdentifierName("System")),
            UsingDirective(QualifiedName(IdentifierName("System"), IdentifierName("Collections"))),
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

        public static readonly SyntaxTokenList PublicStaticReadonlyModifiers =
            TokenList(Token(SyntaxKind.PublicKeyword),
                Token(SyntaxKind.StaticKeyword),
                Token(SyntaxKind.ReadOnlyKeyword));

        public static readonly SyntaxTokenList PrivateStaticModifiers =
            TokenList(Token(SyntaxKind.PrivateKeyword), Token(SyntaxKind.StaticKeyword));
        
        public static readonly SyntaxTokenList PublicPartialModifiers =
            TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.PartialKeyword));

        public static readonly BlockSyntax CtorBody = Block(SingletonList<StatementSyntax>(ExpressionStatement(
            AssignmentExpression(
                SyntaxKind.SimpleAssignmentExpression, IdentifierName(ConstantStrings.OpenInternalObjectInstance),
                BinaryExpression(SyntaxKind.CoalesceExpression, IdentifierName("instance"),
                    ThrowExpression(ObjectCreationExpression(IdentifierName("NullReferenceException"))
                        .WithArgumentList(ArgumentList(SingletonSeparatedList(
                            Argument(LiteralExpression(SyntaxKind.StringLiteralExpression,
                                Literal("Instance must be specified."))))))))))));

        public static readonly FieldDeclarationSyntax OpenInternalMethodInfos
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

        public static readonly MethodDeclarationSyntax GetTypeMethod
            = MethodDeclaration(IdentifierName("Type"), Identifier("GetType"))
                .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.NewKeyword)))
                .WithExpressionBody(ArrowExpressionClause(IdentifierName(ConstantStrings.OpenInternalType)))
                .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));

        public static readonly FieldDeclarationSyntax OpenInternalObjectInstance = FieldDeclaration(
                VariableDeclaration(PredefinedType(Token(SyntaxKind.ObjectKeyword)))
                    .WithVariables(
                        SingletonSeparatedList(
                            VariableDeclarator(Identifier(ConstantStrings.OpenInternalObjectInstance)))))
            .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.ReadOnlyKeyword)));
        
        public static InvocationExpressionSyntax GetEmptyArraySyntax(TypeSyntax typeSyntax)
            => InvocationExpression(
                MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, IdentifierName("Array"),
                    GenericName(Identifier("Empty"))
                        .WithTypeArgumentList(
                            TypeArgumentList(SingletonSeparatedList(typeSyntax)))));
    }
}