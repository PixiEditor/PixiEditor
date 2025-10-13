using System.Collections.ObjectModel;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using PixiEditor.ChangeableDocument.Changeables.Brushes;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.Helpers;
using PixiEditor.Models.BrushEngine;
using PixiEditor.Models.IO;
using PixiEditor.ViewModels.Document;
using PixiEditor.ViewModels.Document.Blackboard;
using PixiEditor.ViewModels.Tools.ToolSettings.Settings;

namespace PixiEditor.ViewModels.Nodes.Properties;

internal class DocumentPropertyViewModel : NodePropertyViewModel<IReadOnlyDocument>
{
    private DocumentViewModel docVm;
    private VariableViewModel? selectedBrushBlackboardVariable;
    public ICommand PickGraphFileCommand { get; }

    public VariableViewModel? SelectedBrushBlackboardVariable
    {
        get => selectedBrushBlackboardVariable;
        set
        {
            if (selectedBrushBlackboardVariable != null && selectedBrushBlackboardVariable.SettingView != null)
                selectedBrushBlackboardVariable.SettingView.PropertyChanged -= OnVariablePropertyChanged;
            if (SetProperty(ref selectedBrushBlackboardVariable, value) && value != null)
            {
                if (value.Value is Brush brush)
                {
                    Value?.Dispose();
                    Value = brush.Document.CloneInternalReadOnlyDocument();
                }
            }

            if(value != null && value.SettingView != null)
                value.SettingView.PropertyChanged += OnVariablePropertyChanged;
        }
    }

    public ObservableCollection<VariableViewModel> BrushBlackboardVariables =>
        new ObservableCollection<VariableViewModel>(docVm.NodeGraph?.Blackboard?.Variables.Where(x => x.Type == typeof(Brush)));

    public DocumentPropertyViewModel(NodeViewModel node, Type valueType) : base(node, valueType)
    {
        docVm = node.Document;
        PickGraphFileCommand = new AsyncRelayCommand(OnPickGraphFile);
    }

    private void OnVariablePropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(VariableViewModel.Value) && sender is BrushSettingViewModel vm)
        {
            if (vm.Value is { } brush)
            {
                Value = brush.Document.CloneInternalReadOnlyDocument();
            }
        }
    }

    private async Task OnPickGraphFile()
    {
        var any = new FileTypeDialogDataSet(FileTypeDialogDataSet.SetKind.Pixi).GetFormattedTypes(false);

        if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var dialog = await desktop.MainWindow.StorageProvider.OpenFilePickerAsync(
                new FilePickerOpenOptions { FileTypeFilter = any.ToList() });

            if (dialog.Count == 0 || !Importer.IsSupportedFile(dialog[0].Path.LocalPath))
                return;

            var doc = Importer.ImportDocument(dialog[0].Path.LocalPath);
            doc.Operations.InvokeCustomAction(() =>
            {
                Value = doc.CloneInternalReadOnlyDocument();
            });
        }
    }
}
