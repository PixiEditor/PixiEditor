using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
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
}
