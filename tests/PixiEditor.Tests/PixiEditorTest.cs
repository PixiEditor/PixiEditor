using Drawie.Backend.Core.Bridge;
using Drawie.Skia;
using DrawiEngine;

namespace PixiEditor.Tests;

public class PixiEditorTest
{
    public PixiEditorTest()
    {
        if (DrawingBackendApi.HasBackend)
        {
            return;
        }

        SkiaDrawingBackend skiaDrawingBackend = new SkiaDrawingBackend();
        DrawingBackendApi.SetupBackend(skiaDrawingBackend, new DrawieRenderingDispatcher());
    }
}