using System;
using System.Collections.Generic;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using PixiEditor.Models.Undo;
using SkiaSharp;

namespace PixiEditor.Models.Tools
{
    public abstract class BitmapOperationTool : Tool
    {
        public bool RequiresPreviewLayer { get; set; }

        public bool ClearPreviewLayerOnEachIteration { get; set; } = true;

        public bool UseDefaultUndoMethod { get; set; } = true;
        public virtual bool UsesShift => true;

        private StorageBasedChange _change;


        public abstract void Use(Layer layer, List<Coordinates> mouseMove, SKColor color);

        public override void AddUndoProcess(Document document)
        {
            var args = new object[] { _change.Document };
            document.UndoManager.AddUndoChange(_change.ToChange(UndoProcess, args));
            _change = null;
        }

        public override void OnRecordingLeftMouseDown(MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;

            Document doc = ViewModels.ViewModelMain.Current.BitmapManager.ActiveDocument;
            _change = new StorageBasedChange(doc, new[] { doc.ActiveLayer }, true);
        }

        private void UndoProcess(Layer[] layers, UndoLayer[] data, object[] args)
        {
            if (args.Length > 0 && args[0] is Document document)
            {
                for (int i = 0; i < layers.Length; i++)
                {
                    Layer layer = layers[i];
                    document.Layers.RemoveAt(data[i].LayerIndex);

                    document.Layers.Insert(data[i].LayerIndex, layer);
                    if (data[i].IsActive)
                    {
                        document.SetMainActiveLayer(data[i].LayerIndex);
                    }
                }

            }
        }
    }
}
