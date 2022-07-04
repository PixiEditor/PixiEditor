using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using PixiEditor.Models.Tools;
using PixiEditor.Models.Undo;
using PixiEditor.ViewModels.SubViewModels.Main;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Windows;

namespace PixiEditor.Models.Controllers
{
    public class BitmapOperationsUtility
    {
        public event EventHandler BitmapChanged;

        public BitmapManager Manager { get; set; }

        public ToolsViewModel Tools { get; set; }

        public BitmapOperationsUtility(BitmapManager manager, ToolsViewModel tools)
        {
            Manager = manager;
            Tools = tools;
        }

        public void DeletePixels(Layer[] layers, Coordinates[] pixels)
        {
            if (Manager.ActiveDocument == null)
            {
                return;
            }

            StorageBasedChange change = new StorageBasedChange(Manager.ActiveDocument, layers, true);

            BitmapPixelChanges changes = BitmapPixelChanges.FromSingleColoredArray(pixels, SKColors.Empty);
            for (int i = 0; i < layers.Length; i++)
            {
                Guid guid = layers[i].GuidValue;
                layers[i].SetPixels(changes);
            }

            var args = new object[] { change.Document };
            Manager.ActiveDocument.UndoManager.AddUndoChange(change.ToChange(StorageBasedChange.BasicUndoProcess, args, "Delete selected pixels"));
        }

        public void UseTool(IReadOnlyList<Coordinates> recordedMouseMovement, BitmapOperationTool tool, SKColor color)
        {
            if (Manager.ActiveDocument.Layers.Count == 0)
                return;

            if (!tool.RequiresPreviewLayer)
            {
                tool.Use(Manager.ActiveLayer, null, Manager.ActiveDocument.Layers, recordedMouseMovement, color);
                BitmapChanged?.Invoke(this, null);
            }
            else
            {
                UseToolOnPreviewLayer(recordedMouseMovement, tool.ClearPreviewLayerOnEachIteration);
            }
        }

        private void UseToolOnPreviewLayer(IReadOnlyList<Coordinates> recordedMouseMovement, bool clearPreviewLayer)
        {
            if (recordedMouseMovement.Count > 0)
            {
                if (clearPreviewLayer)
                {
                    Manager.ActiveDocument.PreviewLayer.ClearCanvas();
                }

                ((BitmapOperationTool)Tools.ActiveTool).Use(
                    Manager.ActiveLayer,
                    Manager.ActiveDocument.PreviewLayer,
                    Manager.ActiveDocument.Layers,
                    recordedMouseMovement,
                    Manager.PrimaryColor);
            }
        }

        /// <summary>
        ///     Applies pixels from preview layer to selected layer.
        /// </summary>
        public void ApplyPreviewLayer()
        {
            var previewLayer = Manager.ActiveDocument.PreviewLayer;
            var activeLayer = Manager.ActiveLayer;

            Int32Rect dirtyRect = new Int32Rect(previewLayer.OffsetX, previewLayer.OffsetY, previewLayer.Width, previewLayer.Height);
            activeLayer.DynamicResizeAbsolute(dirtyRect);
            previewLayer.LayerBitmap.SkiaSurface.Draw(
                    activeLayer.LayerBitmap.SkiaSurface.Canvas,
                    previewLayer.OffsetX - activeLayer.OffsetX,
                    previewLayer.OffsetY - activeLayer.OffsetY,
                    Surface.BlendingPaint
                );

            Manager.ActiveLayer.InvokeLayerBitmapChange(dirtyRect);
            BitmapChanged?.Invoke(this, null);
        }
    }
}
