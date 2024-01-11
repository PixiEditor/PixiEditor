﻿using System.Globalization;
using PixiEditor.AvaloniaUI.ViewModels.Tools.Tools;

namespace PixiEditor.AvaloniaUI.Helpers.Converters;

internal class IsSelectionToolConverter : SingleInstanceConverter<IsSelectionToolConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        value is SelectToolViewModel or LassoToolViewModel or MagicWandToolViewModel;
}