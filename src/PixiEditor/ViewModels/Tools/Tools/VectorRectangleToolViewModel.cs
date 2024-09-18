﻿using Avalonia.Input;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.Models.Handlers;
using PixiEditor.Models.Handlers.Tools;
using PixiEditor.Numerics;
using PixiEditor.UI.Common.Fonts;

namespace PixiEditor.ViewModels.Tools.Tools;

[Command.Tool(Key = Key.R)]
internal class VectorRectangleToolViewModel : ShapeTool, IVectorRectangleToolHandler
{
    private string defaultActionDisplay = "RECTANGLE_TOOL_ACTION_DISPLAY_DEFAULT";
    public override string ToolNameLocalizationKey => "RECTANGLE_TOOL";

    public VectorRectangleToolViewModel()
    {
        ActionDisplay = defaultActionDisplay;
    }

    public override Type[]? SupportedLayerTypes { get; } = [];
    public override LocalizedString Tooltip => new LocalizedString("RECTANGLE_TOOL_TOOLTIP", Shortcut);
    public bool DrawSquare { get; private set; }

    public override string Icon => PixiPerfectIcons.Square;

    public override Type LayerTypeToCreateOnEmptyUse { get; } = typeof(VectorLayerNode);

    public override void ModifierKeyChanged(bool ctrlIsDown, bool shiftIsDown, bool altIsDown)
    {
        if (shiftIsDown)
        {
            DrawSquare = true;
            ActionDisplay = "RECTANGLE_TOOL_ACTION_DISPLAY_SHIFT";
        }
        else
        {
            DrawSquare = false;
            ActionDisplay = defaultActionDisplay;
        }
    }

    public override void UseTool(VecD pos)
    {
        ViewModelMain.Current?.DocumentManagerSubViewModel.ActiveDocument?.Tools.UseVectorRectangleTool();
    }
}