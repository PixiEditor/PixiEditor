using Avalonia;
using Avalonia.Media;
using Drawie.Backend.Core.Text;
using PixiEditor.Helpers.Extensions;
using Color = Drawie.Backend.Core.ColorsImpl.Color;

namespace PixiEditor.Helpers;

public static class ThemeResources
{
    public static string? ThemeFontFamilyName { get; } = "FiraSans";

    public static Font ThemeFont =>
        Font.FromFamilyName(ThemeFontFamilyName) ?? Font.CreateDefault();

    public static Color ForegroundColor =>
        ResourceLoader.GetResource<SolidColorBrush>("ThemeForegroundBrush", Application.Current.ActualThemeVariant)
            .Color.ToColor();

    public static Color BackgroundColor =>
        ResourceLoader.GetResource<SolidColorBrush>("ThemeBackgroundBrush", Application.Current.ActualThemeVariant)
            .Color.ToColor();

    public static Color BorderMidColor =>
        ResourceLoader.GetResource<SolidColorBrush>("ThemeBorderMidBrush", Application.Current.ActualThemeVariant).Color
            .ToColor();

    public static Color ThemeControlHighlightColor =>
        ResourceLoader.GetResource<Avalonia.Media.Color>("ThemeControlHighlightColor", Application.Current.ActualThemeVariant)
            .ToColor();

    public static Color SelectionFillColor =>
        ResourceLoader.GetResource<Avalonia.Media.Color>("SelectionFillColor", Application.Current.ActualThemeVariant)
            .ToColor();

    public static SolidColorBrush ThemeControlLowBrush =>
        ResourceLoader.GetResource<SolidColorBrush>("ThemeControlLowBrush", Application.Current.ActualThemeVariant);

    public static IBrush AutosaveDotBrush =>
        ResourceLoader.GetResource<IBrush>("AutosaveDotBrush", Application.Current.ActualThemeVariant);

    public static IBrush UnsavedDotBrush =>
        ResourceLoader.GetResource<IBrush>("UnsavedDotBrush", Application.Current.ActualThemeVariant);
}
