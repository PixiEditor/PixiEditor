using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using PixiEditor.Models.Commands;
using PixiEditor.Models.Commands.Commands;
using PixiEditor.ViewModels;
using PixiEditor.ViewModels.Document;

namespace PixiEditor.Views.Layers;

#nullable enable
internal partial class ReferenceLayer : UserControl
{
    private Command command;
    private Rect originalBounds;

    public static readonly StyledProperty<DocumentViewModel> DocumentProperty =
        AvaloniaProperty.Register<ReferenceLayer, DocumentViewModel>(
            nameof(Document));

    public DocumentViewModel Document
    {
        get => GetValue(DocumentProperty);
        set => SetValue(DocumentProperty, value);
    }

    static ReferenceLayer()
    {
        DocumentProperty.Changed.Subscribe(OnDocumentChanged);
    }

    public ReferenceLayer()
    {
        command = CommandController.Current.Commands["PixiEditor.Clipboard.PasteReferenceLayer"];
        InitializeComponent();

        DragBorder.AddHandler(DragDrop.DragEnterEvent, ReferenceLayer_DragEnter);
        DragBorder.AddHandler(DragDrop.DragLeaveEvent, ReferenceLayer_DragLeave);
        DragBorder.AddHandler(DragDrop.DropEvent, ReferenceLayer_Drop);
    }

    private static void OnDocumentChanged(AvaloniaPropertyChangedEventArgs<DocumentViewModel> e)
    {
        ReferenceLayer referenceLayer = (ReferenceLayer)e.Sender;
        if (e.OldValue.HasValue && e.OldValue.Value != null)
        {
            e.OldValue.Value.ReferenceLayerViewModel.PropertyChanged -= referenceLayer.OnDocumentPropertyChanged;
        }

        if (e.NewValue.HasValue && e.NewValue.Value != null)
        {
            e.NewValue.Value.ReferenceLayerViewModel.PropertyChanged += referenceLayer.OnDocumentPropertyChanged;
        }
    }

    private void OnDocumentPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ReferenceLayerViewModel.IsTopMost))
        {
            PseudoClasses.Set(":topmost", Document.ReferenceLayerViewModel.IsTopMost);
        }
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
        if (!command.Methods.CanExecute(e.DataTransfer))
        {
            return;
        }

        command.Methods.Execute(e.DataTransfer);
        e.Handled = true;
    }
}
