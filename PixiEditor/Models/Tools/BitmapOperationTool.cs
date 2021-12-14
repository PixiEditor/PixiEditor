using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using PixiEditor.Models.Undo;
using SkiaSharp;
using System.Collections.Generic;

namespace PixiEditor.Models.Tools
{
    public abstract class BitmapOperationTool : Tool
    {
        public bool RequiresPreviewLayer { get; set; }

        public bool ClearPreviewLayerOnEachIteration { get; set; } = true;

        public bool UseDefaultUndoMethod { get; set; } = true;

        private StorageBasedChange _change;

        public abstract void Use(Layer activeLayer, Layer previewLayer, IEnumerable<Layer> allLayers, IReadOnlyList<Coordinates> recordedMouseMovement, SKColor color);

        public override void BeforeUse()
        {
            if (UseDefaultUndoMethod && !RequiresPreviewLayer)
            {
                InitializeStorageBasedChange();
            }
        }

        /// <summary>
        /// Executes undo adding procedure.
        /// </summary>
        /// <remarks>When overriding, set UseDefaultUndoMethod to false.</remarks>
        public override void AfterUse()
        {
            if (!UseDefaultUndoMethod)
                return;

            if (RequiresPreviewLayer)
            {
                InitializeStorageBasedChange();
            }

            var document = ViewModels.ViewModelMain.Current.BitmapManager.ActiveDocument;
            var args = new object[] { _change.Document };
            document.UndoManager.AddUndoChange(_change.ToChange(UndoStorageBasedChange, args));
            _change = null;
        }

        private void InitializeStorageBasedChange()
        {
            Document doc = ViewModels.ViewModelMain.Current.BitmapManager.ActiveDocument;
            _change = new StorageBasedChange(doc, new[] {doc.ActiveLayer}, true);
        }

        private void UndoStorageBasedChange(Layer[] layers, UndoLayer[] data, object[] args)
        {
            Document document = (Document)args[0];
            var ls = document.LayerStructure.CloneGroups();
            StorageBasedChange.BasicUndoProcess(layers, data, args);
            document.BuildLayerStructureProcess(new object[] { ls });
        }
    }
}
