// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.CodeAnalysis.Templating;
internal class TemplateParser
{

}

public enum SyntaxKind : ushort
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


internal abstract class SyntaxNode
{

}
