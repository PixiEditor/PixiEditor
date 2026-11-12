using Avalonia.Controls.Primitives;

namespace PixiEditor.Views.Nodes.Properties;

public partial class Matrix4x5FPropertyView : NodePropertyView
{
    public Matrix4x5FPropertyView()
    {
        InitializeComponent();
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        HideSocket(true, false);
    }
}

