using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Svg.Skia;
using LiveMarkdown.Avalonia;
using PixiDocks.Core.Docking;
using PixiEditor.Helpers.Converters;
using PixiEditor.UI.Common.Localization;
using Svg;
using SvgImage = Avalonia.Svg.Skia.SvgImage;

namespace PixiEditor.ViewModels;

public class ChangelogDockViewModel : ViewModelBase, IDockableContent
{
    public string Id { get; } = Guid.NewGuid().ToString();
    public string Title { get; }
    public bool CanFloat { get; }  = true;
    public bool CanClose { get; }   = true;

    public TabCustomizationSettings TabCustomizationSettings { get; } =
        new TabCustomizationSettings() { ShowCloseButton = true,
            Icon = ImagePathToBitmapConverter.LoadImage("/Images/PixiEditorLogoGrayscale.svg") };

    public ObservableStringBuilder Changelog { get; } = new ObservableStringBuilder();

    public ChangelogDockViewModel(string version, string changelog)
    {
        Title = new LocalizedString("CHANGELOG_TITLE", version);
        Changelog.Append(changelog);
    }
}
