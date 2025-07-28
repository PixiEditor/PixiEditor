using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.Input;
using PixiEditor.Helpers;
using PixiEditor.Models;
using PixiEditor.Models.Dialogs;

namespace PixiEditor.Views.Input;

internal partial class SizePicker : UserControl
{
    public static readonly StyledProperty<bool> EditingEnabledProperty =
        AvaloniaProperty.Register<SizePicker, bool>(nameof(EditingEnabled), true);

    public static readonly StyledProperty<bool> PreserveAspectRatioProperty =
        AvaloniaProperty.Register<SizePicker, bool>(nameof(PreserveAspectRatio), true);

    public static readonly StyledProperty<int> ChosenWidthProperty =
        AvaloniaProperty.Register<SizePicker, int>(nameof(ChosenWidth), 1);

    public static readonly StyledProperty<int> ChosenHeightProperty =
        AvaloniaProperty.Register<SizePicker, int>(nameof(ChosenHeight), 1);

    public static readonly StyledProperty<float> ChosenPercentageSizeProperty =
        AvaloniaProperty.Register<SizePicker, float>(nameof(ChosenPercentageSize), 100f);

    public static readonly StyledProperty<SizeUnit> SelectedUnitProperty =
        AvaloniaProperty.Register<SizePicker, SizeUnit>(nameof(SelectedUnit), SizeUnit.Pixel);

    public static readonly StyledProperty<bool> IsSizeUnitSelectionVisibleProperty =
        AvaloniaProperty.Register<SizePicker, bool>(nameof(IsSizeUnitSelectionVisible), false);

    private System.Drawing.Size? initSize = null;

    public bool EditingEnabled
    {
        get => GetValue(EditingEnabledProperty);
        set => SetValue(EditingEnabledProperty, value);
    }

    public int ChosenWidth
    {
        get => GetValue(ChosenWidthProperty);
        set => SetValue(ChosenWidthProperty, value);
    }

    public int ChosenHeight
    {
        get => GetValue(ChosenHeightProperty);
        set => SetValue(ChosenHeightProperty, value);
    }

    public float ChosenPercentageSize
    {
        get => GetValue(ChosenPercentageSizeProperty);
        set => SetValue(ChosenPercentageSizeProperty, value);
    }

    public SizeUnit SelectedUnit
    {
        get => GetValue(SelectedUnitProperty);
        set => SetValue(SelectedUnitProperty, value);
    }

    public bool IsSizeUnitSelectionVisible
    {
        get => GetValue(IsSizeUnitSelectionVisibleProperty);
        set => SetValue(IsSizeUnitSelectionVisibleProperty, value);
    }

    public bool PreserveAspectRatio
    {
        get => GetValue(PreserveAspectRatioProperty);
        set => SetValue(PreserveAspectRatioProperty, value);
    }

    public RelayCommand LoadedCommand { get; private set; }
    public RelayCommand WidthLostFocusCommand { get; private set; }
    public RelayCommand HeightLostFocusCommand { get; private set; }
    public RelayCommand PercentageLostFocusCommand { get; private set; }

    public SizePicker()
    {
        LoadedCommand = new(AfterLoaded);
        WidthLostFocusCommand = new(WidthLostFocus);
        HeightLostFocusCommand = new(HeightLostFocus);
        PercentageLostFocusCommand = new(PercentageLostFocus);

        InitializeComponent();

        WidthPicker.OnScrollAction = () => OnSizeUpdate(true);
        HeightPicker.OnScrollAction = () => OnSizeUpdate(false);
        PercentageSizePicker.OnScrollAction = PercentageLostFocus;
    }

    public void FocusWidthPicker()
    {
        WidthPicker.FocusAndSelect();
    }

    private void AfterLoaded()
    {
        initSize = new System.Drawing.Size(ChosenWidth, ChosenHeight);
        EnableSizeEditors();
    }

    private void WidthLostFocus() => OnSizeUpdate(true);
    private void HeightLostFocus() => OnSizeUpdate(false);

    public void PercentageLostFocus()
    {
        if (!initSize.HasValue)
            return;

        float targetPercentage = GetTargetPercentage(initSize.Value, ChosenPercentageSize);
        var newSize = SizeCalculator.CalcAbsoluteFromPercentage(targetPercentage, initSize.Value);

        //this shouldn't ever be necessary but just in case
        newSize.Width = Math.Clamp(newSize.Width, 1, Constants.MaxCanvasSize);
        newSize.Height = Math.Clamp(newSize.Height, 1, Constants.MaxCanvasSize);

        ChosenPercentageSize = targetPercentage;
        ChosenWidth = newSize.Width;
        ChosenHeight = newSize.Height;
    }

    private static float GetTargetPercentage(System.Drawing.Size initSize, float desiredPercentage)
    {
        var potentialSize = SizeCalculator.CalcAbsoluteFromPercentage(desiredPercentage, initSize);
        // all good
        if (potentialSize.Width > 0 && potentialSize.Height > 0 && potentialSize.Width <= Constants.MaxCanvasSize &&
            potentialSize.Height <= Constants.MaxCanvasSize)
            return desiredPercentage;

        // canvas too small
        if (potentialSize.Width <= 0 || potentialSize.Height <= 0)
        {
            if (potentialSize.Width < potentialSize.Height)
                return 100f / initSize.Width;
            else
                return 100f / initSize.Height;
        }

        // canvas too big
        if (potentialSize.Width > potentialSize.Height)
            return Constants.MaxCanvasSize * 100f / initSize.Width;
        else
            return Constants.MaxCanvasSize * 100f / initSize.Height;
    }

    private void OnSizeUpdate(bool widthUpdated)
    {
        if (!initSize.HasValue || !PreserveAspectRatio)
            return;

        if (widthUpdated)
        {
            ChosenHeight = Math.Clamp(ChosenWidth * initSize.Value.Height / initSize.Value.Width, (int)1,
                (int)HeightPicker.MaxSize);
        }
        else
        {
            ChosenWidth = Math.Clamp(ChosenHeight * initSize.Value.Width / initSize.Value.Height, (int)1,
                (int)WidthPicker.MaxSize);
        }
    }

    private void PercentageRb_Checked(object sender, RoutedEventArgs e)
    {
        EnableSizeEditors();
    }

    private void AbsoluteRb_Checked(object sender, RoutedEventArgs e)
    {
        EnableSizeEditors();
    }

    private void EnableSizeEditors()
    {
        if (PercentageSizePicker != null)
            PercentageSizePicker.IsEnabled = EditingEnabled && PercentageRb.IsChecked.Value;
        if (WidthPicker != null)
            WidthPicker.IsEnabled = EditingEnabled && !PercentageRb.IsChecked.Value;
        if (HeightPicker != null)
            HeightPicker.IsEnabled = EditingEnabled && !PercentageRb.IsChecked.Value;
    }
}
