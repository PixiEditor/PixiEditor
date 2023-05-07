using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using PixiEditor.Models.Commands;
using PixiEditor.Models.Commands.Commands;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.IO;
using PixiEditor.ViewModels.SubViewModels.Document;

namespace PixiEditor.Views.UserControls.Layers;

#nullable enable
internal partial class ReferenceLayer : UserControl
{
    private Command command;
    
    public static readonly DependencyProperty DocumentProperty =
        DependencyProperty.Register(nameof(Document), typeof(DocumentViewModel), typeof(ReferenceLayer), new(null));

    public DocumentViewModel? Document
    {
        get => (DocumentViewModel?)GetValue(DocumentProperty);
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
