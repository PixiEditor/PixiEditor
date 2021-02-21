using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PixiEditor.Helpers.Extensions;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.ImageManipulation;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using PixiEditor.Models.Tools;
using PixiEditor.Models.Undo;

namespace PixiEditor.Models.Controllers
{
    public class BitmapOperationsUtility
    {
        public List<LayerChange> PreviewLayerChanges => previewLayerChanges;

        private List<LayerChange> previewLayerChanges;

        private Coordinates lastMousePos;

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

            BitmapPixelChanges changes = BitmapPixelChanges.FromSingleColoredArray(pixels, Color.FromArgb(0, 0, 0, 0));
            Dictionary<Guid, Color[]> oldValues = BitmapUtils.GetPixelsForSelection(layers, pixels);
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
        ///     Executes tool Use() method with given parameters. NOTE: mouseMove is reversed inside function!.
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

                mouseMove.Reverse();
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

            foreach (var modifiedLayer in previewLayerChanges)
            {
                Layer layer = Manager.ActiveDocument.Layers.FirstOrDefault(x => x.LayerGuid == modifiedLayer.LayerGuid);

                if (layer != null)
                {
                    BitmapPixelChanges oldValues = ApplyToLayer(layer, modifiedLayer).PixelChanges;

                    BitmapChanged?.Invoke(this, new BitmapChangedEventArgs(
                        modifiedLayer.PixelChanges,
                        oldValues,
                        modifiedLayer.LayerGuid));
                    Manager.ActiveDocument.GeneratePreviewLayer();
                }
            }

            previewLayerChanges = null;
        }

        private void UseTool(List<Coordinates> mouseMoveCords, BitmapOperationTool tool, Color color)
        {
            if (Keyboard.IsKeyDown(Key.LeftShift) && !MouseCordsNotInLine(mouseMoveCords))
            {
                mouseMoveCords = GetSquareCoordiantes(mouseMoveCords);
            }

            if (!tool.RequiresPreviewLayer)
            {
                LayerChange[] modifiedLayers = tool.Use(Manager.ActiveLayer, mouseMoveCords.ToArray(), color);
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
            layer.DynamicResize(change.PixelChanges);

            LayerChange oldPixelsValues = new LayerChange(
                GetOldPixelsValues(change.PixelChanges.ChangedPixels.Keys.ToArray()),
                change.LayerGuid);

            layer.SetPixels(change.PixelChanges, false);
            return oldPixelsValues;
        }

        private bool MouseCordsNotInLine(List<Coordinates> cords)
        {
            return cords[0].X == cords[^1].X || cords[0].Y == cords[^1].Y;
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
            Dictionary<Coordinates, Color> values = new Dictionary<Coordinates, Color>();
            using (Manager.ActiveLayer.LayerBitmap.GetBitmapContext(ReadWriteMode.ReadOnly))
            {
                Coordinates[] relativeCoords = Manager.ActiveLayer.ConvertToRelativeCoordinates(coordinates);
                for (int i = 0; i < coordinates.Length; i++)
                {
                    values.Add(
                        coordinates[i],
                        Manager.ActiveLayer.GetPixel(relativeCoords[i].X, relativeCoords[i].Y));
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
                    mouseMove.ToArray(),
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