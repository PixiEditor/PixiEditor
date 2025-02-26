using PixiEditor.ChangeableDocument.Rendering;
using Drawie.Backend.Core;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.FilterNodes;

public abstract class FilterNode : Node
{
    public OutputProperty<Filter> Output { get; }

    public InputProperty<Filter?> Input { get; }

    public FilterNode()
    {
        Output = CreateOutput<Filter>(nameof(Output), "FILTERS", null);
        Input = CreateInput<Filter>(nameof(Input), "PREVIOUS", null);
    }

    protected override void OnExecute(RenderContext context)
    {
        var colorFilter = GetColorFilter(context.ProcessingColorSpace);
        var imageFilter = GetImageFilter();

        if (colorFilter == null && imageFilter == null)
        {
            Output.Value = Input.Value;
            return;
        }

        var filter = Input.Value;

        Output.Value = filter == null ? new Filter(colorFilter, imageFilter) : filter.Add(colorFilter, imageFilter);
    }

    protected virtual ColorFilter? GetColorFilter(ColorSpace colorSpace) => null;

    protected virtual ImageFilter? GetImageFilter() => null;

    protected ColorMatrix AdjustMatrixForColorSpace(ColorMatrix matrix)
    {
        float[] adjusted = new float[20];
        var transformFn = ColorSpace.CreateSrgb().GetTransformFunction();
        for (int i = 0; i < 20; i++)
        {
            adjusted[i] = transformFn.Transform(matrix[i]);
        }

        return new ColorMatrix(adjusted);
    }
}
