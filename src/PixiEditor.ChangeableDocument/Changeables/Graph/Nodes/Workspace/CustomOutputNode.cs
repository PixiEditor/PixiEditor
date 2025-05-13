using Drawie.Backend.Core.Surfaces;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Workspace;

[NodeInfo("CustomOutput")]
public class CustomOutputNode : Node, IRenderInput, IPreviewRenderable
{
    public const string OutputNamePropertyName = "OutputName";
    public const string IsDefaultExportPropertyName = "IsDefaultExport";
    public const string ExportSizePropertyName = "ExportSize";
    public RenderInputProperty Input { get; }
    public InputProperty<string> OutputName { get; }
    public InputProperty<bool> IsDefaultExport { get; }
    public InputProperty<VecI> ExportSize { get; }

    private VecI? lastDocumentSize;
    public CustomOutputNode()
    {
        Input = new RenderInputProperty(this, OutputNode.InputPropertyName, "BACKGROUND", null);
        AddInputProperty(Input);
        
        OutputName = CreateInput(OutputNamePropertyName, "OUTPUT_NAME", "");
        IsDefaultExport = CreateInput(IsDefaultExportPropertyName, "IS_DEFAULT_EXPORT", false);
        ExportSize = CreateInput(ExportSizePropertyName, "EXPORT_SIZE", VecI.Zero);
    }

    public override Node CreateCopy()
    {
        return new CustomOutputNode();
    }

    protected override void OnExecute(RenderContext context)
    {
        if (context.TargetOutput == OutputName.Value)
        {
            lastDocumentSize = context.DocumentSize;

            int saved = context.RenderSurface.Canvas.Save();
            context.RenderSurface.Canvas.ClipRect(new RectD(0, 0, context.DocumentSize.X, context.DocumentSize.Y));
            Input.Value?.Paint(context, context.RenderSurface);

            context.RenderSurface.Canvas.RestoreToCount(saved);
        }
    }

    RenderInputProperty IRenderInput.Background => Input;
    public RectD? GetPreviewBounds(int frame, string elementToRenderName = "")
    {
        if (lastDocumentSize == null)
        {
            return null;
        }
        
        return new RectD(0, 0, lastDocumentSize.Value.X, lastDocumentSize.Value.Y); 
    }

    public bool RenderPreview(DrawingSurface renderOn, RenderContext context, string elementToRenderName)
    {
        if (Input.Value == null)
        {
            return false;
        }
        
        int saved = renderOn.Canvas.Save();
        Input.Value.Paint(context, renderOn);
        
        renderOn.Canvas.RestoreToCount(saved);
        
        return true;
    }
}
