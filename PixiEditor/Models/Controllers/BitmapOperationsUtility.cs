using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.ImageManipulation;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using PixiEditor.Models.Tools;

namespace PixiEditor.Models.Controllers
{
    public class BitmapOperationsUtility
    {
        private LayerChange[] lastModifiedLayers;

        private Coordinates lastMousePos;

        public BitmapOperationsUtility(BitmapManager manager)
        {
            Manager = manager;
        }

        public event EventHandler<BitmapChangedEventArgs> BitmapChanged;

        public BitmapManager Manager { get; set; }

        public void DeletePixels(Layer[] layers, Coordinates[] pixels)
        {
            BitmapPixelChanges changes = BitmapPixelChanges.FromSingleColoredArray(pixels, Color.FromArgb(0, 0, 0, 0));
            Dictionary<Layer, Color[]> oldValues = BitmapUtils.GetPixelsForSelection(layers, pixels);
            LayerChange[] old = new LayerChange[layers.Length];
            LayerChange[] newChange = new LayerChange[layers.Length];
            for (int i = 0; i < layers.Length; i++)
            {
                old[i] = new LayerChange(
                    BitmapPixelChanges.FromArrays(pixels, oldValues[layers[i]]), i);
                newChange[i] = new LayerChange(changes, i);
                layers[i].SetPixels(changes);
            }

            UndoManager.AddUndoChange(new Change("UndoChanges", old, newChange, "Deleted pixels"));
        }

        /// <summary>
        ///     Executes tool Use() method with given parameters. NOTE: mouseMove is reversed inside function!.
        /// </summary>
        /// <param name="newPos">Most recent coordinates.</param>
        /// <param name="mouseMove">Last mouse movement coordinates.</param>
        /// <param name="tool">Tool to execute.</param>
        public void ExecuteTool(Coordinates newPos, List<Coordinates> mouseMove, BitmapOperationTool tool)
        {
            if (Manager.ActiveDocument != null && tool != null && tool.ToolType != ToolType.None)
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
            if (lastModifiedLayers == null)
            {
                return;
            }

            for (int i = 0; i < lastModifiedLayers.Length; i++)
            {
                Layer layer = Manager.ActiveDocument.Layers[lastModifiedLayers[i].LayerIndex];

                BitmapPixelChanges oldValues = ApplyToLayer(layer, lastModifiedLayers[i]).PixelChanges;

                BitmapChanged?.Invoke(this, new BitmapChangedEventArgs(
                    lastModifiedLayers[i].PixelChanges,
                    oldValues,
                    lastModifiedLayers[i].LayerIndex));
                Manager.PreviewLayer = null;
            }
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
                    Layer layer = Manager.ActiveDocument.Layers[modifiedLayers[i].LayerIndex];
                    oldPixelsValues[i] = ApplyToLayer(layer, modifiedLayers[i]);

                    BitmapChanged?.Invoke(this, new BitmapChangedEventArgs(
                        modifiedLayers[i].PixelChanges,
                        oldPixelsValues[i].PixelChanges,
                        modifiedLayers[i].LayerIndex));
                }
            }
            else
            {
                UseToolOnPreviewLayer(mouseMoveCords);
            }
        }

        private LayerChange ApplyToLayer(Layer layer, LayerChange change)
        {
            layer.DynamicResize(change.PixelChanges);

            LayerChange oldPixelsValues = new LayerChange(
                GetOldPixelsValues(change.PixelChanges.ChangedPixels.Keys.ToArray()),
                change.LayerIndex);

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

        private void UseToolOnPreviewLayer(List<Coordinates> mouseMove)
        {
            LayerChange[] modifiedLayers;
            if (mouseMove.Count > 0 && mouseMove[0] != lastMousePos)
            {
                Manager.GeneratePreviewLayer();
                modifiedLayers = ((BitmapOperationTool)Manager.SelectedTool).Use(
                    Manager.ActiveDocument.ActiveLayer,
                    mouseMove.ToArray(),
                    Manager.PrimaryColor);
                BitmapPixelChanges[] changes = modifiedLayers.Select(x => x.PixelChanges).ToArray();
                Manager.PreviewLayer.SetPixels(BitmapPixelChanges.CombineOverride(changes));
                lastModifiedLayers = modifiedLayers;
            }
        }
    }
}