using System.Drawing;
using Avalonia.Input;
using PixiEditor.ChangeableDocument.Enums;
using Drawie.Backend.Core.Numerics;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.Models.Commands.Attributes.Evaluators;
using PixiEditor.Models.DocumentModels.UpdateableChangeExecutors.Features;
using Drawie.Numerics;
using PixiEditor.UI.Common.Fonts;
using PixiEditor.ViewModels.Document;

namespace PixiEditor.ViewModels.SubViewModels;

[Command.Group("PixiEditor.Selection", "SELECTION")]
internal class SelectionViewModel : SubViewModel<ViewModelMain>
{
    public SelectionViewModel(ViewModelMain owner)
        : base(owner)
    {
    }

    [Command.Basic("PixiEditor.Selection.SelectAll", "SELECT_ALL", "SELECT_ALL_DESCRIPTIVE", CanExecute = "PixiEditor.HasDocument", Key = Key.A, Modifiers = KeyModifiers.Control,
        MenuItemPath = "SELECT/SELECT_ALL", MenuItemOrder = 8, Icon = PixiPerfectIcons.SelectAll, AnalyticsTrack = true)]
    public void SelectAll()
    {
        var doc = Owner.DocumentManagerSubViewModel.ActiveDocument;
        if (doc is null)
            return;
        doc.Operations.SelectAll();
    }

    [Command.Basic("PixiEditor.Selection.Clear", "CLEAR_SELECTION", "CLEAR_SELECTION", CanExecute = "PixiEditor.Selection.IsNotEmpty", Key = Key.D, Modifiers = KeyModifiers.Control,
        MenuItemPath = "SELECT/DESELECT", MenuItemOrder = 9, Icon = PixiPerfectIcons.Deselect, AnalyticsTrack = true)]
    public void ClearSelection()
    {
        var doc = Owner.DocumentManagerSubViewModel.ActiveDocument;
        if (doc is null)
            return;
        doc.Operations.ClearSelection();
    }

    [Command.Basic("PixiEditor.Selection.InvertSelection", "INVERT_SELECTION", "INVERT_SELECTION_DESCRIPTIVE", CanExecute = "PixiEditor.Selection.IsNotEmpty", Key = Key.I, Modifiers = KeyModifiers.Control,
        MenuItemPath = "SELECT/INVERT", MenuItemOrder = 10, Icon = PixiPerfectIcons.Invert, AnalyticsTrack = true)]
    public void InvertSelection()
    {
        Owner.DocumentManagerSubViewModel.ActiveDocument?.Operations.InvertSelection();
    }

    [Evaluator.CanExecute("PixiEditor.Selection.IsNotEmpty",
        nameof(DocumentManagerViewModel.ActiveDocument),
        nameof(DocumentManagerViewModel.ActiveDocument.SelectionPathBindable),
        nameof(DocumentManagerViewModel.ActiveDocument.SelectionPathBindable.IsEmpty))]
    public bool SelectionIsNotEmpty()
    {
        return !Owner.DocumentManagerSubViewModel.ActiveDocument?.SelectionPathBindable?.IsEmpty ?? false;
    }

    [Evaluator.CanExecute("PixiEditor.Selection.IsNotEmptyAndHasMask", 
        nameof(DocumentManagerViewModel.ActiveDocument),
        nameof(DocumentManagerViewModel.ActiveDocument.SelectedStructureMember),
        nameof(DocumentManagerViewModel.ActiveDocument.SelectedStructureMember.HasMaskBindable))]
    public bool SelectionIsNotEmptyAndHasMask()
    {
        return SelectionIsNotEmpty() && (Owner.DocumentManagerSubViewModel.ActiveDocument?.SelectedStructureMember?.HasMaskBindable ?? false);
    }

    [Command.Basic("PixiEditor.Selection.TransformArea", "TRANSFORM_SELECTED_AREA", "TRANSFORM_SELECTED_AREA", CanExecute = "PixiEditor.Selection.IsNotEmpty", 
        Key = Key.T, Modifiers = KeyModifiers.Control, AnalyticsTrack = true)]
    public void TransformSelectedArea()
    {
        Owner.DocumentManagerSubViewModel.ActiveDocument?.Operations.TransformSelectedArea(false);
    }

    [Command.Basic("PixiEditor.Selection.NudgeSelectedObjectLeft", "NUDGE_SELECTED_LEFT", "NUDGE_SELECTED_LEFT", Key = Key.Left, Parameter = new int[] { -1, 0 }, Icon = PixiPerfectIcons.ChevronLeft, CanExecute = "PixiEditor.Selection.CanNudgeSelectedObject",
        ShortcutContexts = [typeof(ViewportWindowViewModel)])]
    [Command.Basic("PixiEditor.Selection.NudgeSelectedObjectRight", "NUDGE_SELECTED_RIGHT", "NUDGE_SELECTED_RIGHT", Key = Key.Right, Parameter = new int[] { 1, 0 }, Icon = PixiPerfectIcons.ChevronRight, CanExecute = "PixiEditor.Selection.CanNudgeSelectedObject",
        ShortcutContexts = [typeof(ViewportWindowViewModel)])]
    [Command.Basic("PixiEditor.Selection.NudgeSelectedObjectUp", "NUDGE_SELECTED_UP", "NUDGE_SELECTED_UP", Key = Key.Up, Parameter = new int[] { 0, -1 }, Icon = PixiPerfectIcons.ChevronUp, CanExecute = "PixiEditor.Selection.CanNudgeSelectedObject",
        ShortcutContexts = [typeof(ViewportWindowViewModel)])]
    [Command.Basic("PixiEditor.Selection.NudgeSelectedObjectDown", "NUDGE_SELECTED_DOWN", "NUDGE_SELECTED_DOWN", Key = Key.Down, Parameter = new int[] { 0, 1 }, Icon = PixiPerfectIcons.ChevronDown, CanExecute = "PixiEditor.Selection.CanNudgeSelectedObject",
        ShortcutContexts = [typeof(ViewportWindowViewModel)])]
    public void NudgeSelectedObject(int[] dist)
    {
        VecI distance = new(dist[0], dist[1]);
        Owner.DocumentManagerSubViewModel.ActiveDocument?.Operations.NudgeSelectedObject(distance);
    }

    [Command.Basic("PixiEditor.Selection.NewToMask", SelectionMode.New, "MASK_FROM_SELECTION", "MASK_FROM_SELECTION_DESCRIPTIVE", CanExecute = "PixiEditor.Selection.IsNotEmpty",
        MenuItemPath = "SELECT/SELECTION_TO_MASK/TO_NEW_MASK", MenuItemOrder = 12, Icon = PixiPerfectIcons.NewMask, AnalyticsTrack = true)]
    [Command.Basic("PixiEditor.Selection.AddToMask", SelectionMode.Add, "ADD_SELECTION_TO_MASK", "ADD_SELECTION_TO_MASK", CanExecute = "PixiEditor.Selection.IsNotEmpty",
        MenuItemPath = "SELECT/SELECTION_TO_MASK/ADD_TO_MASK", MenuItemOrder = 13, Icon = PixiPerfectIcons.AddToMask, AnalyticsTrack = true)]
    [Command.Basic("PixiEditor.Selection.SubtractFromMask", SelectionMode.Subtract, "SUBTRACT_SELECTION_FROM_MASK", "SUBTRACT_SELECTION_FROM_MASK", CanExecute = "PixiEditor.Selection.IsNotEmptyAndHasMask",
        MenuItemPath = "SELECT/SELECTION_TO_MASK/SUBTRACT_FROM_MASK", MenuItemOrder = 14, Icon = PixiPerfectIcons.Subtract, AnalyticsTrack = true)]
    [Command.Basic("PixiEditor.Selection.IntersectSelectionMask", SelectionMode.Intersect, "INTERSECT_SELECTION_MASK", "INTERSECT_SELECTION_MASK", CanExecute = "PixiEditor.Selection.IsNotEmptyAndHasMask",
        MenuItemPath = "SELECT/SELECTION_TO_MASK/INTERSECT_WITH_MASK", MenuItemOrder = 15, Icon = PixiPerfectIcons.Intersect, AnalyticsTrack = true)]
    [Command.Filter("PixiEditor.Selection.ToMaskMenu", "SELECTION_TO_MASK", "SELECTION_TO_MASK", Key = Key.M, Modifiers = KeyModifiers.Control, AnalyticsTrack = true)]
    public void SelectionToMask(SelectionMode mode)
    {
        if (Owner.DocumentManagerSubViewModel.ActiveDocument is null)
            return;
        
        Owner.DocumentManagerSubViewModel.ActiveDocument.Operations.SelectionToMask(mode, Owner.DocumentManagerSubViewModel.ActiveDocument.AnimationDataViewModel.ActiveFrameBindable);
    }

    [Command.Basic("PixiEditor.Selection.CropToSelection", "CROP_TO_SELECTION", "CROP_TO_SELECTION_DESCRIPTIVE", CanExecute = "PixiEditor.Selection.IsNotEmpty",
        MenuItemPath = "SELECT/CROP_TO_SELECTION", MenuItemOrder = 11, Icon = PixiPerfectIcons.CropToSelection, AnalyticsTrack = true)]
    public void CropToSelection()
    {
        var document = Owner.DocumentManagerSubViewModel.ActiveDocument;
        
        document!.Operations.CropToSelection(document.AnimationDataViewModel.ActiveFrameBindable);
    }

    [Evaluator.CanExecute("PixiEditor.Selection.CanNudgeSelectedObject",
        nameof(DocumentManagerViewModel.ActiveDocument))]
    public bool CanNudgeSelectedObject(int[] dist) => Owner.DocumentManagerSubViewModel.ActiveDocument
        ?.IsChangeFeatureActive<ITransformableExecutor>() ?? false;
}
