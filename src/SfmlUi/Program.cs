using PixiEditor.DrawingApi.Core.Bridge;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.DrawingApi.Skia;
using SFML.Graphics;
using SFML.Window;

namespace SfmlUi;

internal class Program
{
    const int WIDTH = 640;
    const int HEIGHT = 480;
    const string TITLE = "SfmlPixiEditor";

    private static DocumentViewModel? doc;

    static void Main(string[] args)
    {
        SkiaDrawingBackend skiaDrawingBackend = new SkiaDrawingBackend();
        DrawingBackendApi.SetupBackend(skiaDrawingBackend);

        VideoMode mode = new VideoMode(WIDTH, HEIGHT);
        RenderWindow window = new RenderWindow(mode, TITLE);

        window.SetVerticalSyncEnabled(true);
        window.Closed += (sender, args) => window.Close();
        
        doc = new((512, 512));
        Viewport viewport = new(window, doc);
        doc.Viewport = viewport;

        viewport.CanvasMouseDown += Viewport_CanvasMouseDown;
        viewport.CanvasMouseMove += Viewport_CanvasMouseMove;
        viewport.CanvasMouseUp += Viewport_CanvasMouseUp;

        while (window.IsOpen)
        {
            window.DispatchEvents();
            window.Clear(Color.Black);

            viewport.Draw();

            window.Display();
        }
    }

    private static void Viewport_CanvasMouseUp(object? sender, PixiEditor.DrawingApi.Core.Numerics.VecD e)
    {
        drawing = false;
        doc!.StopDrawing();
    }

    private static void Viewport_CanvasMouseMove(object? sender, PixiEditor.DrawingApi.Core.Numerics.VecD e)
    {
        if (drawing)
            doc!.Draw((VecI)e);
    }

    private static bool drawing = false;
    private static void Viewport_CanvasMouseDown(object? sender, PixiEditor.DrawingApi.Core.Numerics.VecD e)
    {
        drawing = true;
        doc!.Draw((VecI)e);
    }
}
