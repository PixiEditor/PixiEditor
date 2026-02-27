using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.VisualTree;
using PixiEditor.Helpers.Converters;
using PixiEditor.UI.Common.Localization;

namespace PixiEditor.Helpers.UI;

public class AdvisorFlyout : Flyout
{
    public static readonly StyledProperty<LocalizedString> AdviceProperty = AvaloniaProperty.Register<AdvisorFlyout, LocalizedString>(
        nameof(Advice));

    public LocalizedString Advice
    {
        get => GetValue(AdviceProperty);
        set => SetValue(AdviceProperty, value);
    }


    public AdvisorFlyout(Control control)
    {
        Binding adviceBinding = new Binding(nameof(Advice))
        {
            Source = this
        };

        TextBlock textBlock = new TextBlock()
        {
            Margin = new Thickness(10),
            MaxWidth = 300,
            TextWrapping = TextWrapping.Wrap
        };

        TextBlock mushySays = new TextBlock()
        {
            Text = "Mushy says",
            FontStyle = FontStyle.Italic,
            Foreground =  ResourceLoader.GetResource<IBrush>("ThemeForegroundLowBrush")
        };

        textBlock.Bind(Translator.LocalizedStringProperty, adviceBinding);
        Image image = new Image
        {
            Source = ImagePathToBitmapConverter.TryLoadBitmapFromRelativePath("/Images/Mushy.png"),
            Width = 64, Height = 64,
            Margin = new Thickness(0, 0, 5, 0)
        };

        Grid.SetRow(mushySays, 0);
        Grid.SetColumn(mushySays, 1);

        Grid.SetRow(textBlock, 1);
        Grid.SetColumn(textBlock, 1);

        Grid.SetRow(image, 0);
        Grid.SetRowSpan(image, 2);

        Content = new Grid()
        {
            ColumnDefinitions = new ColumnDefinitions("Auto,*"),
            RowDefinitions = new RowDefinitions("Auto, Auto"),
            Children =
            {
                mushySays,
                image,
                textBlock
            }
        };
        Placement = PlacementMode.Left;

        var root = control.GetVisualRoot();
        if (root is not TopLevel topLevel)
            return;

        OverlayInputPassThroughElement = topLevel;
    }
}
