using System.Collections.ObjectModel;
using System.ComponentModel;
using AvaloniaEdit.Utils;
using PixiEditor.Models.Handlers;
using PixiEditor.UI.Common.Localization;
using PixiEditor.ViewModels.Document;
using PixiEditor.ViewModels.Document.Properties;

namespace PixiEditor.ViewModels.Dock;

internal class PropertiesDockViewModel : DockableViewModel
{
    public override string Id { get; } = "Properties";
    public override string Title => new LocalizedString("PROPERTIES_TITLE");
    public override bool CanFloat { get; } = true;
    public override bool CanClose { get; } = true;

    public DocumentManagerViewModel DocumentManagerViewModel { get; }

    public LocalizedString ObjectTitle
    {
        get;
        set => SetProperty(ref field, value);
    }

    public ObservableCollection<PropertyViewModel> Properties
    {
        get;
        set => SetProperty(ref field, value);
    }

    private IPropertiesObject? currentPropertiesObject;

    public PropertiesDockViewModel(DocumentManagerViewModel documentManagerViewModel)
    {
        DocumentManagerViewModel = documentManagerViewModel;
        DocumentManagerViewModel.ActiveDocumentChanged += DocumentManagerViewModelOnActiveDocumentChanged;
    }

    private void DocumentManagerViewModelOnActiveDocumentChanged(object? sender, DocumentChangedEventArgs e)
    {
        if (e.OldDocument != null)
        {
            e.OldDocument.PropertyChanged += NewDocumentOnPropertyChanged;
        }
        if (e.NewDocument == null)
        {
            Properties = new ObservableCollection<PropertyViewModel>();
            ObjectTitle = "";
            currentPropertiesObject?.PropertiesUpdated -= OnPropertiesUpdated;
            currentPropertiesObject = null;
            return;
        }

        Properties = new ObservableCollection<PropertyViewModel>(e.NewDocument.Properties);
        ObjectTitle = e.NewDocument.LabelKey;

        currentPropertiesObject?.PropertiesUpdated -= OnPropertiesUpdated;
        e.NewDocument.PropertyChanged += NewDocumentOnPropertyChanged;

        currentPropertiesObject = e.NewDocument;

        currentPropertiesObject?.PropertiesUpdated += OnPropertiesUpdated;
    }

    private void NewDocumentOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(DocumentViewModel.SelectedStructureMember))
        {
            if (DocumentManagerViewModel.ActiveDocument.SelectedStructureMember is IPropertiesObject propertiesObject)
            {
                SetFromPropertiesObject(propertiesObject);
            }
            else
            {
                SetFromPropertiesObject(DocumentManagerViewModel.ActiveDocument);
            }
        }
    }

    private void SetFromPropertiesObject(IPropertiesObject propertiesObject)
    {
        currentPropertiesObject?.StopUpdates();
        currentPropertiesObject?.PropertiesUpdated -= OnPropertiesUpdated;
        Properties = new ObservableCollection<PropertyViewModel>(propertiesObject.Properties);
        ObjectTitle = propertiesObject.LabelKey;
        currentPropertiesObject = propertiesObject;
        currentPropertiesObject?.PropertiesUpdated += OnPropertiesUpdated;

        currentPropertiesObject.StartUpdates();
    }

    private void OnPropertiesUpdated(IReadOnlyCollection<PropertyViewModel> prop)
    {
        Properties = new ObservableCollection<PropertyViewModel>(prop);
    }
}
