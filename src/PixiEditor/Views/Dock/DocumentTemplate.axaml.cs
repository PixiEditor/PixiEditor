using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using PixiEditor.Models.Preferences;
using PixiEditor.ViewModels.Dock;
using PixiEditor.Views.Palettes;
using PixiEditor.Extensions.CommonApi.UserPreferences.Settings.PixiEditor;
using PixiEditor.ViewModels.SubViewModels;
using PixiEditor.ViewModels.Tools.Tools;

namespace PixiEditor.Views.Dock;

public partial class DocumentTemplate : UserControl
{
    public DocumentTemplate()
    {
        InitializeComponent();
    }
}

