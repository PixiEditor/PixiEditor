﻿using Avalonia.Input;
using PixiEditor.Models.Handlers;
using PixiEditor.Models.Handlers.Tools;
using PixiEditor.ViewModels.Tools.ToolSettings.Toolbars;
using PixiEditor.Views.Overlays.BrushShapeOverlay;

namespace PixiEditor.ViewModels.Tools;

internal abstract class ShapeTool : ToolViewModel, IShapeToolHandler
{
    public override BrushShape BrushShape => BrushShape.Hidden;

    public override bool UsesColor => true;

    public override bool IsErasable => true;

    public ShapeTool()
    {
        Cursor = new Cursor(StandardCursorType.Cross);
        Toolbar = new BasicShapeToolbar();
    }

    public override void OnDeselecting()
    {
        ViewModelMain.Current.DocumentManagerSubViewModel.ActiveDocument?.Operations.TryStopToolLinkedExecutor();
    }
}
