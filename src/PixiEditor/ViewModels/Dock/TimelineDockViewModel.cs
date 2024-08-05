﻿using PixiDocks.Core.Docking.Events;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.UI.Common.Fonts;
using PixiEditor.ViewModels.Document;

namespace PixiEditor.ViewModels.Dock;

internal class TimelineDockViewModel : DockableViewModel, IDockableSelectionEvents
{
    public const string TabId = "Timeline";

    public override string Id => TabId;
    public override string Title => new LocalizedString("TIMELINE_TITLE");
    public override bool CanFloat => true;
    public override bool CanClose => true;
    
    private DocumentManagerViewModel documentManagerSubViewModel;

    public DocumentManagerViewModel DocumentManagerSubViewModel
    {
        get => documentManagerSubViewModel;
        set => SetProperty(ref documentManagerSubViewModel, value);
    }

    public TimelineDockViewModel(DocumentManagerViewModel documentManagerViewModel)
    {
        DocumentManagerSubViewModel = documentManagerViewModel;
        TabCustomizationSettings.Icon = PixiPerfectIcons.ToIcon(PixiPerfectIcons.Timeline);
    }

    void IDockableSelectionEvents.OnSelected()
    {
        documentManagerSubViewModel.Owner.ShortcutController?.OverwriteContext(GetType());
    }

    void IDockableSelectionEvents.OnDeselected()
    {
        documentManagerSubViewModel.Owner.ShortcutController?.ClearContext(GetType());
    }
}