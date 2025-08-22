﻿using Avalonia.Input;
using PixiEditor.Views.Visuals;
using PixiEditor.Extensions.UI.Overlays;

namespace PixiEditor.Views.Overlays.Pointers;

internal class MouseOverlayPointer : IOverlayPointer
{
    public PointerType Type { get; }
    IPointer pointer;
    private Action<Overlay?, IPointer> captureAction;

    public MouseOverlayPointer(IPointer pointer, Action<Overlay?, IPointer> captureAction)
    {
        this.pointer = pointer;
        this.captureAction = captureAction;
        Type = pointer.Type;
    }


    public void Capture(IOverlay? overlay)
    {
        if (overlay is not Overlay visualOverlay)
        {
            return;
        }

        captureAction(visualOverlay, pointer);
    }
}
