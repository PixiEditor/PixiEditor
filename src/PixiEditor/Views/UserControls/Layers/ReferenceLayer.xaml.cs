using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using PixiEditor.Models.Commands;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.IO;
using PixiEditor.ViewModels.SubViewModels.Document;

namespace PixiEditor.Views.UserControls.Layers;

#nullable enable
internal partial class ReferenceLayer : UserControl
{
    public static readonly DependencyProperty DocumentProperty =
        DependencyProperty.Register(nameof(Document), typeof(DocumentViewModel), typeof(ReferenceLayer), new(null));

    public DocumentViewModel? Document
    {
        get => (DocumentViewModel?)GetValue(DocumentProperty);
        set => SetValue(DocumentProperty, value);
    }

    public ReferenceLayer()
    {
        InitializeComponent();
    }

    private void ReferenceLayer_DragEnter(object sender, DragEventArgs e)
    {
        ViewModelMain.Current.ActionDisplays[nameof(ReferenceLayer_Drop)] = "Import as reference layer";
        e.Handled = true;
    }

    private void ReferenceLayer_DragLeave(object sender, DragEventArgs e)
    {
        ViewModelMain.Current.ActionDisplays[nameof(ReferenceLayer_Drop)] = null;
        e.Handled = true;
    }

    private void ReferenceLayer_Drop(object sender, DragEventArgs e)
    {
        var command = CommandController.Current.Commands["PixiEditor.Layer.PasteReferenceLayer"];

        if (!command.Methods.CanExecute(e.Data))
        {
            return;
        }

        command.Methods.Execute(e.Data);
        e.Handled = true;
    }
}
