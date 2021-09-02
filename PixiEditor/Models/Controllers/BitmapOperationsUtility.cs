using PixiEditor.Helpers.Extensions;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.ImageManipulation;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using PixiEditor.Models.Tools;
using PixiEditor.Models.Tools.ToolSettings.Settings;
using PixiEditor.Models.Undo;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace PixiEditor.Models.Controllers
{
    public class BitmapOperationsUtility
    {
        public List<LayerChange> PreviewLayerChanges => previewLayerChanges;

        private List<LayerChange> previewLayerChanges;

        private Coordinates lastMousePos;

        private SizeSetting sizeSetting;

        public BitmapOperationsUtility(BitmapManager manager)
        {
            Manager = manager;
        }

        public event EventHandler<BitmapChangedEventArgs> BitmapChanged;

        public BitmapManager Manager { get; set; }

        public void DeletePixels(Layer[] layers, Coordinates[] pixels)
        {
            if (Manager.ActiveDocument == null)
            {
                return;
            }

            BitmapPixelChanges changes = BitmapPixelChanges.FromSingleColoredArray(pixels, SKColors.Empty);
            Dictionary<Guid, SKColor[]> oldValues = BitmapUtils.GetPixelsForSelection(layers, pixels);
            LayerChange[] old = new LayerChange[layers.Length];
            LayerChange[] newChange = new LayerChange[layers.Length];
            for (int i = 0; i < layers.Length; i++)
            {
                Guid guid = layers[i].LayerGuid;
                old[i] = new LayerChange(
                    BitmapPixelChanges.FromArrays(pixels, oldValues[layers[i].LayerGuid]), guid);
                newChange[i] = new LayerChange(changes, guid);
                layers[i].SetPixels(changes);
            }

            Manager.ActiveDocument.UndoManager.AddUndoChange(new Change("UndoChanges", old, newChange, "Deleted pixels"));
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

                lastMousePos = newPos;
            }
        }

        /// <summary>
        ///     Applies pixels from preview layer to selected layer.
        /// </summary>
        public void ApplyPreviewLayer()
        {
            if (previewLayerChanges == null)
            {
                return;
            }

            Layer[] layers = new Layer[previewLayerChanges.Count];

            for (int i = 0; i < layers.Length; i++)
            {
                layers[i] = Manager.ActiveDocument.Layers.First(x => x.LayerGuid == previewLayerChanges[i].LayerGuid);
            }

            if (layers.Length > 0)
            {
                IEnumerable<LayerChange> oldValues =
                    ApplyToLayers(layers, previewLayerChanges.ToArray());

                foreach (var oldValue in oldValues)
                {
                    var previewChanges = previewLayerChanges.First(x => x.LayerGuid == oldValue.LayerGuid);

                    BitmapChanged?.Invoke(this, new BitmapChangedEventArgs(
                        previewChanges.PixelChanges,
                        oldValue.PixelChanges,
                        previewChanges.LayerGuid));
                }

                Manager.ActiveDocument.GeneratePreviewLayer();
            }

            previewLayerChanges = null;
        }

        private void UseTool(List<Coordinates> mouseMoveCords, BitmapOperationTool tool, SKColor color)
        {
            if (sizeSetting == null)
            {
                sizeSetting = tool.Toolbar.GetSetting<SizeSetting>("ToolSize");
            }

            int thickness = sizeSetting != null ? sizeSetting.Value : 1;

            bool shiftDown = Keyboard.IsKeyDown(Key.LeftShift);

            if (shiftDown)
            {
                bool mouseInLine = MouseCordsNotInLine(mouseMoveCords, thickness);

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
                LayerChange[] modifiedLayers = tool.Use(Manager.ActiveLayer, mouseMoveCords, color);
                LayerChange[] oldPixelsValues = new LayerChange[modifiedLayers.Length];
                for (int i = 0; i < modifiedLayers.Length; i++)
                {
                    Layer layer = Manager.ActiveDocument.Layers.First(x => x.LayerGuid == modifiedLayers[i].LayerGuid);
                    oldPixelsValues[i] = ApplyToLayer(layer, modifiedLayers[i]);

                    BitmapChanged?.Invoke(this, new BitmapChangedEventArgs(
                        modifiedLayers[i].PixelChanges,
                        oldPixelsValues[i].PixelChanges,
                        modifiedLayers[i].LayerGuid));
                }
            }
            else
            {
                UseToolOnPreviewLayer(mouseMoveCords, tool.ClearPreviewLayerOnEachIteration);
            }
        }

        private LayerChange ApplyToLayer(Layer layer, LayerChange change)
        {
            return ApplyToLayers(new Layer[] { layer }, new LayerChange[] { change })[0];
        }

        private LayerChange[] ApplyToLayers(Layer[] layers, LayerChange[] changes)
        {
            LayerChange[] oldPixelValues = new LayerChange[changes.Length];
            for (int i = 0; i < layers.Length; i++)
            {
                Layer layer = layers[i];
                LayerChange change = changes.First(x => x.LayerGuid == layer.LayerGuid);
                layer.DynamicResize(change.PixelChanges);

                oldPixelValues[i] = new LayerChange(
                GetOldPixelsValues(change.PixelChanges.ChangedPixels.Keys.ToArray()),
                change.LayerGuid);
            }

            for (int i = 0; i < layers.Length; i++)
            {
                Layer layer = layers[i];
                LayerChange change = changes.First(x => x.LayerGuid == layer.LayerGuid);
                layer.SetPixels(change.PixelChanges, false);
            }

            return oldPixelValues;
        }

        private bool MouseCordsNotInLine(List<Coordinates> cords, int thickness)
        {
            return (cords[0].X > cords[^1].X - thickness && cords[0].X < cords[^1].X + thickness)
                || (cords[0].Y > cords[^1].Y - thickness && cords[0].Y < cords[^1].Y + thickness);
        }

        private List<Coordinates> GetLineCoordinates(List<Coordinates> mouseMoveCords, int thickness)
        {
            int y = mouseMoveCords[0].Y;
            int x = mouseMoveCords[0].X;


            if (Math.Abs(mouseMoveCords[^1].X - mouseMoveCords[0].X) - thickness > 0)
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
            int xLength = mouseMoveCords[0].Y - mouseMoveCords[^1].Y;
            int yLength = mouseMoveCords[0].Y - mouseMoveCords[^1].Y;
            if (mouseMoveCords[^1].Y > mouseMoveCords[0].Y)
            {
                xLength *= -1;
            }

            if (mouseMoveCords[^1].X > mouseMoveCords[0].X)
            {
                xLength *= -1;
            }

            mouseMoveCords[0] = new Coordinates(mouseMoveCords[^1].X + xLength, mouseMoveCords[^1].Y + yLength);
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
            LayerChange[] modifiedLayers;
            if (mouseMove.Count > 0 && mouseMove[0] != lastMousePos)
            {
                if (clearPreviewLayer || Manager.ActiveDocument.PreviewLayer == null)
                {
                    Manager.ActiveDocument.GeneratePreviewLayer();
                }

                modifiedLayers = ((BitmapOperationTool)Manager.SelectedTool).Use(
                    Manager.ActiveDocument.ActiveLayer,
                    mouseMove,
                    Manager.PrimaryColor);

                BitmapPixelChanges[] changes = modifiedLayers.Select(x => x.PixelChanges).ToArray();
                if (changes.Length == 0)
                {
                    return;
                }

                Manager.ActiveDocument.PreviewLayer.SetPixels(BitmapPixelChanges.CombineOverride(changes));

                if (clearPreviewLayer || previewLayerChanges == null)
                {
                    previewLayerChanges = new List<LayerChange>(modifiedLayers);
                }
                else
                {
                    InjectPreviewLayerChanges(modifiedLayers);
                }
            }
        }

        private void InjectPreviewLayerChanges(LayerChange[] modifiedLayers)
        {
            for (int i = 0; i < modifiedLayers.Length; i++)
            {
                var layer = previewLayerChanges.First(x => x.LayerGuid == modifiedLayers[i].LayerGuid);
                layer.PixelChanges.ChangedPixels.AddRangeOverride(modifiedLayers[i].PixelChanges.ChangedPixels);
                layer.PixelChanges = layer.PixelChanges.WithoutTransparentPixels();
            }
        }
    }
}
