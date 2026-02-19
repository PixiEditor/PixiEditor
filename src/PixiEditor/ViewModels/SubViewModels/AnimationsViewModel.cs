using System.ComponentModel;
using Avalonia.Input;
using Avalonia.Threading;
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
    private DispatcherTimer _playTimer;

    public AnimationsViewModel(ViewModelMain owner) : base(owner)
    {
        owner.DocumentManagerSubViewModel.ActiveDocumentChanged += (sender, args) =>
        {
            if (args.NewDocument != null)
            {
                InitTimer();
                args.NewDocument.AnimationDataViewModel.PropertyChanged += AnimationDataViewModelOnPropertyChanged;
            }

            TogglePlayTimer(args.NewDocument?.AnimationDataViewModel.IsPlayingBindable ?? false);
        };
    }

    private void AnimationDataViewModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(AnimationDataViewModel.IsPlayingBindable))
        {
            TogglePlayTimer(Owner.DocumentManagerSubViewModel.ActiveDocument.AnimationDataViewModel.IsPlayingBindable);
        }
        else if (e.PropertyName == nameof(AnimationDataViewModel.FrameRateBindable))
        {
            InitTimer();
        }
    }

    private void InitTimer()
    {
        if (_playTimer != null)
        {
            _playTimer.Stop();
            _playTimer.Tick -= PlayTimerOnTick;
        }
        
        var activeDocument = Owner.DocumentManagerSubViewModel.ActiveDocument;
        _playTimer =
            new DispatcherTimer(DispatcherPriority.Render) { Interval = TimeSpan.FromMilliseconds(1000f / activeDocument.AnimationDataViewModel.FrameRateBindable) };
        _playTimer.Tick += PlayTimerOnTick;
    }

    private void TogglePlayTimer(bool isPlaying)
    {
        if (isPlaying)
        {
            if (_playTimer is null)
            {
                return;
            }

            _playTimer.Start();
        }
        else
        {
            _playTimer?.Stop();
        }
    }

    private void PlayTimerOnTick(object? sender, EventArgs e)
    {
        var activeDocument = Owner.DocumentManagerSubViewModel.ActiveDocument;
        if (activeDocument.AnimationDataViewModel.ActiveFrameBindable + 1 >= activeDocument.AnimationDataViewModel.LastFrame)
        {
            activeDocument.AnimationDataViewModel.ActiveFrameBindable = 1;
        }
        else
        {
            activeDocument.AnimationDataViewModel.ActiveFrameBindable++;
        }
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
        if (activeDocument is null || IsTransforming())
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

        activeDocument.AnimationDataViewModel.IsPlayingBindable =
            !activeDocument.AnimationDataViewModel.IsPlayingBindable;
    }

    [Command.Basic("PixiEditor.Animation.CreateCel", "CREATE_CEL", "CREATE_CEL_DESCRIPTIVE",
        Parameter = false, AnalyticsTrack = true)]
    [Command.Basic("PixiEditor.Animation.DuplicateCel", "DUPLICATE_CEL",
        "DUPLICATE_CEL_DESCRIPTIVE", Parameter = true, AnalyticsTrack = true)]
    public void CreateCel(bool duplicate)
    {
        var activeDocument = Owner.DocumentManagerSubViewModel.ActiveDocument;
        if (activeDocument?.SelectedStructureMember is null)
        {
            return;
        }

        var kfAtFrame = activeDocument.AnimationDataViewModel.AllCels.FirstOrDefault(x =>
            x.IsWithinRange(activeDocument.AnimationDataViewModel.ActiveFrameBindable));

        int newFrame = kfAtFrame != null
            ? kfAtFrame.StartFrameBindable + kfAtFrame.DurationBindable - 1
            : activeDocument.AnimationDataViewModel.ActiveFrameBindable;

        Guid toCloneFrom = duplicate ? activeDocument.SelectedStructureMember.Id : Guid.Empty;
        int frameToCopyFrom = duplicate ? activeDocument.AnimationDataViewModel.ActiveFrameBindable : -1;

        activeDocument.AnimationDataViewModel.CreateCel(
            activeDocument.SelectedStructureMember.Id,
            newFrame,
            toCloneFrom,
            frameToCopyFrom);

        int newPos = kfAtFrame != null ? kfAtFrame.StartFrameBindable + kfAtFrame.DurationBindable : activeDocument.AnimationDataViewModel.ActiveFrameBindable;
        activeDocument.Operations.SetActiveFrame(newPos);

        Analytics.SendCreateKeyframe(
            newFrame,
            "Raster",
            activeDocument.AnimationDataViewModel.FrameRateBindable,
            activeDocument.AnimationDataViewModel.FramesCount,
            activeDocument.AnimationDataViewModel.AllCels.Count);
    }

    [Command.Basic("PixiEditor.Animation.ToggleOnionSkinning", "TOGGLE_ONION_SKINNING",
        "TOGGLE_ONION_SKINNING_DESCRIPTIVE",
        ShortcutContexts = [typeof(TimelineDockViewModel)], Key = Key.O, AnalyticsTrack = true)]
    public void ToggleOnionSkinning(bool value)
    {
        if (Owner.DocumentManagerSubViewModel.ActiveDocument is null)
            return;

        Owner.DocumentManagerSubViewModel.ActiveDocument?.AnimationDataViewModel.ToggleOnionSkinning(value);
    }

    [Command.Basic("PixiEditor.Animation.DeleteCels", "DELETE_CELS", "DELETE_CELS_DESCRIPTIVE",
        ShortcutContexts = [typeof(TimelineDockViewModel)], Key = Key.Delete, AnalyticsTrack = true)]
    public void DeleteCels()
    {
        var activeDocument = Owner.DocumentManagerSubViewModel.ActiveDocument;
        var selected = activeDocument?.AnimationDataViewModel?.AllCels.Where(x => x is { IsSelected: true }).ToArray();

        if (activeDocument is null || selected == null || selected.Length == 0)
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

        if (activeDocument is null || info == default)
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

    private bool IsTransforming()
    {
        var activeDocument = Owner.DocumentManagerSubViewModel.ActiveDocument;
        if (activeDocument is null)
            return false;

        return activeDocument.TransformViewModel.TransformActive || activeDocument.LineToolOverlayViewModel.IsEnabled
                                                                 || activeDocument.PathOverlayViewModel.IsActive;
    }
}
