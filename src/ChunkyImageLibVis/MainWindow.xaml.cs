using ChunkyImageLib.DataHolders;
using ChunkyImageLib.Operations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Drawie.Backend.Core.Numerics;
using Drawie.Numerics;

namespace ChunkyImageLibVis;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window, INotifyPropertyChanged
{
    private double x1;
    private double y1;
    private double x2;
    private double y2;

    public double X1
    {
        get => x1;
        set
        {
            x1 = value;
            PropertyChanged?.Invoke(this, new(nameof(X1)));
            PropertyChanged?.Invoke(this, new(nameof(RectWidth)));
            PropertyChanged?.Invoke(this, new(nameof(HalfRectWidth)));
        }
    }
    public double X2
    {
        get => x2;
        set
        {
            x2 = value;
            PropertyChanged?.Invoke(this, new(nameof(X2)));
            PropertyChanged?.Invoke(this, new(nameof(RectWidth)));
            PropertyChanged?.Invoke(this, new(nameof(HalfRectWidth)));
        }
    }
    public double Y1
    {
        get => y1;
        set
        {
            y1 = value;
            PropertyChanged?.Invoke(this, new(nameof(Y1)));
            PropertyChanged?.Invoke(this, new(nameof(RectHeight)));
            PropertyChanged?.Invoke(this, new(nameof(HalfRectHeight)));
        }
    }
    public double Y2
    {
        get => y2;
        set
        {
            y2 = value;
            PropertyChanged?.Invoke(this, new(nameof(Y2)));
            PropertyChanged?.Invoke(this, new(nameof(RectHeight)));
            PropertyChanged?.Invoke(this, new(nameof(HalfRectHeight)));
        }
    }

    public double RectWidth { get => Math.Abs(X2 - X1); }
    public double RectHeight { get => Math.Abs(Y2 - Y1); }

    public double HalfRectWidth { get => Math.Abs(X2 - X1) / 2; }
    public double HalfRectHeight { get => Math.Abs(Y2 - Y1) / 2; }


    private double angle;
    public double Angle
    {
        get => angle;
        set
        {
            angle = value;
            PropertyChanged?.Invoke(this, new(nameof(Angle)));
        }
    }

    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;
        CreateGrid();
    }

    public void CreateGrid()
    {
        for (int i = 0; i < 20; i++)
        {
            Line ver = new()
            {
                X1 = i * 32,
                X2 = i * 32,
                Y1 = 0,
                Y2 = 1000,
                Stroke = Brushes.Gray,
                StrokeThickness = 1,
            };
            Line hor = new()
            {
                X1 = 0,
                X2 = 1000,
                Y1 = i * 32,
                Y2 = i * 32,
                Stroke = Brushes.Gray,
                StrokeThickness = 1,
            };
            canvas.Children.Add(ver);
            canvas.Children.Add(hor);
        }
    }

    public List<Rectangle> rectangles = new();
    private void UpdateChunks()
    {
        foreach (var rect in rectangles)
        {
            canvas.Children.Remove(rect);
        }
        rectangles.Clear();
        var chunks = OperationHelper.FindChunksTouchingRectangle(new VecD(X1 + HalfRectWidth, Y1 + HalfRectHeight), new(X2 - X1, Y2 - Y1), Angle * Math.PI / 180, 32);
        var innerChunks = OperationHelper.FindChunksFullyInsideRectangle(new VecD(X1 + HalfRectWidth, Y1 + HalfRectHeight), new(X2 - X1, Y2 - Y1), Angle * Math.PI / 180, 32);
        chunks.ExceptWith(innerChunks);
        foreach (var chunk in chunks)
        {
            Rectangle rectangle = new()
            {
                Fill = Brushes.Green,
                Width = 32,
                Height = 32,
            };
            Canvas.SetLeft(rectangle, chunk.X * 32);
            Canvas.SetTop(rectangle, chunk.Y * 32);
            canvas.Children.Add(rectangle);
            rectangles.Add(rectangle);
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private bool drawing = false;
    private bool rotating = false;
    private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (rotating)
        {
            rotating = false;
            return;
        }
        drawing = true;
        Angle = 0;
        var pos = e.GetPosition(canvas);
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            X1 = pos.X;
            Y1 = pos.Y;
        }
        else
        {
            X1 = Math.Floor(pos.X / 32) * 32;
            Y1 = Math.Floor(pos.Y / 32) * 32;
        }
    }

    private void Canvas_MouseMove(object sender, MouseEventArgs e)
    {
        var pos = e.GetPosition(canvas);
        if (drawing)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                X2 = pos.X;
                Y2 = pos.Y;
            }
            else
            {
                X2 = Math.Floor(pos.X / 32) * 32;
                Y2 = Math.Floor(pos.Y / 32) * 32;
            }
        }
        else if (rotating)
        {
            VecD center = new VecD(X1 + HalfRectWidth, Y1 + HalfRectHeight);
            Angle = new VecD(pos.X - center.X, pos.Y - center.Y).CCWAngleTo(new VecD(X2 - center.X, Y2 - center.Y)) * -180 / Math.PI;
        }
        UpdateChunks();
    }

    private void Canvas_MouseUp(object sender, MouseButtonEventArgs e)
    {
        if (drawing)
        {
            drawing = false;
            rotating = true;
        }
    }
}
