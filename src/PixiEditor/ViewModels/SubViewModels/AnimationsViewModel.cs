using Avalonia.Input;
using ChunkyImageLib;
using PixiEditor.AnimationRenderer.Core;
using PixiEditor.Models.AnalyticsAPI;
using PixiEditor.Models.IO;
using PixiEditor.Models.Commands.Attributes.Commands;
using Drawie.Numerics;
using PixiEditor.ViewModels.Dock;
using PixiEditor.ViewModels.Document;

namespace PixiEditor.ViewModels.SubViewModels;

[Command.Group("PixiEditor.Animations", "ANIMATIONS")]
internal class AnimationsViewModel : SubViewModel<ViewModelMain>
{
    public AnimationsViewModel(ViewModelMain owner) : base(owner)
    {
    }

    [Command.Basic("PixiEditor.Animation.NudgeActiveFrameNext", "CHANGE_ACTIVE_FRAME_NEXT",
        "CHANGE_ACTIVE_FRAME_NEXT",
        Parameter = 1, Key = Key.Right)]
    [Command.Basic("PixiEditor.Animation.NudgeActiveFramePrevious", "CHANGE_ACTIVE_FRAME_PREVIOUS",
        "CHANGE_ACTIVE_FRAME_PREVIOUS",
        Parameter = -1, Key = Key.Left)]
    public void ChangeActiveFrame(int nudgeBy)
    {
        var activeDocument = Owner.DocumentManagerSubViewModel.ActiveDocument;
        if (activeDocument is null || activeDocument.TransformViewModel.TransformActive)
            return;

        int newFrame = activeDocument.AnimationDataViewModel.ActiveFrameBindable + nudgeBy;
        newFrame = Math.Max(1, newFrame);
        activeDocument.Operations.SetActiveFrame(newFrame);
    }

    [Command.Basic("PixiEditor.Animation.TogglePlayAnimation", "TOGGLE_ANIMATION", "TOGGLE_ANIMATION",
        Key = Key.Space, Modifiers = KeyModifiers.Shift)]
    public void ToggleAnimation()
    {
        var activeDocument = Owner.DocumentManagerSubViewModel.ActiveDocument;
        if (activeDocument is null)
            return;

        activeDocument.AnimationDataViewModel.IsPlayingBindable = !activeDocument.AnimationDataViewModel.IsPlayingBindable;
    }

    [Command.Basic("PixiEditor.Animation.CreateRasterKeyFrame", "Create Raster Key Frame", "Create a raster key frame",
        Parameter = false, AnalyticsTrack = true)]
    [Command.Basic("PixiEditor.Animation.DuplicateRasterKeyFrame", "Duplicate Raster Key Frame",
        "Duplicate a raster key frame", Parameter = true, AnalyticsTrack = true)]
    public void CreateRasterKeyFrame(bool duplicate)
    {
        var activeDocument = Owner.DocumentManagerSubViewModel.ActiveDocument;
        if (activeDocument?.SelectedStructureMember is null)
        {
            return;
        }

        int newFrame = GetActiveFrame(activeDocument, activeDocument.SelectedStructureMember.Id);

        Guid toCloneFrom = duplicate ? activeDocument.SelectedStructureMember.Id : Guid.Empty;
        int frameToCopyFrom = duplicate ? activeDocument.AnimationDataViewModel.ActiveFrameBindable : -1;

        activeDocument.AnimationDataViewModel.CreateRasterKeyFrame(
            activeDocument.SelectedStructureMember.Id,
            newFrame,
            toCloneFrom,
            frameToCopyFrom);

        activeDocument.Operations.SetActiveFrame(newFrame);

        Analytics.SendCreateKeyframe(
            newFrame,
            "Raster",
            activeDocument.AnimationDataViewModel.FrameRateBindable,
            activeDocument.AnimationDataViewModel.FramesCount,
            activeDocument.AnimationDataViewModel.AllCels.Count);
    }

    [Command.Basic("PixiEditor.Animation.ToggleOnionSkinning", "TOGGLE_ONION_SKINNING",
        "TOGGLE_ONION_SKINNING_DESCRIPTIVE",
        ShortcutContext = typeof(TimelineDockViewModel), Key = Key.O, AnalyticsTrack = true)]
    public void ToggleOnionSkinning(bool value)
    {
        if (Owner.DocumentManagerSubViewModel.ActiveDocument is null)
            return;

        Owner.DocumentManagerSubViewModel.ActiveDocument?.AnimationDataViewModel.ToggleOnionSkinning(value);
    }

    [Command.Basic("PixiEditor.Animation.DeleteCels", "DELETE_CELS", "DELETE_CELS_DESCRIPTIVE",
        ShortcutContext = typeof(TimelineDockViewModel), Key = Key.Delete, AnalyticsTrack = true)]
    public void DeleteCels()
    {
        var activeDocument = Owner.DocumentManagerSubViewModel.ActiveDocument;
        var selected = activeDocument.AnimationDataViewModel.AllCels.Where(x => x.IsSelected).ToArray();

        if (activeDocument is null || selected.Length == 0)
            return;

        List<Guid> celIds = selected.Select(x => x.Id).ToList();

        for (int i = 0; i < celIds.Count; i++)
        {
            if (!activeDocument.AnimationDataViewModel.TryFindCels<CelViewModel>(celIds[i], out _))
            {
                celIds.RemoveAt(i);
                i--;
            }
        }

        activeDocument.AnimationDataViewModel.DeleteCels(celIds);
    }

    [Command.Internal("PixiEditor.Animation.ChangeKeyFramesStartPos")]
    public void ChangeKeyFramesStartPos((Guid[] ids, int delta, bool end) info)
    {
        var activeDocument = Owner.DocumentManagerSubViewModel.ActiveDocument;

        if (activeDocument is null)
            return;

        if (!info.end)
        {
            activeDocument.AnimationDataViewModel.ChangeKeyFramesStartPos(info.ids, info.delta);
        }
        else
        {
            activeDocument.AnimationDataViewModel.EndKeyFramesStartPos();
        }
    }


    private static int GetActiveFrame(DocumentViewModel activeDocument, Guid targetLayer)
    {
        int active = activeDocument.AnimationDataViewModel.ActiveFrameBindable;
        if (activeDocument.AnimationDataViewModel.TryFindCels<CelGroupViewModel>(targetLayer,
                out CelGroupViewModel groupViewModel))
        {
            if (groupViewModel.Children.All(x => !x.IsWithinRange(active )))
            {
                return active;
            }
            
            if (groupViewModel.Children.All(x => !x.IsWithinRange(active + 1)))
            {
                return active + 1;
            }
        }

        return active;
    }

    [Command.Internal("PixiEditor.Document.StartChangeActiveFrame", CanExecute = "PixiEditor.HasDocument")]
    public void StartChangeActiveFrame(int newActiveFrame)
    {
        if (Owner.DocumentManagerSubViewModel.ActiveDocument is null)
            return;
        // TODO: same as below
        //Owner.DocumentManagerSubViewModel.ActiveDocument.EventInlet.StartChangeActiveFrame();
    }

    [Command.Internal("PixiEditor.Document.ChangeActiveFrame", CanExecute = "PixiEditor.HasDocument")]
    public void ChangeActiveFrame(double newActiveFrame)
    {
        if (Owner.DocumentManagerSubViewModel.ActiveDocument is null)
            return;

        int intNewActiveFrame = (int)newActiveFrame;
        // TODO: Check if this should be implemented
        //Owner.DocumentManagerSubViewModel.ActiveDocument.EventInlet.ChangeActiveFrame(intNewActiveFrame);
    }

    [Command.Internal("PixiEditor.Document.EndChangeActiveFrame", CanExecute = "PixiEditor.HasDocument")]
    public void EndChangeActiveFrame()
    {
        if (Owner.DocumentManagerSubViewModel.ActiveDocument is null)
            return;

        //Owner.DocumentManagerSubViewModel.ActiveDocument.EventInlet.EndChangeActiveFrame();
    }

    [Command.Internal("PixiEditor.Animation.ActiveFrameSet")]
    public void ActiveFrameSet(double value)
    {
        var document = Owner.DocumentManagerSubViewModel.ActiveDocument;

        if (document is null)
            return;

        document.Operations.SetActiveFrame((int)value);
    }
}
