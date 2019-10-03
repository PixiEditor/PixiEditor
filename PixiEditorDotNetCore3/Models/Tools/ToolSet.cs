using PixiEditor.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace PixiEditorDotNetCore3.Models.Tools
{
    public class ToolSet
    {
        public List<Tool> Tools { get; set; } = new List<Tool>();
        private Coordinates _activeCoordinates = new Coordinates();
        private bool _toolIsExecuting = false;
        private int _asyncDelay = 15;


        public ToolSet(List<Tool> tools)
        {
            Tools = tools;
        }

        /// <summary>
        /// Executes tool action
        /// </summary>
        /// <param name="layer">Layer to operate on.</param>
        /// <param name="startingCoords">Click coordinates.</param>
        /// <param name="color">Color that tool will use.</param>
        /// <param name="toolSize">Size/thickness of tool</param>
        /// <param name="tool">Tool to execute</param>
        /// <returns></returns>
        public void ExecuteTool(Layer layer, Coordinates startingCoords, Color color,int toolSize, ToolType tool)
        {
            if (toolSize < 1) return;
            BitmapPixelChanges changes;


            Tool selectedTool = Tools.Find(x => x.GetToolType() == tool);
            changes = selectedTool.Use(layer, startingCoords, color, toolSize);

            if (tool != ToolType.ColorPicker)
            {
                UndoManager.RecordChanges("ActiveLightLayer", new LightLayer(layer.LayerBitmap.ToByteArray(), (int)layer.LayerBitmap.Height, (int)layer.LayerBitmap.Width),
                    $"{tool.ToString()} Tool.");
            }

            layer.ApplyPixels(changes, color);

        }

        /// <summary>
        /// Not working yet.
        /// </summary>
        /// <param name="bitmap"></param>
        /// <param name="pixelCoordinates"></param>
        /// <param name="color"></param>
        /// <param name="highlightThickness"></param>
        public static void HighlightPixel(WriteableBitmap bitmap,Coordinates pixelCoordinates, Color color, int highlightThickness)
        {
            bitmap.Clear();
            bitmap.Blit(new Rect(new Size(bitmap.Width, bitmap.Height)), bitmap, new Rect(new Size(bitmap.Width, bitmap.Height)), WriteableBitmapExtensions.BlendMode.Additive);
            DoubleCords centerCords = CoordinatesCalculator.CalculateThicknessCenter(pixelCoordinates, highlightThickness);
            bitmap.FillRectangle(centerCords.Coords1.X, centerCords.Coords1.Y, centerCords.Coords2.X, centerCords.Coords2.Y, color);
        }

        /// <summary>
        /// Updates coordinates in order to some tools work
        /// </summary>
        /// <param name="cords">Current coordinates</param>
        public void UpdateCoordinates(Coordinates cords)
        {
            _activeCoordinates = cords;
        }
        

        /// <summary>
        /// Fills area with color (forest fire alghoritm)
        /// </summary>
        /// <param name="canvas">Bitmap to operate on</param>
        /// <param name="pixelPosition">Position of starting pixel</param>
        /// <param name="color">Fills area with this color</param>
        private WriteableBitmap FloodFill(WriteableBitmap canvas, Coordinates pixelPosition, Color color)
        {
            WriteableBitmap bm = canvas;
            Color colorToReplace = bm.GetPixel(pixelPosition.X, pixelPosition.Y);
            var stack = new Stack<Tuple<int, int>>();
            stack.Push(Tuple.Create(pixelPosition.X, pixelPosition.Y));

            while (stack.Count > 0)
            {
                var point = stack.Pop();
                if (point.Item1 < 0 || point.Item1 > bm.Height - 1) continue;
                if (point.Item2 < 0 || point.Item2 > bm.Width - 1) continue;
                if (bm.GetPixel(point.Item1, point.Item2) == color) continue;

                if (bm.GetPixel(point.Item1, point.Item2) == colorToReplace)
                {
                    bm.SetPixel(point.Item1, point.Item2, color);
                    stack.Push(Tuple.Create(point.Item1, point.Item2 - 1));
                    stack.Push(Tuple.Create(point.Item1 + 1, point.Item2));
                    stack.Push(Tuple.Create(point.Item1, point.Item2 + 1));
                    stack.Push(Tuple.Create(point.Item1 - 1, point.Item2));
                }
            }
            return bm;
        }

        /// <summary>
        /// Draws line in canvas 
        /// </summary>
        /// <param name="layer">Layer to operate on</param>
        /// <param name="coordinates">Starting coordinates, usually click point</param>
        /// <param name="color">Does it really need a description?</param> 
        private async void LineAsync(Layer layer, Coordinates coordinates, Color color, int size)
        {
            WriteableBitmap wb = layer.LayerBitmap;
            _toolIsExecuting = true;
            //clones bitmap before line
            WriteableBitmap writeableBitmap = wb.Clone();
            //While Mouse buttons are pressed, clears current bitmap, pastes cloned bitmap and draws line, on each iteration
            while (Mouse.LeftButton == MouseButtonState.Pressed || Mouse.RightButton == MouseButtonState.Pressed)
            {
                wb.Clear();
                wb.Blit(new Rect(new Size(layer.Width, layer.Height)), writeableBitmap, new Rect(new Size(layer.Width, layer.Height)), WriteableBitmapExtensions.BlendMode.Additive);
                    wb.DrawLineBresenham(coordinates.X, coordinates.Y, _activeCoordinates.X, _activeCoordinates.Y, color);
                await Task.Delay(_asyncDelay);
            }           
            _toolIsExecuting = false;
        }

        /// <summary>
        /// Draws circle on bitmap.
        /// </summary>
        /// <param name="layer">Layer to operate on.</param>
        /// <param name="coordinates">Starting pixel coordinates.</param>
        /// <param name="color">Circle color.</param>
        private async void CircleAsync(Layer layer, Coordinates coordinates, Color color)
        {
            WriteableBitmap wb = layer.LayerBitmap;
            //Basically does the same like rectangle method, but with different shape
            _toolIsExecuting = true;
            WriteableBitmap bitmap = wb.Clone();
            while (Mouse.LeftButton == MouseButtonState.Pressed || Mouse.RightButton == MouseButtonState.Pressed)
            {
                wb.Clear();
                wb.Blit(new Rect(new Size(layer.Width, layer.Height)), bitmap, new Rect(new Size(layer.Width, layer.Height)), WriteableBitmapExtensions.BlendMode.Additive);
                if (coordinates.X > _activeCoordinates.X && coordinates.Y > _activeCoordinates.Y)
                {
                    wb.DrawEllipse(_activeCoordinates.X, _activeCoordinates.Y, coordinates.X, coordinates.Y, color);
                }
                else if (coordinates.X < _activeCoordinates.X && coordinates.Y < _activeCoordinates.Y)
                {
                    wb.DrawEllipse(coordinates.X, coordinates.Y, _activeCoordinates.X, _activeCoordinates.Y, color);
                }
                else if (coordinates.Y > _activeCoordinates.Y)
                {
                    wb.DrawEllipse(coordinates.X, _activeCoordinates.Y, _activeCoordinates.X, coordinates.Y, color);
                }
                else
                {
                    wb.DrawEllipse(_activeCoordinates.X, coordinates.Y, coordinates.X, _activeCoordinates.Y, color);
                }
                await Task.Delay(_asyncDelay);
            }
            _toolIsExecuting = false;
        }

        /// <summary>
        /// Draws rectangle on bitmap
        /// </summary>
        /// <param name="layer">Layer to operate on</param>
        /// <param name="coordinates">Starting pixel coordinate</param>
        /// <param name="color">Rectangle color</param>
        private async void RectangleAsync(Layer layer, Coordinates coordinates, Color color)
        {
            WriteableBitmap wb = layer.LayerBitmap;
            _toolIsExecuting = true;
            WriteableBitmap writeableBitmap = wb.Clone();
            while (Mouse.LeftButton == MouseButtonState.Pressed || Mouse.RightButton == MouseButtonState.Pressed)
            {
                //Two lines below are responsible for clearing last rectangle (on mouse move), to live show rectangle on bitmap
                wb.Clear();
                wb.Blit(new Rect(new Size(layer.Width, layer.Height)), writeableBitmap, new Rect(new Size(layer.Width, layer.Height)), WriteableBitmapExtensions.BlendMode.Additive);
                //Those ifs are changing direction of rectangle. In other words: flips rectangle on X and Y axis when needed
                if (coordinates.X > _activeCoordinates.X && coordinates.Y > _activeCoordinates.Y)
                {
                    wb.DrawRectangle(_activeCoordinates.X, _activeCoordinates.Y, coordinates.X, coordinates.Y, color);
                }
                else if (coordinates.X < _activeCoordinates.X && coordinates.Y < _activeCoordinates.Y)
                {
                    wb.DrawRectangle(coordinates.X, coordinates.Y, _activeCoordinates.X, _activeCoordinates.Y, color);
                }
                else if (coordinates.Y > _activeCoordinates.Y)
                {
                    wb.DrawRectangle(coordinates.X, _activeCoordinates.Y, _activeCoordinates.X, coordinates.Y, color);
                }
                else
                {
                    wb.DrawRectangle(_activeCoordinates.X, coordinates.Y, coordinates.X, _activeCoordinates.Y, color);
                }
                await Task.Delay(_asyncDelay);
            }            
            _toolIsExecuting = false;
        }
        /// <summary>
        /// Returns color of pixel.
        /// </summary>
        /// <param name="layer">Layer in which bitmap with pixels are stored.</param>
        /// <param name="coordinates">Pixel coordinate.</param>
        /// <returns></returns>
        public static Color ColorPicker(Layer layer, Coordinates coordinates)
        {
            return layer.LayerBitmap.GetPixel(coordinates.X, coordinates.Y);
        }
        /// <summary>
        /// Ligtens pixel color.
        /// </summary>
        /// <param name="bitmap">Bitmap to work on.</param>
        /// <param name="coordinates">Pixel coordinates.</param>
        /// <returns></returns>
        private WriteableBitmap Lighten(WriteableBitmap bitmap, Coordinates coordinates)
        {
            WriteableBitmap wb = bitmap;
            Color pixel = wb.GetPixel(coordinates.X, coordinates.Y);
            Color newColor = ExColor.ChangeColorBrightness(System.Drawing.Color.FromArgb(pixel.R, pixel.G, pixel.B), 0.1f);
            wb.SetPixel(coordinates.X, coordinates.Y, newColor);
            return wb;
        }
        /// <summary>
        /// Darkens pixel color.
        /// </summary>
        /// <param name="bitmap">Bitmap to work on.</param>
        /// <param name="coordinates">Pixel coordinates.</param>
        /// <returns></returns>
        private WriteableBitmap Darken(WriteableBitmap bitmap, Coordinates coordinates)
        {
            WriteableBitmap wb = bitmap;
            Color pixel = wb.GetPixel(coordinates.X, coordinates.Y);
            Color newColor = ExColor.ChangeColorBrightness(System.Drawing.Color.FromArgb(pixel.R,pixel.G,pixel.B), -0.06f);
            wb.SetPixel(coordinates.X, coordinates.Y, newColor);
            return wb;
        }
    }
}
