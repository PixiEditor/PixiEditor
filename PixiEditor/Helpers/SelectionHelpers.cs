using System.Collections.Generic;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Enums;
using PixiEditor.Models.Position;
using PixiEditor.Models.Undo;

namespace PixiEditor.Helpers
{
    public class SelectionHelpers
    {
        public static void UndoSelect(object[] arguments)
        {
            Document document = (Document)arguments[0];

            document.ActiveSelection.SetSelection((IEnumerable<Coordinates>)arguments[1], SelectionType.New);
        }

        public static void RedoSelect(object[] arguments)
        {
            Document document = (Document)arguments[0];

            document.ActiveSelection.SetSelection((IEnumerable<Coordinates>)arguments[1], SelectionType.New);
        }

        public static void AddSelectionUndoStep(Document document, IEnumerable<Coordinates> oldPoints, SelectionType mode)
        {
            if (mode == SelectionType.New && document.ActiveSelection.SelectedPoints.Count != 0)
            {
                // Add empty selection as the old one get's fully deleted first
                document.UndoManager.AddUndoChange(
                    new Change(UndoSelect, new object[] { document, new List<Coordinates>(oldPoints) }, RedoSelect, new object[] { document, new List<Coordinates>() }));
                document.UndoManager.AddUndoChange(
                    new Change(UndoSelect, new object[] { document, new List<Coordinates>() }, RedoSelect, new object[] { document, new List<Coordinates>(document.ActiveSelection.SelectedPoints) }));
            }
            else
            {
                document.UndoManager.AddUndoChange(
                    new Change(UndoSelect, new object[] { document, new List<Coordinates>(oldPoints) }, RedoSelect, new object[] { document, new List<Coordinates>(document.ActiveSelection.SelectedPoints) }));
            }
        }
    }
}