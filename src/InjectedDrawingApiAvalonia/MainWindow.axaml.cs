using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using PixiEditor.DrawingApi.Core.Bridge;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Surfaces;
using PixiEditor.DrawingApi.Core.Surfaces.PaintImpl;
using PixiEditor.DrawingApi.Skia;
using PixiEditor.Numerics;
using SkiaSharp;
using Colors = PixiEditor.DrawingApi.Core.ColorsImpl.Colors;

namespace InjectedDrawingApiAvalonia;

public partial class MainWindow : Window
{
    private Texture texture;

    public MainWindow()
    {
        InitializeComponent();
        SurfaceControl.Draw += SurfaceControlOnDraw;
        
        Task.Run(async () =>
        {
            texture = new(new VecI(100, 100));
            texture.GpuSurface.Canvas.DrawRect(0, 0, 100, 100, new Paint() { Color = Colors.Red });
            await Task.Delay(50); // some test delay

            Texture texture2 = new(new VecI(50, 50));
            texture2.GpuSurface.Canvas.DrawRect(0, 0, 50, 50, new Paint() { Color = Colors.Blue });

            texture.GpuSurface.Canvas.DrawSurface(texture2.GpuSurface, 0, 0);
            Dispatcher.UIThread.Post(() =>
            {
                SurfaceControl.InvalidateVisual();
            });
        });
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        SurfaceControl.InvalidateVisual();
    }

    private void SurfaceControlOnDraw(SKCanvas draw)
    {
        draw.Clear(SKColors.White);
        texture.GpuSurface.Flush();
        draw.DrawSurface(texture.GpuSurface.Native as SKSurface, 0, 0);
    }
}
