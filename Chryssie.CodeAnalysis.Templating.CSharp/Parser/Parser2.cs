// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Templating.Parser;
internal class TemplateParser
{

}

public enum TemplateSyntaxKind : ushort
{
    LiteralToken = 24058,
    DirectiveStartToken = LiteralToken + 1,
    DirectiveEndToken = DirectiveStartToken + 1,

    StandardControlBlockStartToken = DirectiveEndToken + 1,
    StandardControlBlockEndToken = StandardControlBlockStartToken + 1,
    ExpressionControlBlockStartToken = StandardControlBlockEndToken + 1,
    ExpressionControlBlockEndToken = ExpressionControlBlockStartToken + 1,
    ClassFeatureControlBlockStartToken = ExpressionControlBlockEndToken + 1,
    ClassFeatureControlBlockEndToken = ClassFeatureControlBlockStartToken + 1,
}

public abstract class TemplateSyntaxNode
{
    private readonly ushort kind;
}

/// <summary>
/// 
/// </summary>
/// <remarks>
/// <para>Refer to https://ericlippert.com/2012/06/08/red-green-trees/ for the concept.</para>
/// </remarks>
internal abstract class GreenNode
{
    private readonly ushort kind;
}


internal abstract class SyntaxNode
{

}
