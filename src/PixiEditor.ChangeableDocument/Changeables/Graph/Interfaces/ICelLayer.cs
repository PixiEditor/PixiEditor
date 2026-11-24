using System;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Animations;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;

internal interface ICelLayer
{
    public string CelDataKey { get; }
    public object? CreateEmptyCelData(VecI documentSize);
    public object? CloneCelDataAtFrame(int frame);
    public object? CloneCelData(KeyFrameData cel);

    public bool FallbackAnimationToLayerImage { get; set; }
}
