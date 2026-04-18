using PixiDocks.Core.Docking;
using PixiEditor.Extensions.CommonApi.UserPreferences;
using PixiEditor.Extensions.CommonApi.UserPreferences.Settings.PixiEditor;
using PixiEditor.Models.AdvisorSystem;
using PixiEditor.UI.Common.Localization;
using PixiEditor.ViewModels.Dock;
using PixiEditor.ViewModels.Document.Nodes;

namespace PixiEditor.ViewModels.SubViewModels;

internal class AdvicesViewModel : SubViewModel<ViewModelMain>
{
    public IAdvisor Advisor { get; }
    const string NeedsNewLayerNestedAdviceKey = "NeedsNewLayerNested";
    const string AddEmptyFrame = "AddEmptyFrame";
    const string OpenGraph = "OpenGraph";

    public AdvicesViewModel(ViewModelMain owner, IAdvisor advisor) : base(owner)
    {
        Advisor = advisor;
    }

    public void RegisterAdvices()
    {
        if (!IPreferences.Current.GetLocalPreference<bool>($"Advice{NeedsNewLayerNestedAdviceKey}Dismissed"))
        {
            Advice advice = new Advice(NeedsNewLayerNestedAdviceKey, new LocalizedString("DRAW_NESTED_ADVICE"));
            advice.UserDismissed += () => Owner.IoSubViewModel.LayerNeedsNewLayer -= NewLayerNeeded;
            Advisor.RegisterAdvice(NeedsNewLayerNestedAdviceKey, advice);
            Owner.IoSubViewModel.LayerNeedsNewLayer += NewLayerNeeded;
        }

        if (!IPreferences.Current.GetLocalPreference<bool>($"Advice{AddEmptyFrame}Dismissed"))
        {
            Advice addEmptyFrameAdvice = new Advice(AddEmptyFrame, new LocalizedString("ADD_EMPTY_FRAME_ADVICE"));
            Advisor.RegisterAdvice(AddEmptyFrame, addEmptyFrameAdvice);
            Owner.AnimationsSubViewModel.OnCreateCel += CreatedCel;
            addEmptyFrameAdvice.UserDismissed += () => Owner.AnimationsSubViewModel.OnCreateCel -= CreatedCel;
        }

        if (!IPreferences.Current.GetLocalPreference<bool>($"Advice{OpenGraph}Dismissed"))
        {
            Advice openedGraphAdvice = new Advice(OpenGraph, new LocalizedString("OPENED_GRAPH_ADVICE"));
            Advisor.RegisterAdvice(OpenGraph, openedGraphAdvice);
            Owner.LayoutSubViewModel.LayoutManager.DockContext.DockableFocused += OnFocusedTargetChanged;
            openedGraphAdvice.UserDismissed += () => Owner.LayoutSubViewModel.LayoutManager.DockContext.DockableFocused -= OnFocusedTargetChanged;
        }
    }

    private void NewLayerNeeded()
    {
        var activeLayer = Owner.DocumentManagerSubViewModel.ActiveDocument?.SelectedStructureMember;
        if (activeLayer is NestedDocumentNodeViewModel)
        {
            var advice = Advisor.RequestAdvice("NeedsNewLayerNested")
                .WithChoices("AUTO_RASTERIZE", "AUTO_CREATE_NEW_LAYER")
                .OnChoiceSelected(choice =>
                {
                    PixiEditorSettings.Tools.AutoRasterizeNestedLayersOnDraw.Value = choice == 0;
                    IPreferences.Current.UpdateLocalPreference($"Advice{NeedsNewLayerNestedAdviceKey}Dismissed", true);
                    Owner.IoSubViewModel.LayerNeedsNewLayer -= NewLayerNeeded;
                })
                .WithFollowUpAdvice(new Advice("GotIt", "GOT_IT_CHECK_SETTINGS"));

            Advisor.SendAdvice(NeedsNewLayerNestedAdviceKey, advice);
        }
    }

    private void CreatedCel()
    {
        var advice = Advisor.RequestAdvice("AddEmptyFrame")
            .WithFollowUpAdvice(new Advice("MakeGroupInvisibleForALayer", "DISABLE_ANIM_LAYER_ADVICE")
                .WithAutoDismiss(false)).OnDismissed(() =>
                {
                    IPreferences.Current.UpdateLocalPreference($"Advice{AddEmptyFrame}Dismissed", true);
                    Owner.AnimationsSubViewModel.OnCreateCel -= CreatedCel;
                });
        Advisor.SendAdvice(AddEmptyFrame, advice);
    }

    private void OnFocusedTargetChanged(IDockable dockable)
    {
        if (dockable.Id == NodeGraphDockViewModel.TabId)
        {
            var advice = Advisor.RequestAdvice("OpenGraph")
                .WithFollowUpAdvice(new Advice("OpenGraphFollowUp","OPENED_GRAPH_ADVICE_FOLLOW_UP")
                    .WithAutoDismiss(false)).OnDismissed(() =>
                    {
                        IPreferences.Current.UpdateLocalPreference($"Advice{OpenGraph}Dismissed", true);
                        Owner.LayoutSubViewModel.LayoutManager.DockContext.DockableFocused -= OnFocusedTargetChanged;
                    });
            Advisor.SendAdvice(OpenGraph, advice);
        }
    }
}
