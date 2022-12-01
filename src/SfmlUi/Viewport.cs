using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChunkyImageLib.DataHolders;
using PixiEditor.DrawingApi.Core.Numerics;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace SfmlUi;
internal class Viewport
{
    public event EventHandler<VecD>? CanvasMouseDown;
    public event EventHandler<VecD>? CanvasMouseMove;
    public event EventHandler<VecD>? CanvasMouseUp;

    private readonly RenderWindow window;
    private readonly DocumentViewModel document;

    private Dictionary<ChunkResolution, Sprite> sprites = new();

    private Vector2f centerOnDragStart;
    private Vector2i dragStartPos;
    private bool isDragging = false;

    private View view;

    public RectD VisibleArea => RectD.FromCenterAndSize(new(view.Center.X, view.Center.Y), new(view.Size.X, view.Size.Y));
    
    public Viewport(RenderWindow window, DocumentViewModel document)
    {
        this.window = window;
        this.document = document;

        sprites.Add(ChunkResolution.Full, new Sprite(document.Textures[ChunkResolution.Full].Texture));
        sprites.Add(ChunkResolution.Half, new Sprite(document.Textures[ChunkResolution.Half].Texture));
        sprites.Add(ChunkResolution.Quarter, new Sprite(document.Textures[ChunkResolution.Quarter].Texture));
        sprites.Add(ChunkResolution.Eighth, new Sprite(document.Textures[ChunkResolution.Eighth].Texture));

        view = new View(window.DefaultView);

        window.MouseMoved += MouseMove;
        window.MouseButtonPressed += MouseDown;
        window.MouseButtonReleased += MouseUp;
        window.MouseWheelScrolled += MouseWheelScrolled;
    }

    private void MouseWheelScrolled(object? sender, MouseWheelScrollEventArgs e)
    {
        if (e.Delta < 0)
            view.Zoom(1.1f);
        else
            view.Zoom(0.9f);
    }

    private void MouseUp(object? sender, MouseButtonEventArgs e)
    {
        if (e.Button == Mouse.Button.Left)
        {
            var pos = window.MapPixelToCoords(new(e.X, e.Y), view);
            CanvasMouseUp?.Invoke(this, new(pos.X, pos.Y));
            return;
        }
        isDragging = false;
    }

    private void MouseDown(object? sender, MouseButtonEventArgs e)
    {
        if (e.Button == Mouse.Button.Left)
        {
            var pos = window.MapPixelToCoords(new(e.X, e.Y), view);
            CanvasMouseDown?.Invoke(this, new(pos.X, pos.Y));
            return;
        }
        isDragging = true;
        dragStartPos = new(e.X, e.Y);
        centerOnDragStart = view.Center;
    }

    private void MouseMove(object? sender, MouseMoveEventArgs e)
    {
        Vector2i windowMousePos = new(e.X, e.Y);
        Vector2f curPosOnCanvas = window.MapPixelToCoords(windowMousePos, view);

        CanvasMouseMove?.Invoke(this, new(curPosOnCanvas.X, curPosOnCanvas.Y));
        if (!isDragging)
            return;

        Vector2f startPosOnCavnas = window.MapPixelToCoords(dragStartPos, view);
        Vector2f delta = curPosOnCanvas - startPosOnCavnas;

        view.Center = centerOnDragStart - delta;
    }

    public void Draw()
    {
        window.SetView(view);

        window.Draw(new RectangleShape(new Vector2f(document.Size.X, document.Size.Y)) { FillColor = Color.Yellow });
        window.Draw(sprites[ChunkResolution.Full]);
    }
}
