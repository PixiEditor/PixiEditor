using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using PixiEditor.Models.Tools;
using PixiEditor.Models.Tools.ToolSettings.Settings;
using PixiEditor.Models.Undo;
using PixiEditor.ViewModels.SubViewModels.Main;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace PixiEditor.Models.Controllers
{
    public class BitmapOperationsUtility
    {
        private SKPaint BlendingPaint { get; } = new SKPaint() { BlendMode = SKBlendMode.SrcOver };


        private SizeSetting sizeSetting;

        public BitmapOperationsUtility(BitmapManager manager, ToolsViewModel tools)
        {
            Manager = manager;
            Tools = tools;
        }

        public event EventHandler<BitmapChangedEventArgs> BitmapChanged;

        public BitmapManager Manager { get; set; }

        public ToolsViewModel Tools { get; set; }

        public void DeletePixels(Layer[] layers, Coordinates[] pixels)
        {
            if (Manager.ActiveDocument == null)
            {
                return;
            }

            StorageBasedChange change = new StorageBasedChange(Manager.ActiveDocument, layers, true);
            

            // TODO: Fix
            BitmapPixelChanges changes = BitmapPixelChanges.FromSingleColoredArray(pixels, SKColors.Empty);
            //Dictionary<Guid, SKColor[]> oldValues = BitmapUtils.GetPixelsForSelection(layers, pixels);
            //LayerChange[] old = new LayerChange[layers.Length];
            //LayerChange[] newChange = new LayerChange[layers.Length];
            for (int i = 0; i < layers.Length; i++)
            {
                Guid guid = layers[i].LayerGuid;
                //old[i] = new LayerChange(
                    //BitmapPixelChanges.FromArrays(pixels, oldValues[layers[i].LayerGuid]), guid);
                //newChange[i] = new LayerChange(changes, guid);
                layers[i].SetPixels(changes);
            }

            var args = new object[] { change.Document };
            Manager.ActiveDocument.UndoManager.AddUndoChange(change.ToChange(StorageBasedChange.BasicUndoProcess, args, "Delete selected pixels"));
        }

        /// <summary>
        ///     Executes tool Use() method with given parameters. NOTE: [0] is a start point, [^1] is latest.
        /// </summary>
        /// <param name="newPos">Most recent coordinates.</param>
        /// <param name="mouseMove">Last mouse movement coordinates.</param>
        /// <param name="tool">Tool to execute.</param>
        public void ExecuteTool(Coordinates newPos, List<Coordinates> mouseMove, BitmapOperationTool tool)
        {
            if (Manager.ActiveDocument != null && tool != null)
            {
                if (Manager.ActiveDocument.Layers.Count == 0 || mouseMove.Count == 0)
                {
                    return;
                }

                UseTool(mouseMove, tool, Manager.PrimaryColor);
            }
        }

        /// <summary>
        ///     Applies pixels from preview layer to selected layer.
        /// </summary>
        public void ApplyPreviewLayer()
        {
            var previewLayer = Manager.ActiveDocument.PreviewLayer;
            var activeLayer = Manager.ActiveLayer;

            activeLayer.DynamicResizeAbsolute(previewLayer.OffsetX + previewLayer.Width, previewLayer.OffsetY + previewLayer.Height, previewLayer.OffsetX, previewLayer.OffsetY);
            previewLayer.LayerBitmap.SkiaSurface.Draw(
                    activeLayer.LayerBitmap.SkiaSurface.Canvas,
                    previewLayer.OffsetX - activeLayer.OffsetX,
                    previewLayer.OffsetY - activeLayer.OffsetY,
                    BlendingPaint
                );
            Manager.ActiveLayer.InvokeLayerBitmapChange(new Int32Rect(previewLayer.OffsetX, previewLayer.OffsetY, previewLayer.Width, previewLayer.Height));
            // Don't forget about firing BitmapChanged
            BitmapChanged?.Invoke(this, null);
            Manager.ActiveDocument.PreviewLayer.Reset();
        }

        private void UseTool(List<Coordinates> mouseMoveCords, BitmapOperationTool tool, SKColor color)
        {
            if (sizeSetting == null)
            {
                sizeSetting = tool.Toolbar.GetSetting<SizeSetting>("ToolSize");
            }

            int thickness = sizeSetting != null ? sizeSetting.Value : 1;

            bool shiftDown = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);

            if (shiftDown && tool.UsesShift)
            {
                bool mouseInLine = DoCoordsFormLine(mouseMoveCords, thickness);

                if (!mouseInLine)
                {
                    mouseMoveCords = GetSquareCoordiantes(mouseMoveCords);
                }
                else
                {
                    mouseMoveCords = GetLineCoordinates(mouseMoveCords, thickness);
                }
            }

            if (!tool.RequiresPreviewLayer)
            {
                if (!Manager.ActiveDocument.PreviewLayer.IsReset)
                    Manager.ActiveDocument.PreviewLayer.Reset();
                tool.Use(Manager.ActiveLayer, mouseMoveCords, color);
                BitmapChanged?.Invoke(this, null);
            }
            else
            {
                UseToolOnPreviewLayer(mouseMoveCords, tool.ClearPreviewLayerOnEachIteration);
            }
        }

        private bool DoCoordsFormLine(List<Coordinates> coords, int thickness)
        {
            var p1 = coords[0];
            var p2 = coords[^1];
            //find delta and mirror to first quadrant
            float dX = Math.Abs(p2.X - p1.X);
            float dY = Math.Abs(p2.Y - p1.Y);

            //normalize
            float length = (float)Math.Sqrt(dX * dX + dY * dY);
            if (length == 0)
                return false;
            dX = dX / length;
            dY = dY / length;

            return dX < 0.25f || dY < 0.25f; //angle < 15 deg or angle > 75 deg (sin 15 ~= 0.25)
        }

        private List<Coordinates> GetLineCoordinates(List<Coordinates> mouseMoveCords, int thickness)
        {
            int y = mouseMoveCords[0].Y;
            int x = mouseMoveCords[0].X;

            if (Math.Abs(mouseMoveCords[^1].X - mouseMoveCords[0].X) > Math.Abs(mouseMoveCords[^1].Y - mouseMoveCords[0].Y))
            {
                y = mouseMoveCords[^1].Y;
            }
            else
            {
                x = mouseMoveCords[^1].X;
            }

            mouseMoveCords[0] = new Coordinates(x, y);
            return mouseMoveCords;
        }

        /// <summary>
        ///     Extracts square from rectangle mouse drag, used to draw symmetric shapes.
        /// </summary>
        private List<Coordinates> GetSquareCoordiantes(List<Coordinates> mouseMoveCords)
        {
            var p1 = mouseMoveCords[0];
            var p2 = mouseMoveCords[^1];

            //find delta and mirror to first quadrant
            var dX = Math.Abs(p2.X - p1.X);
            var dY = Math.Abs(p2.Y - p1.Y);

            float sqrt2 = (float)Math.Sqrt(2);
            //vector of length 1 at 45 degrees;
            float diagX, diagY;
            diagX = diagY = 1 / sqrt2;

            //dot product of delta and diag, returns length of [delta projected onto diag]
            float projectedLength = diagX * dX + diagY * dY;
            //project above onto axes
            float axisLength = projectedLength / sqrt2;

            //final coords
            float x = -Math.Sign(p2.X - p1.X) * axisLength;
            float y = -Math.Sign(p2.Y - p1.Y) * axisLength;
            mouseMoveCords[0] = new Coordinates((int)x + p2.X, (int)y + p2.Y);
            return mouseMoveCords;
        }

        private BitmapPixelChanges GetOldPixelsValues(Coordinates[] coordinates)
        {
            Dictionary<Coordinates, SKColor> values = new Dictionary<Coordinates, SKColor>();
            //using (Manager.ActiveLayer.LayerBitmap.GetBitmapContext(ReadWriteMode.ReadOnly))
            {
                Coordinates[] relativeCoords = Manager.ActiveLayer.ConvertToRelativeCoordinates(coordinates);
                for (int i = 0; i < coordinates.Length; i++)
                {
                    var cl = Manager.ActiveLayer.GetPixel(relativeCoords[i].X, relativeCoords[i].Y);
                    values.Add(
                        coordinates[i],
                        cl);
                }
            }

            return new BitmapPixelChanges(values);
        }

        private void UseToolOnPreviewLayer(List<Coordinates> mouseMove, bool clearPreviewLayer = true)
        {
            if (mouseMove.Count > 0)
            {
                if (clearPreviewLayer)
                {
                    Manager.ActiveDocument.PreviewLayer.ClearCanvas();
                }

                ((BitmapOperationTool)Tools.ActiveTool).Use(
                    Manager.ActiveDocument.PreviewLayer,
                    mouseMove,
                    Manager.PrimaryColor);
            }
        }
    }
}
