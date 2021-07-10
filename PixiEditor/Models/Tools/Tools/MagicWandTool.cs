using PixiEditor.Helpers;
using PixiEditor.Helpers.Extensions;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Enums;
using PixiEditor.Models.ImageManipulation;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using PixiEditor.Models.Tools.ToolSettings;
using PixiEditor.Models.Tools.ToolSettings.Settings;
using PixiEditor.Models.Tools.ToolSettings.Toolbars;
using PixiEditor.ViewModels;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace PixiEditor.Models.Tools.Tools
{
    public class MagicWandTool : ReadonlyTool
    {
        private readonly FloodFill floodFill;
        private SelectionType previousSelectionType;

        public override string ImagePath => $"/Images/Tools/{nameof(FloodFill)}Image.png";

        private static Selection ActiveSelection { get => ViewModelMain.Current.BitmapManager.ActiveDocument.ActiveSelection; }

        private BitmapManager BitmapManager { get; }

        private IEnumerable<Coordinates> oldSelection;

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
                layer = new Layer("_CombinedLayers", BitmapUtils.CombineLayers(document.Width, document.Height, document.Layers, document.LayerStructure));
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

            var selectionTypeSetting = Toolbar.GetEnumSetting<SelectionType>("SelectMode");

            selectionTypeSetting.ValueChanged += MagicWandTool_ValueChanged;

            UpdateActionDisplay(selectionTypeSetting);
        }

        public override void Use(List<Coordinates> pixels)
        {
            return;
        }

        public override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key != Key.LeftShift || e.IsRepeat)
            {
                return;
            }

            EnumSetting<SelectionType> enumSetting = Toolbar.GetEnumSetting<SelectionType>("SelectMode");
            previousSelectionType = enumSetting.Value;
            enumSetting.Value = SelectionType.Add;
        }

        public override void OnKeyUp(KeyEventArgs e)
        {
            if (e.Key != Key.LeftShift)
            {
                return;
            }

            Toolbar.GetEnumSetting<SelectionType>("SelectMode").Value = previousSelectionType;
        }

        private void MagicWandTool_ValueChanged(object sender, SettingValueChangedEventArgs<SelectionType> e)
        {
            UpdateActionDisplay(sender as EnumSetting<SelectionType>);
        }

        private void UpdateActionDisplay(EnumSetting<SelectionType> setting)
        {
            if (setting.Value == SelectionType.Add)
            {
                if (Keyboard.IsKeyDown(Key.LeftShift))
                {
                    ActionDisplay = $"Click to flood the selection. Release shift to revert the selection type to {previousSelectionType}";
                    return;
                }

                ActionDisplay = "Click to flood the selection";
            }
            else
            {
                ActionDisplay = $"Click to flood the selection. Hold shift to set the selection type to {nameof(SelectionType.Add)}";
            }
        }
    }
}
