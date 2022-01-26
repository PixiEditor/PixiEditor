using PixiEditor.Helpers;
using PixiEditor.Models;
using PixiEditor.Models.Enums;
using PixiEditor.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;

namespace PixiEditor.Views
{
    public partial class SizePicker : UserControl
    {
        public static readonly DependencyProperty EditingEnabledProperty =
            DependencyProperty.Register(nameof(EditingEnabled), typeof(bool), typeof(SizePicker), new PropertyMetadata(true));

        public static readonly DependencyProperty PreserveAspectRatioProperty =
            DependencyProperty.Register(nameof(PreserveAspectRatio), typeof(bool), typeof(SizePicker), new PropertyMetadata(true));

        public static readonly DependencyProperty ChosenWidthProperty =
            DependencyProperty.Register(nameof(ChosenWidth), typeof(int), typeof(SizePicker), new PropertyMetadata(1));

        public static readonly DependencyProperty ChosenHeightProperty =
            DependencyProperty.Register(nameof(ChosenHeight), typeof(int), typeof(SizePicker), new PropertyMetadata(1));

        public static readonly DependencyProperty NextControlProperty =
            DependencyProperty.Register(nameof(NextControl), typeof(FrameworkElement), typeof(SizePicker));

        public static readonly DependencyProperty ChosenPercentageSizeProperty =
            DependencyProperty.Register(nameof(ChosenPercentageSize), typeof(int), typeof(SizePicker), new PropertyMetadata(1, InputSizeChanged));

        public static readonly DependencyProperty SelectedUnitProperty =
            DependencyProperty.Register(nameof(SelectedUnit), typeof(SizeUnit), typeof(SizePicker), new PropertyMetadata(SizeUnit.Pixel));

        public static readonly DependencyProperty SizeUnitSelectionVisibilityProperty =
            DependencyProperty.Register(nameof(SizeUnitSelectionVisibility), typeof(Visibility), typeof(SizePicker), new PropertyMetadata(Visibility.Collapsed));

        System.Drawing.Size? initSize = null;
               
        private static void InputSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sizePicker = d as SizePicker;
            if (!sizePicker.initSize.HasValue)
                return;

            var newValue = (int)e.NewValue;
            var newSize = SizeCalculator.CalcAbsoluteFromPercentage(newValue, sizePicker.initSize.Value);
            if (newSize.Width > Constants.MaxWidth || newSize.Width > Constants.MaxHeight)
            {
                newSize = new System.Drawing.Size(Constants.MaxWidth, Constants.MaxHeight);
                d.SetValue(ChosenPercentageSizeProperty, SizeCalculator.CalcPercentageFromAbsolute(sizePicker.initSize.Value.Width, newSize.Width));
            }
            
            d.SetValue(ChosenWidthProperty, newSize.Width);
            d.SetValue(ChosenHeightProperty, newSize.Height);
        }

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

        public int ChosenPercentageSize
        {
            get => (int)GetValue(ChosenPercentageSizeProperty);
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

        public FrameworkElement NextControl
        {
            get => (FrameworkElement)GetValue(NextControlProperty);
            set => SetValue(NextControlProperty, value);
        }

        public bool PreserveAspectRatio
        {
            get => (bool)GetValue(PreserveAspectRatioProperty);
            set => SetValue(PreserveAspectRatioProperty, value);
        }

        public RelayCommand LoadedCommand { get; private set; }
        public RelayCommand WidthLostFocusCommand { get; private set; }
        public RelayCommand HeightLostFocusCommand { get; private set; }
                        
        public SizePicker()
        {
            LoadedCommand = new(AfterLoaded);
            WidthLostFocusCommand = new(WidthLostFocus);
            HeightLostFocusCommand = new(HeightLostFocus);
            InitializeComponent();
        }

        public void FocusWidthPicker()
        {
            PercentageSizePicker.FocusAndSelect();
        }

        private void AfterLoaded(object parameter)
        {
            initSize = new System.Drawing.Size(ChosenWidth, ChosenHeight);
            EnableSizeEditors();
        }

        private void WidthLostFocus(object param) => OnSizeUpdate(true);
        private void HeightLostFocus(object param) => OnSizeUpdate(false);

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
            if(PercentageSizePicker != null)
                PercentageSizePicker.IsEnabled = EditingEnabled && PercentageRb.IsChecked.Value;
            if (WidthPicker != null)
                WidthPicker.IsEnabled = EditingEnabled && !PercentageRb.IsChecked.Value;
            if(HeightPicker != null)
                HeightPicker.IsEnabled = EditingEnabled && !PercentageRb.IsChecked.Value;
        }
    }
}
