using System.ComponentModel;
using System.Runtime.InteropServices;
using Avalonia.Interactivity;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using PixiEditor.Helpers;
using PixiEditor.Models.Dialogs;
using PixiEditor.Models.Handlers;
using PixiEditor.Models.IO;
using PixiEditor.OperatingSystem;
using PixiEditor.UI.Common.Localization;

namespace PixiEditor.ViewModels.Nodes.Properties;

internal class StringPropertyViewModel : NodePropertyViewModel<string>
{
    private string kind = "txt";
    public string StringValue
    {
        get => Value;
        set => Value = value;
    }

    public string Kind
    {
        get => kind;
        set => SetProperty(ref kind, value);
    }

    public StringPropertyViewModel(NodeViewModel node, Type valueType) : base(node, valueType)
    {
        PropertyChanged += StringPropertyViewModel_PropertyChanged;
    }

    private void StringPropertyViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Value))
        {
            OnPropertyChanged(nameof(StringValue));
        }
    }
}
