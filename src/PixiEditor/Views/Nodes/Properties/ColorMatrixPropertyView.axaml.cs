using Avalonia.Controls.Primitives;

namespace PixiEditor.Views.Nodes.Properties;

public partial class ColorMatrixPropertyView : NodePropertyView
{
    public ColorMatrixPropertyView()
    {
        InitializeComponent();
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        HideSocket(true, false);
    }
}

