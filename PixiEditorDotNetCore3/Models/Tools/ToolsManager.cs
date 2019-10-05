using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using PixiEditor.ViewModels;
using System.Timers;
using System.Windows.Threading;
using System.Threading;

namespace PixiEditorDotNetCore3.Models.Tools
{
    public class ToolsManager
    {
        public List<Tool> Tools { get; set; } = new List<Tool>();
        public Tool SelectedTool { get; private set; }
        private bool _toolRecievedData = false;

        private System.Timers.Timer _loopTimer;
        private Layer _layer;
        private WriteableBitmap _clonedBitmap;
        private Coordinates _startCoordinates;
        private Color _color;
        private int _toolSzie;

        public ToolsManager(List<Tool> tools)
        {
            Tools = tools;
            _loopTimer = new System.Timers.Timer
            {
                Interval = 15,
                Enabled = false,
                AutoReset = true
            };
            _loopTimer.Elapsed += LoopTimer_Elapsed;
        }

        private void LoopTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {              
                if(_clonedBitmap != null)
                {
                    _layer.LayerBitmap.Clear();
                    _layer.LayerBitmap.Blit(new Rect(new Size(_layer.Width, _layer.Height)), _clonedBitmap, new Rect(new Size(_layer.Width, _layer.Height)), WriteableBitmapExtensions.BlendMode.Additive);
                }
                BitmapPixelChanges changes = SelectedTool.Use(_layer, _startCoordinates, _color, _toolSzie);
                if (!SelectedTool.ExecutesItself)
                {
                    _layer.ApplyPixels(changes, _color);
                }
            });
        }

        public void SetTool(ToolType tool)
        {
              SelectedTool = Tools.Find(x => x.ToolType == tool);
        }

        public void StopExectuingTool()
        {
            _loopTimer.Enabled = false;
            _toolRecievedData = false;
            _clonedBitmap = null;
        }

        private void StartTimer()
        {
            _toolRecievedData = true;
            _loopTimer.Enabled = true;
        }

        private void CloneBitmapIfToolIsShape()
        {
            if (SelectedTool.IsShapeCreating == true)
            {
                _clonedBitmap = _layer.LayerBitmap.Clone();                
            }
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
        public void ExecuteTool(Layer layer, Coordinates startingCoords, Color color, int toolSize)
        {
            if (toolSize < 1)
                return;

            if(_toolRecievedData == false || (_toolRecievedData == true && !SelectedTool.IsShapeCreating))
            {
                _startCoordinates = startingCoords;
                _layer = layer;
                _color = color;
                _toolSzie = toolSize;
            }
                     

            if (_loopTimer.Enabled == false)
            {
                StartTimer();
                CloneBitmapIfToolIsShape();
            }
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
