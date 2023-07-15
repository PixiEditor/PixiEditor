﻿using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace PixiEditor.Avalonia.Views.Main;

public partial class MainTitleBar : UserControl
{
    public MainTitleBar()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
