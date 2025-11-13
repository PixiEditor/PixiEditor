using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using PixiEditor.ViewModels.Tools.ToolSettings.Settings;

namespace PixiEditor.Views.Tools.ToolSettings.Settings;

public partial class EnumSettingView : UserControl
{
    public EnumSettingView()
    {
        InitializeComponent();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        iconButtonsPicker.ContainerPrepared += IconButtonsPickerOnContainerPrepared;
    }

    override protected void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        iconButtonsPicker.ContainerPrepared -= IconButtonsPickerOnContainerPrepared;
    }

    private void IconButtonsPickerOnContainerPrepared(object? sender, ContainerPreparedEventArgs e)
    {
        if (e.Index == 0)
        {
            e.Container.Classes.Add("first");
        }
    }
}

