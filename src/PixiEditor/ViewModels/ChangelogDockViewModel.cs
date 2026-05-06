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
    public string Title => new LocalizedString("CHANGELOG_TITLE", Version);
    public bool CanFloat { get; }  = true;
    public bool CanClose { get; }   = true;

    public TabCustomizationSettings TabCustomizationSettings { get; } =
        new TabCustomizationSettings() { ShowCloseButton = true,
            Icon = ImagePathToBitmapConverter.LoadImage("/Images/PixiEditorLogoGrayscale.svg") };

    public string Version { get; set; }

    public ObservableStringBuilder Changelog { get; } = new ObservableStringBuilder();

    public ChangelogDockViewModel(string version, string changelog)
    {
        Version = version;
        Changelog.Append(changelog);
    }
}
