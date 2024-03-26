using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using PixiEditor.AvaloniaUI.Models.Commands;
using PixiEditor.AvaloniaUI.Models.Commands.Commands;
using PixiEditor.AvaloniaUI.ViewModels;
using PixiEditor.AvaloniaUI.ViewModels.Document;

namespace PixiEditor.AvaloniaUI.Views.Layers;

#nullable enable
internal partial class ReferenceLayer : UserControl
{
    private Command command;

    public static readonly StyledProperty<DocumentViewModel> DocumentProperty = AvaloniaProperty.Register<ReferenceLayer, DocumentViewModel>(
        nameof(Document));

    public DocumentViewModel Document
    {
        get => GetValue(DocumentProperty);
        set => SetValue(DocumentProperty, value);
    }

    public ReferenceLayer()
    {
        command = CommandController.Current.Commands["PixiEditor.Clipboard.PasteReferenceLayer"];
        InitializeComponent();
    }

    private void ReferenceLayer_DragEnter(object sender, DragEventArgs e)
    {
        if (!command.Methods.CanExecute(e.Data))
        {
            return;
        }

        ViewModelMain.Current.ActionDisplays[nameof(ReferenceLayer_Drop)] = "IMPORT_AS_REFERENCE_LAYER";
        e.Handled = true;
    }

    private void ReferenceLayer_DragLeave(object sender, DragEventArgs e)
    {
        ViewModelMain.Current.ActionDisplays[nameof(ReferenceLayer_Drop)] = null;
    }

    private void ReferenceLayer_Drop(object sender, DragEventArgs e)
    {
        if (!command.Methods.CanExecute(e.Data))
        {
            return;
        }

        command.Methods.Execute(e.Data);
        e.Handled = true;
    }
}
