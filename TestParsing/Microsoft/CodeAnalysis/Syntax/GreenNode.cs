// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Chryssie.PooledObjects;
using Microsoft.CodeAnalysis.Collections;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Syntax.InternalSyntax;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.Templating;

/// <summary>
/// 
/// </summary>
/// <remarks>
/// <para>Refer to https://ericlippert.com/2012/06/08/red-green-trees/ for the concept.</para>
/// </remarks>



internal abstract record GreenNode
{
    public abstract int SlotCount { get; }
    internal virtual bool HasSlots => SlotCount == 0;

    public abstract int FullWidth { get; }


    internal abstract GreenNode? GetSlot(int index);

    public virtual int GetSlotOffset(int index)
    {
        int offset = 0;
        for (int i = 0; i < index; i++)
        {
            var child = this.GetSlot(i);
            if (child != null)
            {
                offset += child.FullWidth;
            }
        }

        return offset;
    }

    internal GreenNode GetRequiredSlot(int index)
    {
        var node = GetSlot(index);
        Debug.Assert(node is not null);
        return node;
    }

    public void WriteTo(TextWriter writer)
    {
        using (ImmutableArrayBuilderPool<GreenNode>.Default.Get(out var builder))
        {
            builder.Push(this);
            ProcessStack(writer, builder);
        }
        
        static void ProcessStack(TextWriter writer, ImmutableArray<GreenNode>.Builder stack)
        {
            while (stack.Count > 0) stack.Pop().WriteTo(writer, stack);
        }
    }

    protected internal void WriteTo(TextWriter writer, ImmutableArrayBuilder<GreenNode> stack);

    override 
}
