using ChunkyImageLib.DataHolders;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Models.Enums;

namespace PixiEditor.Models.Containers;

internal interface ITransformHandler : IHandler
{
    public void KeyModifiersInlet(bool argsIsShiftDown, bool argsIsCtrlDown, bool argsIsAltDown);
    public void ShowTransform(DocumentTransformMode transformMode, bool b, ShapeCorners shapeCorners, bool b1);
    public void HideTransform();
    public void Undo();
    public void Redo();
    public void Nudge(VecI distance);
}
