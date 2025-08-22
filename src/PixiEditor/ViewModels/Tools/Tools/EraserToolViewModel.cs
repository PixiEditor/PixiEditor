﻿using Avalonia.Input;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Vector;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.Models.Handlers;
using PixiEditor.Models.Handlers.Tools;
using Drawie.Numerics;
using PixiEditor.Models.Handlers.Toolbars;
using PixiEditor.UI.Common.Fonts;
using PixiEditor.UI.Common.Localization;
using PixiEditor.ViewModels.Tools.ToolSettings.Toolbars;
using PixiEditor.Views.Overlays.BrushShapeOverlay;

namespace PixiEditor.ViewModels.Tools.Tools;

[Command.Tool(Key = Key.E)]
internal class EraserToolViewModel : ToolViewModel, IEraserToolHandler
{
    public EraserToolViewModel()
    {
        ActionDisplay = "ERASER_TOOL_ACTION_DISPLAY";
        Toolbar = ToolbarFactory.Create<EraserToolViewModel, PenToolbar>(this);
    }

    [Settings.Inherited] public double ToolSize => GetValue<double>();

    public override bool IsErasable => true;

    public override string ToolNameLocalizationKey => "ERASER_TOOL";
    //TODO: PaintShape == PaintBrushShape.Square ? BrushShape.Square : BrushShapeSetting;
    public override Type[]? SupportedLayerTypes { get; } = { typeof(IRasterLayerHandler) };

    public override string DefaultIcon => PixiPerfectIcons.Eraser;

    public override LocalizedString Tooltip => new LocalizedString("ERASER_TOOL_TOOLTIP", Shortcut);

    public override Type LayerTypeToCreateOnEmptyUse { get; } = typeof(ImageLayerNode);

    [Settings.Enum("BRUSH_SHAPE_SETTING", BrushShape.CirclePixelated, ExposedByDefault = false,
        Notify = nameof(BrushShapeChanged))]
    public BrushShape BrushShapeSetting => GetValue<BrushShape>();

    [Settings.Inherited(Notify = nameof(PenShapeChanged))]
    public PaintBrushShape PaintShape
    {
        get => GetValue<PaintBrushShape>();
        set
        {
            SetValue(value);
            OnPropertyChanged(nameof(FinalBrushShape));
        }
    }

    public override void UseTool(VecD pos)
    {
        ViewModelMain.Current?.DocumentManagerSubViewModel.ActiveDocument?.Tools.UseEraserTool();
    }

    private void BrushShapeChanged()
    {
        OnPropertyChanged(nameof(FinalBrushShape));
    }

    private void PenShapeChanged()
    {
        OnPropertyChanged(nameof(FinalBrushShape));
    }
}
