using System.Collections.Generic;
using PixiEditor.DrawingApi.Core.Surface.PaintImpl;
using SkiaSharp;

namespace InjectedDrawingApiAvalonia;

public class CommandBuffer
{
    private List<Command> _commands = new List<Command>();
    public void DrawRect(int x, int y, int width, int height, Paint paint)
    {
        _commands.Add(new DrawRectCommand(x, y, width, height, paint));
    }
    
    public void Dispatch(SKCanvas surface)
    {
        foreach (var command in _commands)
        {
            command.Execute(surface);
        }
    }
}

public abstract class Command
{
    public abstract void Execute(SKCanvas surface);
}

public class DrawRectCommand : Command
{
    private readonly int _x;
    private readonly int _y;
    private readonly int _width;
    private readonly int _height;
    private readonly SKPaint _paint;

    public DrawRectCommand(int x, int y, int width, int height, Paint paint)
    {
        _x = x;
        _y = y;
        _width = width;
        _height = height;
        _paint = paint.Native as SKPaint;
    }

    public override void Execute(SKCanvas surface)
    {
        surface.DrawRect(_x, _y, _width, _height, _paint);
    }
}
