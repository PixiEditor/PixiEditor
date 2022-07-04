using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using PixiEditor.Models.Tools;
using SkiaSharp;
using System;
using System.Collections.Generic;

namespace PixiEditorTests.ModelsTests.ControllersTests;

public class MockedSinglePixelPenTool : BitmapOperationTool
{
    public override string Tooltip => "";

    public override void Use(Layer activeLayer, Layer previewLayer, IEnumerable<Layer> allLayers, IReadOnlyList<Coordinates> recordedMouseMovement,
        SKColor color)
    {
        if (recordedMouseMovement == null || activeLayer == null)
            throw new ArgumentException("Parameter is null");
        activeLayer.LayerBitmap.SkiaSurface.Canvas.DrawPoint(recordedMouseMovement[0].ToSKPoint(), color);
    }
}