// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#nullable disable


using Microsoft.CodeAnalysis.CSharp.Templating.Parser;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal static class TemplatingKinds
{
    public const SyntaxKind TemplateDirectiveStartToken = (SyntaxKind)TemplateSyntaxKind.DirectiveStartToken;
    public const SyntaxKind TemplateDirectiveEndToken = (SyntaxKind)TemplateSyntaxKind.DirectiveEndToken;

    public const SyntaxKind TemplateStandardControlBlockStartToken = (SyntaxKind)TemplateSyntaxKind.StandardControlBlockStartToken;
    public const SyntaxKind TemplateStandardControlBlockEndToken = (SyntaxKind)TemplateSyntaxKind.StandardControlBlockEndToken;
    public const SyntaxKind TemplateExpressionControlBlockStartToken = (SyntaxKind)TemplateSyntaxKind.ExpressionControlBlockStartToken;
    public const SyntaxKind TemplateExpressionControlBlockEndToken = (SyntaxKind)TemplateSyntaxKind.ExpressionControlBlockEndToken;
    public const SyntaxKind TemplateClassFeatureControlBlockStartToken = (SyntaxKind)TemplateSyntaxKind.ClassFeatureControlBlockStartToken;
    public const SyntaxKind TemplateClassFeatureControlBlockEndToken = (SyntaxKind)TemplateSyntaxKind.ClassFeatureControlBlockEndToken;
}
