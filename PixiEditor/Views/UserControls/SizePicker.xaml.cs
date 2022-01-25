using PixiEditor.Helpers;
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

        public bool PreserveAspectRatio
        {
            get => (bool)GetValue(PreserveAspectRatioProperty);
            set => SetValue(PreserveAspectRatioProperty, value);
        }

        public RelayCommand LoadedCommand { get; private set; }
        public RelayCommand WidthLostFocusCommand { get; private set; }
        public RelayCommand HeightLostFocusCommand { get; private set; }

        private bool initialValuesLoaded = false;
        private int initW;
        private int initH;
        public SizePicker()
        {
            LoadedCommand = new(AfterLoaded);
            WidthLostFocusCommand = new(WidthLostFocus);
            HeightLostFocusCommand = new(HeightLostFocus);
            InitializeComponent();
        }

        public void FocusWidthPicker()
        {
            WidthPicker.FocusAndSelect();
        }

        private void AfterLoaded(object parameter)
        {
            initW = ChosenWidth;
            initH = ChosenHeight;
            initialValuesLoaded = true;
        }

        private void WidthLostFocus(object param) => OnSizeUpdate(true);
        private void HeightLostFocus(object param) => OnSizeUpdate(false);

        private void OnSizeUpdate(bool widthUpdated)
        {
            if (!initialValuesLoaded || !PreserveAspectRatio)
                return;

            if (widthUpdated)
            {
                ChosenHeight = Math.Clamp(ChosenWidth * initH / initW, 1, HeightPicker.MaxSize);
            }
            else
            {
                ChosenWidth = Math.Clamp(ChosenHeight * initW / initH, 1, WidthPicker.MaxSize);
            }
        }
    }
}
