using System.Windows;
using System.Windows.Controls;
using PixiEditor.Helpers;
using PixiEditor.Models;
using PixiEditor.Models.Enums;

namespace PixiEditor.Views.UserControls;

internal partial class SizePicker : UserControl
{
    public static readonly DependencyProperty EditingEnabledProperty =
        DependencyProperty.Register(nameof(EditingEnabled), typeof(bool), typeof(SizePicker), new PropertyMetadata(true));

    public static readonly DependencyProperty PreserveAspectRatioProperty =
        DependencyProperty.Register(nameof(PreserveAspectRatio), typeof(bool), typeof(SizePicker), new PropertyMetadata(true));

    public static readonly DependencyProperty ChosenWidthProperty =
        DependencyProperty.Register(nameof(ChosenWidth), typeof(int), typeof(SizePicker), new PropertyMetadata(1));

    public static readonly DependencyProperty ChosenHeightProperty =
        DependencyProperty.Register(nameof(ChosenHeight), typeof(int), typeof(SizePicker), new PropertyMetadata(1));

    public static readonly DependencyProperty ChosenPercentageSizeProperty =
        DependencyProperty.Register(nameof(ChosenPercentageSize), typeof(float), typeof(SizePicker), new PropertyMetadata(100f));

    public static readonly DependencyProperty SelectedUnitProperty =
        DependencyProperty.Register(nameof(SelectedUnit), typeof(SizeUnit), typeof(SizePicker), new PropertyMetadata(SizeUnit.Pixel));

    public static readonly DependencyProperty SizeUnitSelectionVisibilityProperty =
        DependencyProperty.Register(nameof(SizeUnitSelectionVisibility), typeof(Visibility), typeof(SizePicker), new PropertyMetadata(Visibility.Collapsed));

    System.Drawing.Size? initSize = null;

    public bool EditingEnabled
    {
        get => (bool)GetValue(EditingEnabledProperty);
        set => SetValue(EditingEnabledProperty, value);
    }

    public int ChosenWidth
    {
        get => (int)GetValue(ChosenWidthProperty);
        set => SetValue(ChosenWidthProperty, value);
    }

    public int ChosenHeight
    {
        get => (int)GetValue(ChosenHeightProperty);
        set => SetValue(ChosenHeightProperty, value);
    }

    public float ChosenPercentageSize
    {
        get => (float)GetValue(ChosenPercentageSizeProperty);
        set => SetValue(ChosenPercentageSizeProperty, value);
    }

    public SizeUnit SelectedUnit
    {
        get => (SizeUnit)GetValue(SelectedUnitProperty);
        set => SetValue(SelectedUnitProperty, value);
    }

    public Visibility SizeUnitSelectionVisibility
    {
        get => (Visibility)GetValue(SizeUnitSelectionVisibilityProperty);
        set => SetValue(SizeUnitSelectionVisibilityProperty, value);
    }

    public bool PreserveAspectRatio
    {
        get => (bool)GetValue(PreserveAspectRatioProperty);
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
        PercentageSizePicker.OnScrollAction = () => PercentageLostFocus(null);
    }

    public void FocusWidthPicker()
    {
        WidthPicker.FocusAndSelect();
    }

    private void AfterLoaded(object parameter)
    {
        initSize = new System.Drawing.Size(ChosenWidth, ChosenHeight);
        EnableSizeEditors();
    }

    private void WidthLostFocus(object param) => OnSizeUpdate(true);
    private void HeightLostFocus(object param) => OnSizeUpdate(false);

    private void PercentageLostFocus(object param)
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
        if (potentialSize.Width > 0 && potentialSize.Height > 0 && potentialSize.Width <= Constants.MaxCanvasSize && potentialSize.Height <= Constants.MaxCanvasSize)
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
            ChosenHeight = Math.Clamp(ChosenWidth * initSize.Value.Height / initSize.Value.Width, 1, HeightPicker.MaxSize);
        }
        else
        {
            ChosenWidth = Math.Clamp(ChosenHeight * initSize.Value.Width / initSize.Value.Height, 1, WidthPicker.MaxSize);
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
