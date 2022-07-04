using System.Collections.Generic;
using System.Linq;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Enums;
using PixiEditor.Models.Position;
using PixiEditor.Models.Undo;

namespace PixiEditor.Helpers;

public static class SelectionHelpers
{
    public static void AddSelectionUndoStep(Document document, IEnumerable<Coordinates> oldPoints, SelectionType mode)
    {
        if (mode == SelectionType.New && document.ActiveSelection.SelectedPoints.Count != 0)
        {
            if (oldPoints.Any())
            {
                // Add empty selection as the old one get's fully deleted first
                document.UndoManager.AddUndoChange(
                    new Change(
                        SetSelectionProcess, new object[] { document, new List<Coordinates>(oldPoints) },
                        SetSelectionProcess, new object[] { document, new List<Coordinates>() }));
            }

            document.UndoManager.AddUndoChange(
                new Change(
                    SetSelectionProcess, new object[] { document, new List<Coordinates>() },
                    SetSelectionProcess, new object[] { document, new List<Coordinates>(document.ActiveSelection.SelectedPoints) }));
        }
        else
        {
            document.UndoManager.AddUndoChange(
                new Change(
                    SetSelectionProcess, new object[] { document, oldPoints is null ? new List<Coordinates>() : new List<Coordinates>(oldPoints) },
                    SetSelectionProcess, new object[] { document, new List<Coordinates>(document.ActiveSelection.SelectedPoints) }));
        }
    }

    private static void SetSelectionProcess(object[] arguments)
    {
        Document document = (Document)arguments[0];

        document.ActiveSelection.SetSelection((IEnumerable<Coordinates>)arguments[1], SelectionType.New);
    }
}