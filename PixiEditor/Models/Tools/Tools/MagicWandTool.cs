using PixiEditor.Helpers;
using PixiEditor.Helpers.Extensions;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Enums;
using PixiEditor.Models.ImageManipulation;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using PixiEditor.Models.Tools.ToolSettings.Toolbars;
using PixiEditor.ViewModels;
using SkiaSharp;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;

namespace PixiEditor.Models.Tools.Tools
{
    internal class MagicWandTool : ReadonlyTool, ICachedDocumentTool
    {
        private static Selection ActiveSelection { get => ViewModelMain.Current.BitmapManager.ActiveDocument.ActiveSelection; }

        private BitmapManager BitmapManager { get; }

        private IEnumerable<Coordinates> oldSelection;
        private List<Coordinates> newSelection = new List<Coordinates>();

        public override string Tooltip => "Magic Wand (W). Flood's the selection";

        private Layer cachedDocument;

        public MagicWandTool(BitmapManager manager)
        {
            BitmapManager = manager;

            Toolbar = new MagicWandToolbar();

            ActionDisplay = "Click to flood the selection.";
        }

        public override void Use(IReadOnlyList<Coordinates> pixels)
        {
            if (pixels.Count > 1)
                return;

            oldSelection = new ReadOnlyCollection<Coordinates>(ActiveSelection.SelectedPoints);

            SelectionType selectionType = Toolbar.GetEnumSetting<SelectionType>("SelectMode").Value;
            DocumentScope documentScope = Toolbar.GetEnumSetting<DocumentScope>(nameof(DocumentScope)).Value;

            Document document = BitmapManager.ActiveDocument;
            Layer layer;

            if (documentScope == DocumentScope.SingleLayer)
            {
                layer = BitmapManager.ActiveLayer;
            }
            else
            {
                ValidateCache(document);
                layer = cachedDocument;
            }

            Selection selection = BitmapManager.ActiveDocument.ActiveSelection;


            newSelection.Clear();

            ToolCalculator.GetLinearFillAbsolute(
                   layer,
                   pixels[0],
                   BitmapManager.ActiveDocument.Width,
                   BitmapManager.ActiveDocument.Height,
                   SKColors.White,
                   newSelection);

            selection.SetSelection(newSelection, selectionType);

            SelectionHelpers.AddSelectionUndoStep(ViewModelMain.Current.BitmapManager.ActiveDocument, oldSelection, selectionType);
        }

        public void DocumentChanged()
        {
            cachedDocument = null;
        }

        private void ValidateCache(Document document)
        {
            cachedDocument ??= new Layer("_CombinedLayers", BitmapUtils.CombineLayers(
                new Int32Rect(0, 0, document.Width, document.Height),
                document.Layers,
                document.LayerStructure));
        }
    }
}
