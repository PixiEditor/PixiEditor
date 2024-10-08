﻿using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core;
using PixiEditor.DrawingApi.Core.Shaders.Generation.Expressions;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.CombineSeparate;

[NodeInfo("SeparateVecI")]
public class SeparateVecINode : Node
{
    public FuncInputProperty<Int2> Vector { get; }
    
    public FuncOutputProperty<Int1> X { get; }
    
    public FuncOutputProperty<Int1> Y { get; }
    
    public SeparateVecINode()
    {
        X = CreateFuncOutput<Int1>("X", "X", ctx => Vector.Value(ctx).X);
        Y = CreateFuncOutput<Int1>("Y", "Y", ctx => Vector.Value(ctx).Y);
        Vector = CreateFuncInput<Int2>("Vector", "VECTOR", new VecI(0, 0));
    }

    protected override Texture? OnExecute(RenderingContext context)
    {
        return null;
    }


    public override Node CreateCopy() => new SeparateVecINode();
}
