using PixiEditor.ChangeableDocument.Changeables.Graph.Context;
using PixiEditor.ChangeableDocument.Enums;
using PixiEditor.ChangeableDocument.Rendering;
using Drawie.Backend.Core;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Shaders.Generation.Expressions;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.CombineSeparate;

[NodeInfo("SeparateColor")]
public class SeparateColorNode : Node
{
    private readonly NodeVariableAttachments contextVariables = new();
    
    public FuncInputProperty<Half4, ShaderFuncContext> Color { get; }
    
    public InputProperty<CombineSeparateColorMode> Mode { get; }

    /// <summary>
    /// Represents either Red 'R' or Hue 'H' depending on <see cref="Mode"/>
    /// </summary>
    public FuncOutputProperty<Float1, ShaderFuncContext> V1 { get; }
    
    /// <summary>
    /// Represents either Green 'G' or Saturation 'S' depending on <see cref="Mode"/>
    /// </summary>
    public FuncOutputProperty<Float1, ShaderFuncContext> V2 { get; }
    
    /// <summary>
    /// Represents either Blue 'B', Value 'V' or Lightness 'L' depending on <see cref="Mode"/>
    /// </summary>
    public FuncOutputProperty<Float1, ShaderFuncContext> V3 { get; }
    
    public FuncOutputProperty<Float1, ShaderFuncContext> A { get; }
    
    public SeparateColorNode()
    {
        V1 = CreateFuncOutput<Float1, ShaderFuncContext>("R", "R", ctx => GetColor(ctx).R);
        V2 = CreateFuncOutput<Float1, ShaderFuncContext>("G", "G", ctx => GetColor(ctx).G);
        V3 = CreateFuncOutput<Float1, ShaderFuncContext>("B", "B", ctx => GetColor(ctx).B);
        A = CreateFuncOutput<Float1, ShaderFuncContext>("A", "A", ctx => GetColor(ctx).A);
        Mode = CreateInput("Mode", "MODE", CombineSeparateColorMode.RGB);
        Color = CreateFuncInput<Half4, ShaderFuncContext>(nameof(Color), "COLOR", new Color());
    }

    protected override void OnExecute(RenderContext context)
    {
    }
    
    private Half4 GetColor(ShaderFuncContext ctx) =>
        Mode.Value switch
        {
            CombineSeparateColorMode.RGB => GetRgba(ctx),
            CombineSeparateColorMode.HSV => GetHsva(ctx),
            CombineSeparateColorMode.HSL => GetHsla(ctx)
        };

    private Half4 GetRgba(ShaderFuncContext ctx) =>
        ctx.HasContext ? contextVariables.GetOrAttachNew(ctx, Color, () => ctx.GetValue(Color)) : ctx.GetValue(Color);

    private Half4 GetHsva(ShaderFuncContext ctx) =>
        ctx.HasContext ? contextVariables.GetOrAttachNew(ctx, Color, () => ctx.RgbaToHsva(ctx.GetValue(Color))) : ctx.RgbaToHsva(ctx.GetValue(Color));

    private Half4 GetHsla(ShaderFuncContext ctx) =>
        ctx.HasContext ? contextVariables.GetOrAttachNew(ctx, Color, () => ctx.RgbaToHsla(ctx.GetValue(Color))) : ctx.RgbaToHsla(ctx.GetValue(Color));

    public override Node CreateCopy() => new SeparateColorNode();
}
