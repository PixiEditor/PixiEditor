using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.DrawingApi.Core.Surface.ImageData;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

public class MergeNode : Node, IBackgroundInput
{
    public InputProperty<Image?> Top { get; }
    public InputProperty<Image?> Bottom { get; }
    public OutputProperty<Image?> Output { get; }
    
    public MergeNode() 
    {
        Top = CreateInput<Image?>("Top", "TOP", null);
        Bottom = CreateInput<Image?>("Bottom", "BOTTOM", null);
        Output = CreateOutput<Image?>("Output", "OUTPUT", null);
    }
    
    public override bool Validate()
    {
        return Top.Connection != null || Bottom.Connection != null;
    }

    public override Node CreateCopy()
    {
        return new MergeNode();
    }

    protected override Image? OnExecute(KeyFrameTime frame)
    {
        if(Top.Value == null && Bottom.Value == null)
        {
            Output.Value = null;
            return null;
        }
        
        int width = Top.Value?.Width ?? Bottom.Value.Width;
        int height = Top.Value?.Height ?? Bottom.Value.Height;
        
        Surface workingSurface = new Surface(new VecI(width, height));
        
        if(Bottom.Value != null)
        {
            workingSurface.DrawingSurface.Canvas.DrawImage(Bottom.Value, 0, 0);
        }
        
        if(Top.Value != null)
        {
            workingSurface.DrawingSurface.Canvas.DrawImage(Top.Value, 0, 0);
        }
        
        Output.Value = workingSurface.DrawingSurface.Snapshot();
        
        workingSurface.Dispose();
        return Output.Value;
    }

    InputProperty<Image> IBackgroundInput.Background => Bottom;
}
