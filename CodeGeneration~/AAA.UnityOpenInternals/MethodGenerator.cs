using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace AAA.UnityOpenInternals
{
    public static class MethodGenerator
    {
        private static int _index = 0;

        private static InvocationExpressionSyntax GetEmptyArraySyntax(TypeSyntax typeSyntax)
            => InvocationExpression(
                MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, IdentifierName("Array"),
                    GenericName(Identifier("Empty"))
                        .WithTypeArgumentList(
                            TypeArgumentList(SingletonSeparatedList(typeSyntax)))));

        public static void AddMethodToSyntaxList(List<MemberDeclarationSyntax> list, IMethodSymbol methodSymbol)
        {
            var methodInfoFieldName = $"{methodSymbol.Name}MethodInfo{_index++.ToString()}";
            var returnTypeName = methodSymbol.ReturnType.GetOIParameterTypeName();

            var assignmentParameterSyntaxList = GetParameterSyntaxes<ExpressionSyntax>(methodSymbol,
                symbol => InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                    IdentifierName(symbol.Name), IdentifierName("GetType"))));


            var returnParameterSyntaxList = GetParameterSyntaxes<ExpressionSyntax>(methodSymbol,
                symbol =>
                    symbol.Type switch
                    {
                        IArrayTypeSymbol arrayTypeSymbol => TypeOfExpression(IdentifierName(symbol.Type.Name)),
                        INamedTypeSymbol { SpecialType: SpecialType.None } namedTypeSymbol
                            => MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                IdentifierName(symbol.Name),
                                IdentifierName(ConstantStrings.OpenInternalObjectInstance)),
                        INamedTypeSymbol => TypeOfExpression(IdentifierName(symbol.Type.ToDisplayString())),
                        _ => throw new ArgumentOutOfRangeException()
                    }
            );

            var assignmentStatement = ExpressionStatement(AssignmentExpression(SyntaxKind.CoalesceAssignmentExpression,
                IdentifierName(methodInfoFieldName),
                InvocationExpression(
                        MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName(ConstantStrings.OpenInternalMethods),
                            IdentifierName("GetTargetMethod")))
                    .WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]
                    {
                        Argument(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(methodSymbol.Name))),
                        Token(SyntaxKind.CommaToken),
                        Argument(LiteralExpression(SyntaxKind.StringLiteralExpression,
                            Literal(methodSymbol.ReturnType.ToDisplayString()))),
                        Token(SyntaxKind.CommaToken),
                        Argument(methodSymbol.Parameters.Length == 0
                            ? GetEmptyArraySyntax(IdentifierName("Type"))
                            : ArrayCreationExpression(ArrayType(IdentifierName("Type")).WithRankSpecifiers(
                                    SingletonList(
                                        ArrayRankSpecifier(
                                            SingletonSeparatedList<ExpressionSyntax>(
                                                OmittedArraySizeExpression())))))
                                .WithInitializer(InitializerExpression(SyntaxKind.ArrayInitializerExpression,
                                    assignmentParameterSyntaxList)))
                    })))));


            var invocationExpression = InvocationExpression(MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    IdentifierName(methodInfoFieldName), IdentifierName("Invoke")))
                .WithArgumentList(
                    ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]
                    {
                        Argument(IdentifierName(ConstantStrings.OpenInternalObjectInstance)),
                        Token(SyntaxKind.CommaToken),
                        Argument(methodSymbol.Parameters.Length == 0
                            ? GetEmptyArraySyntax(PredefinedType(Token(SyntaxKind.ObjectKeyword)))
                            : ArrayCreationExpression(ArrayType(PredefinedType(Token(SyntaxKind.ObjectKeyword)))
                                    .WithRankSpecifiers(
                                        SingletonList(
                                            ArrayRankSpecifier(
                                                SingletonSeparatedList<ExpressionSyntax>(
                                                    OmittedArraySizeExpression())))))
                                .WithInitializer(InitializerExpression(SyntaxKind.ArrayInitializerExpression,
                                    returnParameterSyntaxList)))
                    })));

            var returnTypeSyntax = IdentifierName(returnTypeName);
            StatementSyntax returnStatement = methodSymbol.ReturnType switch
            {
                { SpecialType: SpecialType.System_Void } => ExpressionStatement(invocationExpression),
                { SpecialType: SpecialType.System_Collections_IEnumerable } => ReturnStatement(InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                        ParenthesizedExpression(CastExpression(IdentifierName("IEnumerable"), invocationExpression)),
                        IdentifierName("Select"))).WithArgumentList(ArgumentList(SingletonSeparatedList(
                        Argument(SimpleLambdaExpression(Parameter(Identifier("x"))).WithExpressionBody(
                            ObjectCreationExpression(returnTypeSyntax).WithArgumentList(ArgumentList(
                                SingletonSeparatedList(Argument(
                                    IdentifierName("x"))))))))))),
                
                { SpecialType: SpecialType.System_Array } => ReturnStatement(InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression, InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                    ParenthesizedExpression(CastExpression(IdentifierName("IEnumerable"), invocationExpression)),
                    IdentifierName("Select"))).WithArgumentList(ArgumentList(SingletonSeparatedList(
                    Argument(SimpleLambdaExpression(Parameter(Identifier("x"))).WithExpressionBody(
                        ObjectCreationExpression(returnTypeSyntax).WithArgumentList(ArgumentList(
                            SingletonSeparatedList(Argument(
                                IdentifierName("x")))))))))),IdentifierName("ToArray")))),

                { SpecialType: SpecialType.None } => ReturnStatement(ObjectCreationExpression(returnTypeSyntax)
                    .WithArgumentList(ArgumentList(SingletonSeparatedList(Argument(invocationExpression))))),
                _ => ReturnStatement(CastExpression(returnTypeSyntax, invocationExpression))
            };

            var parameterSyntaxes = GetParameterSyntaxes(methodSymbol,
                symbol => Parameter(Identifier(symbol.Name))
                    .WithType(IdentifierName(symbol.Type.GetOIParameterTypeName())));

            var fieldDeclaration = FieldDeclaration(VariableDeclaration(IdentifierName("MethodInfo"))
                    .WithVariables(SingletonSeparatedList(VariableDeclarator(Identifier(methodInfoFieldName)))))
                .WithModifiers(TokenList(Token(SyntaxKind.PrivateKeyword)));

            var methodDeclaration =
                MethodDeclaration(returnTypeSyntax, Identifier(methodSymbol.Name))
                    .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                    .WithParameterList(ParameterList(parameterSyntaxes))
                    .WithBody(Block(assignmentStatement, returnStatement));

            list.Add(fieldDeclaration);
            list.Add(methodDeclaration);
        }

        private static SeparatedSyntaxList<T> GetParameterSyntaxes<T>(IMethodSymbol methodSymbol,
            Func<IParameterSymbol, T> onParameterAdded)
            where T : SyntaxNode
        {
            var arrayLength = (methodSymbol.Parameters.Length * 2) - 1;
            if (arrayLength <= 0)
                return SeparatedList<T>();

            var array = new SyntaxNodeOrToken[arrayLength];
            for (var i = 0; i < arrayLength; i++)
            {
                array[i] = i % 2 == 0
                    ? onParameterAdded?.Invoke(methodSymbol.Parameters[i / 2])
                    : Token(SyntaxKind.CommaToken);
            }

            return SeparatedList<T>(array);
        }
    }
}