// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#nullable disable


namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal static class TemplatingKinds
{
    public const SyntaxKind TemplateDirectiveStartToken = (SyntaxKind)24059;
    public const SyntaxKind TemplateDirectiveEndToken = TemplateDirectiveStartToken + 1;

    public const SyntaxKind TemplateDirectiveStandardControlBlockStartToken = TemplateDirectiveEndToken + 1;
    public const SyntaxKind TemplateDirectiveStandardControlBlockEndToken = TemplateDirectiveStandardControlBlockStartToken + 1;
    public const SyntaxKind TemplateDirectiveExpressionControlBlockStartToken = TemplateDirectiveStandardControlBlockEndToken + 1;
    public const SyntaxKind TemplateDirectiveExpressionControlBlockEndToken = TemplateDirectiveExpressionControlBlockStartToken + 1;
    public const SyntaxKind TemplateDirectiveClassFeatureControlBlockStartToken = TemplateDirectiveExpressionControlBlockEndToken + 1;
    public const SyntaxKind TemplateDirectiveClassFeatureControlBlockEndToken = TemplateDirectiveClassFeatureControlBlockStartToken + 1;
}
