using System.ComponentModel;
using PixiEditor.ChangeableDocument.Changeables.Brushes;
using PixiEditor.Helpers;
using PixiEditor.Helpers.Decorators;
using PixiEditor.Models.BrushEngine;
using PixiEditor.Views.Overlays.BrushShapeOverlay;

namespace PixiEditor.Models.Handlers.Toolbars;

internal interface IBrushToolbar : IToolbar, IToolSizeToolbar
{
    public bool AntiAliasing { get; set; }
    public bool ForcePressure { get; set; }
    public Brush Brush { get; set; }
    public BrushData CreateBrushData();
    public BrushData LastBrushData { get; }
    public double Stabilization { get; set; }
    public StabilizationMode StabilizationMode { get; set; }
}

public enum StabilizationMode
{
    [Description("NONE_STABILIZATION")]
    [IconName(PixiPerfectIcons.Minus)]
    None,
    [Description("TIME_BASED_STABILIZATION")]
    [IconName(PixiPerfectIcons.TimeStabilizer)]
    TimeBased,
    [Description("DISTANCE_BASED_STABILIZATION")]
    [IconName(PixiPerfectIcons.DistanceStabilizer)]
    CircleRope
}
