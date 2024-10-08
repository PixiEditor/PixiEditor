﻿using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Numerics;

namespace PixiEditor.Models.Handlers;

internal interface ILineOverlayHandler
{
    public void Hide();
    public bool Nudge(VecD distance);
    public bool Undo();
    public bool Redo();
    public void Show(VecD startPos, VecD endPos, bool showApplyButton);
    public bool HasUndo { get; }
    public bool HasRedo { get; }
}
