﻿using Avalonia.Input;
using PixiEditor.Extensions.CommonApi.UserPreferences.Settings.PixiEditor;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.Models.Preferences;
using PixiEditor.UI.Common.Fonts;
using PixiEditor.ViewModels.UserPreferences.Settings;

namespace PixiEditor.ViewModels.SubViewModels;
#nullable enable
internal class ViewOptionsViewModel : SubViewModel<ViewModelMain>
{
    private bool gridLinesEnabled;

    public bool GridLinesEnabled
    {
        get => gridLinesEnabled;
        set => SetProperty(ref gridLinesEnabled, value);
    }
    
    private bool snappingEnabled = true;
    public bool SnappingEnabled
    {
        get => snappingEnabled;
        set
        {
            SetProperty(ref snappingEnabled, value);
            Owner.DocumentManagerSubViewModel.ActiveDocument.SnappingViewModel.SnappingController.SnappingEnabled = value;
        }
    }
    
    private bool highResRender = true;
    public bool HighResRender
    {
        get => highResRender;
        set
        {
            SetProperty(ref highResRender, value);
            Owner.DocumentManagerSubViewModel.ActiveDocument.SceneRenderer.HighResRendering = value;
        }
    }

    private int maxBilinearSampleSize = 4096;
    public int MaxBilinearSampleSize
    {
        get => maxBilinearSampleSize;
        set
        {
            SetProperty(ref maxBilinearSampleSize, value);
        }
    }

    public ViewOptionsViewModel(ViewModelMain owner)
        : base(owner)
    {
        MaxBilinearSampleSize = PixiEditorSettings.Performance.MaxBilinearSampleSize.Value;

        PixiEditorSettings.Performance.MaxBilinearSampleSize.ValueChanged += (s, e) =>
        {
            MaxBilinearSampleSize = PixiEditorSettings.Performance.MaxBilinearSampleSize.Value;
        };
    }

    [Command.Basic("PixiEditor.View.ToggleGrid", "TOGGLE_GRIDLINES", "TOGGLE_GRIDLINES", Key = Key.OemTilde,
        Modifiers = KeyModifiers.Control,
        Icon = PixiPerfectIcons.Grid)]
    public void ToggleGridLines()
    {
        GridLinesEnabled = !GridLinesEnabled;
    }

    [Command.Basic("PixiEditor.View.ZoomIn", 1, "ZOOM_IN", "ZOOM_IN", CanExecute = "PixiEditor.HasDocument",
        Key = Key.OemPlus,
        Icon = PixiPerfectIcons.ZoomIn, AnalyticsTrack = true)]
    [Command.Basic("PixiEditor.View.Zoomout", -1, "ZOOM_OUT", "ZOOM_OUT", CanExecute = "PixiEditor.HasDocument",
        Key = Key.OemMinus,
        Icon = PixiPerfectIcons.ZoomOut, AnalyticsTrack = true)]
    public void ZoomViewport(double zoom)
    {
        ViewportWindowViewModel? viewport = Owner.WindowSubViewModel.ActiveWindow as ViewportWindowViewModel;
        if (viewport is null)
            return;
        viewport.ZoomViewportTrigger.Execute(this, zoom);
    }

    [Command.Basic("PixiEditor.ToggleSnapping", "TOGGLE_SNAPPING", "TOGGLE_SNAPPING",
        Icon = PixiPerfectIcons.Snapping)]
    public void ToggleSnapping()
    {
        SnappingEnabled = !SnappingEnabled;
    }
}
