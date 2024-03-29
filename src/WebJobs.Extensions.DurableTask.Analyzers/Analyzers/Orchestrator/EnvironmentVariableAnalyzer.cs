﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.Azure.WebJobs.Extensions.DurableTask.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class EnvironmentVariableAnalyzer
    {
        public const string DiagnosticId = "DF0106";

        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.EnvironmentVariableAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.DeterministicAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.DeterministicAnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = SupportedCategories.Orchestrator;
        public const DiagnosticSeverity Severity = DiagnosticSeverity.Warning;

        public static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, Severity, isEnabledByDefault: true, description: Description);

        internal static bool RegisterDiagnostic(CompilationAnalysisContext context, SemanticModel semanticModel, SyntaxNode method)
        {
            var diagnosedIssue = false;

            foreach (SyntaxNode descendant in method.DescendantNodes())
            {
                if (descendant is IdentifierNameSyntax identifierName)
                {
                    var identifierText = identifierName.Identifier.ValueText;
                    if (identifierText == "GetEnvironmentVariable" || identifierText == "GetEnvironmentVariables" || identifierText == "ExpandEnvironmentVariables")
                    {
                        var memberAccessExpression = identifierName.Parent;
                        var invocationExpression = memberAccessExpression.Parent;
                        if (SyntaxNodeUtils.TryGetISymbol(semanticModel, memberAccessExpression, out ISymbol memberSymbol))
                        {
                            if (memberSymbol.ToString().StartsWith("System.Environment"))
                            {
                                var diagnostic = Diagnostic.Create(Rule, invocationExpression.GetLocation(), memberAccessExpression);

                                if (context.Compilation.ContainsSyntaxTree(method.SyntaxTree))
                                {
                                    context.ReportDiagnostic(diagnostic);
                                }

                                diagnosedIssue = true;
                            }
                        }
                    }
                }
            }

            return diagnosedIssue;
        }
    }
}
