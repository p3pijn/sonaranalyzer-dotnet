﻿/*
 * SonarLint for Visual Studio
 * Copyright (C) 2015-2016 SonarSource SA
 * mailto:contact@sonarsource.com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02
 */

using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarLint.Common;
using SonarLint.Common.Sqale;
using SonarLint.Helpers;

namespace SonarLint.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [SqaleSubCharacteristic(SqaleSubCharacteristic.ArchitectureReliability)]
    [SqaleConstantRemediation("5min")]
    [Rule(DiagnosticId, RuleSeverity, Title, IsActivatedByDefault)]
    [Tags(Tag.Suspicious)]
    public class EmptyMethod : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S1186";
        internal const string Title = "Methods should not be empty";
        internal const string Description =
            "There are several reasons for a method not to have a method body: It is an " +
            "unintentional omission, and should be fixed. It is not yet, or never will be, " +
            "supported. In this case a \"NotSupportedException\" should be thrown. The " +
            "method is an intentionally-blank override. In this case a nested comment should " +
            "explain the reason for the blank override.";
        internal const string MessageFormat = "Add a nested comment explaining why this method is empty{0} or complete the implementation.";
        internal const string MessageInternal = ", throw a \"NotSupportedException\"";
        internal const string Category = SonarLint.Common.Category.Reliability;
        internal const Severity RuleSeverity = Severity.Major;
        internal const bool IsActivatedByDefault = true;

        internal static readonly DiagnosticDescriptor Rule =
            new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category,
                RuleSeverity.ToDiagnosticSeverity(), IsActivatedByDefault,
                helpLinkUri: DiagnosticId.GetHelpLink(),
                description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c => CheckMethodDeclaration(c),
                SyntaxKind.MethodDeclaration);

            context.RegisterSyntaxNodeActionInNonGenerated(
                c => CheckConstructorDeclaration(c),
                SyntaxKind.ConstructorDeclaration);

            context.RegisterSyntaxNodeActionInNonGenerated(
                c => CheckDestructorDeclaration(c),
                SyntaxKind.DestructorDeclaration);
        }

        private static void CheckDestructorDeclaration(SyntaxNodeAnalysisContext c)
        {
            var destructorDeclaration = (DestructorDeclarationSyntax)c.Node;

            if (destructorDeclaration.Body != null &&
                IsEmpty(destructorDeclaration.Body))
            {
                c.ReportDiagnostic(Diagnostic.Create(Rule, destructorDeclaration.Identifier.GetLocation(), string.Empty));
            }
        }

        private static void CheckConstructorDeclaration(SyntaxNodeAnalysisContext c)
        {
            var constructorDeclaration = (ConstructorDeclarationSyntax)c.Node;

            if (!IsConstructorParameterless(constructorDeclaration) ||
                !IsConstructorBodyEmpty(constructorDeclaration))
            {
                return;
            }

            if (IsSinglePublicConstructor(constructorDeclaration, c.SemanticModel) ||
                constructorDeclaration.Modifiers.Any(modifier => modifier.IsKind(SyntaxKind.StaticKeyword)))
            {
                c.ReportDiagnostic(Diagnostic.Create(Rule, constructorDeclaration.Identifier.GetLocation(), string.Empty));
            }
        }

        private static bool IsSinglePublicConstructor(ConstructorDeclarationSyntax constructorDeclaration, SemanticModel semanticModel)
        {
            return constructorDeclaration.Modifiers.Any(modifier => modifier.IsKind(SyntaxKind.PublicKeyword)) &&
                constructorDeclaration.Initializer == null &&
                TypeHasExactlyOneConstructor(constructorDeclaration, semanticModel);
        }

        private static bool TypeHasExactlyOneConstructor(ConstructorDeclarationSyntax constructorDeclaration, SemanticModel semanticModel)
        {
            var symbol = semanticModel.GetDeclaredSymbol(constructorDeclaration);
            return symbol != null &&
                symbol.ContainingType.GetMembers().OfType<IMethodSymbol>().Count(m => m.MethodKind == MethodKind.Constructor) == 1;
        }

        private static bool IsConstructorBodyEmpty(ConstructorDeclarationSyntax constructorDeclaration)
        {
            return constructorDeclaration.Body != null &&
                IsEmpty(constructorDeclaration.Body);
        }

        private static bool IsConstructorParameterless(ConstructorDeclarationSyntax constructorDeclaration)
        {
            return constructorDeclaration.ParameterList != null &&
                !constructorDeclaration.ParameterList.Parameters.Any();
        }

        private static void CheckMethodDeclaration(SyntaxNodeAnalysisContext c)
        {
            var methodNode = (MethodDeclarationSyntax)c.Node;

            if (methodNode.Body != null &&
                IsEmpty(methodNode.Body) &&
                !ShouldMethodBeExcluded(methodNode, c.SemanticModel))
            {
                c.ReportDiagnostic(Diagnostic.Create(Rule, methodNode.Identifier.GetLocation(), MessageInternal));
            }
        }

        private static bool ShouldMethodBeExcluded(MethodDeclarationSyntax methodNode, SemanticModel semanticModel)
        {
            if (methodNode.Modifiers.Any(modifier => modifier.IsKind(SyntaxKind.VirtualKeyword)))
            {
                return true;
            }

            var methodSymbol = semanticModel.GetDeclaredSymbol(methodNode);
            if (methodSymbol != null &&
                methodSymbol.IsOverride &&
                methodSymbol.OverriddenMethod != null &&
                methodSymbol.OverriddenMethod.IsAbstract)
            {
                return true;
            }

            return methodNode.Modifiers.Any(modifier => modifier.IsKind(SyntaxKind.OverrideKeyword)) &&
                semanticModel.Compilation.IsTest();
        }

        private static bool IsEmpty(BlockSyntax node)
        {
            return !node.Statements.Any() && !ContainsComment(node);
        }

        private static bool ContainsComment(BlockSyntax node)
        {
            return ContainsComment(node.OpenBraceToken.TrailingTrivia) || ContainsComment(node.CloseBraceToken.LeadingTrivia);
        }

        private static bool ContainsComment(SyntaxTriviaList trivias)
        {
            return trivias.Any(trivia => trivia.IsKind(SyntaxKind.SingleLineCommentTrivia) || trivia.IsKind(SyntaxKind.MultiLineCommentTrivia));
        }
    }
}
