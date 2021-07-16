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
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace PixiEditor.Models.Tools.Tools
{
    public class MagicWandTool : ReadonlyTool, ICachedDocumentTool
    {
        private readonly FloodFill floodFill;

        private static Selection ActiveSelection { get => ViewModelMain.Current.BitmapManager.ActiveDocument.ActiveSelection; }

        private BitmapManager BitmapManager { get; }

        private IEnumerable<Coordinates> oldSelection;

        public override string Tooltip => "Magic Wand (W). Flood's the selection";

        private Layer cachedDocument;

        public override void OnRecordingLeftMouseDown(MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
            {
                return;
            }

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

            selection.SetSelection(
                floodFill.ForestFire(
                    layer,
                    new Coordinates((int)document.MouseXOnCanvas, (int)document.MouseYOnCanvas),
                    System.Windows.Media.Colors.White
                    ).ChangedPixels.Keys,
                selectionType);

            SelectionHelpers.AddSelectionUndoStep(ViewModelMain.Current.BitmapManager.ActiveDocument, oldSelection, selectionType);
        }

        public MagicWandTool(BitmapManager manager)
        {
            floodFill = new FloodFill(manager);
            BitmapManager = manager;

            Toolbar = new MagicWandToolbar();

            ActionDisplay = "Click to flood the selection.";
        }

        public override void Use(List<Coordinates> pixels)
        {
        }

        public void DocumentChanged()
        {
            cachedDocument = null;
        }

        private void ValidateCache(Document document)
        {
            cachedDocument ??= new Layer("_CombinedLayers", BitmapUtils.CombineLayers(document.Width, document.Height, document.Layers, document.LayerStructure));
        }
    }
}
