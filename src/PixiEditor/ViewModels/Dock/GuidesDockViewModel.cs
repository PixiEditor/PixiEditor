using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using PixiEditor.Helpers.Converters;
using PixiEditor.UI.Common.Converters;
using PixiEditor.UI.Common.Fonts;
using PixiEditor.UI.Common.Localization;
using PixiEditor.ViewModels.Document;
using PixiEditor.ViewModels.SubViewModels;

namespace PixiEditor.ViewModels.Dock;

internal partial class GuidesDockViewModel : DockableViewModel
{
    public const string TabId = "Guides";
    public override string Id => TabId;
    public override string Title => new LocalizedString("GUIDES_TITLE");
    public override bool CanFloat => true;
    public override bool CanClose => true;

    private GuidesViewModel guidesViewModel;

    public GuidesViewModel GuidesSubViewModel
    {
        get => guidesViewModel;
        set => SetProperty(ref guidesViewModel, value);
    }

    [ObservableProperty]
    public partial MeasurementConverterData MeasurementConverterData { get; set; }

    [ObservableProperty]
    public partial DocumentViewModel Document { get; set; }

    public GuidesDockViewModel(DocumentManagerViewModel docManager)
    {
        docManager.ActiveDocumentChanged += DocManagerOnActiveDocumentChanged;

        MeasurementConverterData = new MeasurementConverterData(
            () => docManager.ActiveDocument.DpiBindable,
            (val, dpi) => docManager.ActiveDocument.MeasurementUnitBindable.FromPixels(val, dpi),
            (val, dpi) => docManager.ActiveDocument.MeasurementUnitBindable.ToPixels(val, dpi));

        TabCustomizationSettings.Icon = PixiPerfectIconExtensions.ToIcon(PixiPerfectIcons.ClosedGrid);
    }

    private void DocManagerOnActiveDocumentChanged(object? sender, DocumentChangedEventArgs e)
    {
        if (e.OldDocument != null)
        {
            e.OldDocument.PropertyChanged -= NewDocumentOnPropertyChanged;
        }

        e.NewDocument?.PropertyChanged += NewDocumentOnPropertyChanged;
        GuidesSubViewModel = e.NewDocument?.GuidesViewModel;
        Document = e.NewDocument;
    }

    private void NewDocumentOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(DocumentViewModel.MeasurementUnitBindable))
        {
            MeasurementConverterData= new MeasurementConverterData(
                () => Document.DpiBindable,
                (val, dpi) => Document.MeasurementUnitBindable.FromPixels(val, dpi),
                (val, dpi) => Document.MeasurementUnitBindable.ToPixels(val, dpi));
        }
    }
}
