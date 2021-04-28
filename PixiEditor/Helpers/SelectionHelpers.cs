using System.Collections.Generic;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Enums;
using PixiEditor.Models.Position;
using PixiEditor.Models.Undo;

namespace PixiEditor.Helpers
{
    public static class SelectionHelpers
    {
        public static void AddSelectionUndoStep(Document document, IEnumerable<Coordinates> oldPoints, SelectionType mode)
        {
#pragma warning disable SA1117 // Parameters should be on same line or separate lines. Justification: Making it readable
            if (mode == SelectionType.New && document.ActiveSelection.SelectedPoints.Count != 0)
            {
                // Add empty selection as the old one get's fully deleted first
                document.UndoManager.AddUndoChange(
                    new Change(
                        SetSelectionProcess, new object[] { document, new List<Coordinates>(oldPoints) },
                        SetSelectionProcess, new object[] { document, new List<Coordinates>() }));
                document.UndoManager.AddUndoChange(
                    new Change(
                        SetSelectionProcess, new object[] { document, new List<Coordinates>() },
                        SetSelectionProcess, new object[] { document, new List<Coordinates>(document.ActiveSelection.SelectedPoints) }));
            }
            else
            {
                document.UndoManager.AddUndoChange(
                    new Change(
                        SetSelectionProcess, new object[] { document, new List<Coordinates>(oldPoints) },
                        SetSelectionProcess, new object[] { document, new List<Coordinates>(document.ActiveSelection.SelectedPoints) }));
#pragma warning restore SA1117 // Parameters should be on same line or separate lines
            }
        }

        private static void SetSelectionProcess(object[] arguments)
        {
            Document document = (Document)arguments[0];

            document.ActiveSelection.SetSelection((IEnumerable<Coordinates>)arguments[1], SelectionType.New);
        }
    }
}