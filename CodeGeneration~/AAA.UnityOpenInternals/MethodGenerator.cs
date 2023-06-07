using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static AAA.UnityOpenInternals.StaticUtilitySyntaxes;

namespace AAA.UnityOpenInternals
{
    public static class MethodGenerator
    {
        private static int _index;

        public static void AddMethodToSyntaxList(List<MemberDeclarationSyntax> list, IMethodSymbol methodSymbol)
        {
            var methodInfoFieldName = $"{methodSymbol.Name}MethodInfo{_index++.ToString()}";
            var methodInfoIdentifier = IdentifierName(methodInfoFieldName);
            var returnTypeName = methodSymbol.ReturnType.GetOIParameterTypeName();
            var returnTypeIdentifier = IdentifierName(returnTypeName);

            var fieldDeclaration = GetMethodInfoField(methodInfoFieldName);

            var assignmentStatement = GetAssignmentStatementSyntax(methodSymbol, methodInfoIdentifier);
            var invocationExpression = GetMethodInvocationSyntax(methodSymbol, methodInfoIdentifier);

            var returnStatement = GetReturnStatementSyntax(methodSymbol, invocationExpression, returnTypeIdentifier);
            var body = Block(assignmentStatement, returnStatement);

            var methodDeclaration = GetMethodDeclarationSyntax(methodSymbol, returnTypeIdentifier, body);

            list.Add(fieldDeclaration);
            list.Add(methodDeclaration);
        }

        private static FieldDeclarationSyntax GetMethodInfoField(string methodInfoFieldName)
        {
            return FieldDeclaration(VariableDeclaration(IdentifierName("MethodInfo"))
                    .WithVariables(SingletonSeparatedList(VariableDeclarator(Identifier(methodInfoFieldName)))))
                .WithModifiers(TokenList(Token(SyntaxKind.PrivateKeyword)));
        }

        private static MethodDeclarationSyntax GetMethodDeclarationSyntax(IMethodSymbol methodSymbol,
            IdentifierNameSyntax returnTypeIdentifier, BlockSyntax body)
        {
            var parameterSyntaxes = GetParameterSyntaxes(methodSymbol,
                symbol => Parameter(Identifier(symbol.Name))
                    .WithType(IdentifierName(symbol.Type.GetOIParameterTypeName())));

            return MethodDeclaration(returnTypeIdentifier, Identifier(methodSymbol.Name))
                .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                .WithParameterList(ParameterList(parameterSyntaxes))
                .WithBody(body);
        }

        private static StatementSyntax GetReturnStatementSyntax(IMethodSymbol methodSymbol,
            InvocationExpressionSyntax invocationExpression, IdentifierNameSyntax returnTypeIdentifier)
        {
            switch (methodSymbol.ReturnType)
            {
                case { SpecialType: SpecialType.System_Void }:
                    return ExpressionStatement(invocationExpression);
                case INamedTypeSymbol
                    {
                        SpecialType: SpecialType.System_Collections_Generic_IEnumerable_T
                        or SpecialType.System_Collections_IEnumerable
                    } namedTypeSymbol
                    when !namedTypeSymbol.IsPurelyPublicTypes():
                    return ReturnStatement(
                        InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                invocationExpression, IdentifierName("ToEnumerable")))
                            .WithArgumentList(ArgumentList(SingletonSeparatedList(Argument(
                                SimpleLambdaExpression(Parameter(Identifier("x")))
                                    .WithExpressionBody(ObjectCreationExpression(IdentifierName(namedTypeSymbol.TypeArguments.First().GetOITypeName()))
                                        .WithArgumentList(
                                            ArgumentList(SingletonSeparatedList(Argument(IdentifierName("x")))))))))));
                case IArrayTypeSymbol arrayTypeSymbol when !arrayTypeSymbol.IsPurelyPublicTypes():
                    return ReturnStatement(InvocationExpression(MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                invocationExpression, IdentifierName("ToEnumerable")))
                            .WithArgumentList(ArgumentList(SingletonSeparatedList(Argument(
                                SimpleLambdaExpression(Parameter(Identifier("x")))
                                    .WithExpressionBody(ObjectCreationExpression(IdentifierName(arrayTypeSymbol.ElementType.GetOITypeName()))
                                        .WithArgumentList(
                                            ArgumentList(SingletonSeparatedList(Argument(IdentifierName("x")))))))))),
                        IdentifierName("ToArray"))));
                case { SpecialType: SpecialType.None, DeclaredAccessibility: < Accessibility.Public }:
                    return ReturnStatement(ObjectCreationExpression(returnTypeIdentifier)
                        .WithArgumentList(ArgumentList(SingletonSeparatedList(Argument(invocationExpression)))));
                default:
                    return ReturnStatement(CastExpression(returnTypeIdentifier, invocationExpression));
            }
        }

        private static InvocationExpressionSyntax GetMethodInvocationSyntax(IMethodSymbol methodSymbol,
            IdentifierNameSyntax methodInfoIdentifier)
        {
            var returnParameterSyntaxList = GetParameterSyntaxes(methodSymbol, GetObjectFromParameter);

            return InvocationExpression(MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    methodInfoIdentifier, IdentifierName("Invoke")))
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
        }

        private static ExpressionStatementSyntax GetAssignmentStatementSyntax(IMethodSymbol methodSymbol,
            ExpressionSyntax methodInfoIdentifier)
        {
            var assignmentParameters = GetParameterSyntaxes(methodSymbol, GetTypeFromParameter);

            return ExpressionStatement(AssignmentExpression(SyntaxKind.CoalesceAssignmentExpression,
                methodInfoIdentifier,
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
                            : ArrayCreationExpression(ArrayType(IdentifierName("Type"))
                                    .WithRankSpecifiers(SingletonList(ArrayRankSpecifier(
                                        SingletonSeparatedList<ExpressionSyntax>(OmittedArraySizeExpression())))))
                                .WithInitializer(InitializerExpression(SyntaxKind.ArrayInitializerExpression,
                                    assignmentParameters)))
                    })))));
        }

        private static ExpressionSyntax GetTypeFromParameter(IParameterSymbol symbol)
        {
            return InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                IdentifierName(symbol.Name), IdentifierName("GetType")));
        }

        private static ExpressionSyntax GetObjectFromParameter(IParameterSymbol symbol)
        {
            switch (symbol.Type)
            {
                case IArrayTypeSymbol arrayTypeSymbol:
                    return InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                        InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                IdentifierName(symbol.Name), IdentifierName("Select")))
                            .WithArgumentList(ArgumentList(SingletonSeparatedList<ArgumentSyntax>(Argument(
                                SimpleLambdaExpression(Parameter(Identifier("x")))
                                    .WithExpressionBody(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                        IdentifierName("x"),
                                        IdentifierName("OI_ObjectInstance"))))))),
                        IdentifierName("ToArray")));
                case INamedTypeSymbol
                    {
                        SpecialType: SpecialType.System_Collections_Generic_IEnumerable_T
                        or SpecialType.System_Collections_IEnumerable
                    } namedTypeSymbol
                    when !namedTypeSymbol.IsPurelyPublicTypes():
                    return InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName(symbol.Name), IdentifierName("Select")))
                        .WithArgumentList(ArgumentList(SingletonSeparatedList<ArgumentSyntax>(
                            Argument(SimpleLambdaExpression(Parameter(Identifier("x")))
                                .WithExpressionBody(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                    IdentifierName("x"),
                                    IdentifierName(ConstantStrings.OpenInternalObjectInstance)))))));
                case INamedTypeSymbol { SpecialType: SpecialType.None, DeclaredAccessibility: < Accessibility.Public }:
                    return MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, IdentifierName(symbol.Name),
                        IdentifierName(ConstantStrings.OpenInternalObjectInstance));
                case INamedTypeSymbol:
                    return IdentifierName(symbol.Name);
                default:
                    OpenInternalsSourceGenerator.StringBuilder.Append(
                        $"{symbol.ToDisplayString()}, {symbol.Type.ToDisplayString()}, {symbol.Type.GetType()}, {symbol.Type.SpecialType}");
                    break;
            }

            return IdentifierName(symbol.Name);
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