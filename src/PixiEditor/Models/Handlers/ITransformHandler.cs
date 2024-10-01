using ChunkyImageLib.DataHolders;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Models.DocumentModels;
using PixiEditor.Numerics;

namespace PixiEditor.Models.Handlers;

internal interface ITransformHandler : IHandler
{
    public void KeyModifiersInlet(bool argsIsShiftDown, bool argsIsCtrlDown, bool argsIsAltDown);
    public void ShowTransform(DocumentTransformMode transformMode, bool coverWholeScreen, ShapeCorners shapeCorners, bool showApplyButton);
    public void HideTransform();
    public bool Undo();
    public bool Redo();
    public bool Nudge(VecD distance);
    public bool HasUndo { get; }
    public bool HasRedo { get; }
}
