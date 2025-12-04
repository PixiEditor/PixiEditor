using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;

namespace PixiEditor.Views.Nodes.Properties;

public partial class FontFamilyNamePropertyView : NodePropertyView
{
    public FontFamilyNamePropertyView()
    {
        InitializeComponent();
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
    }
}
