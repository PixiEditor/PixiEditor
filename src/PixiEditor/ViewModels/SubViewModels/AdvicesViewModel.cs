using PixiEditor.Extensions.CommonApi.UserPreferences;
using PixiEditor.Extensions.CommonApi.UserPreferences.Settings.PixiEditor;
using PixiEditor.Models.AdvisorSystem;
using PixiEditor.UI.Common.Localization;
using PixiEditor.ViewModels.Document.Nodes;

namespace PixiEditor.ViewModels.SubViewModels;

internal class AdvicesViewModel : SubViewModel<ViewModelMain>
{
    const string NeedsNewLayerNestedAdviceKey = "NeedsNewLayerNested";

    public AdvicesViewModel(ViewModelMain owner) : base(owner)
    {
    }

    public void RegisterAdvices()
    {
        Advice advice = new Advice(NeedsNewLayerNestedAdviceKey, new LocalizedString("DRAW_NESTED_ADVICE"));
        advice.UserDismissed += () => Owner.IoSubViewModel.LayerNeedsNewLayer -= NewLayerNeeded;
        if (!IPreferences.Current.GetLocalPreference<bool>($"Advice{NeedsNewLayerNestedAdviceKey}Dismissed"))
        {
            IAdvisor.Current.RegisterAdvice(NeedsNewLayerNestedAdviceKey, advice);
            Owner.IoSubViewModel.LayerNeedsNewLayer += NewLayerNeeded;
        }
    }

    private void NewLayerNeeded()
    {
        var activeLayer = Owner.DocumentManagerSubViewModel.ActiveDocument?.SelectedStructureMember;
        if (activeLayer is NestedDocumentNodeViewModel)
        {
            var advice = IAdvisor.Current.RequestAdvice("NeedsNewLayerNested")
                .WithChoices(new LocalizedString("AUTO_RASTERIZE"), new LocalizedString("AUTO_CREATE_NEW_LAYER"))
                .OnChoiceSelected(choice =>
                {
                    PixiEditorSettings.Tools.AutoRasterizeNestedLayersOnDraw.Value = choice == 0;
                    IPreferences.Current.UpdateLocalPreference($"Advice{NeedsNewLayerNestedAdviceKey}Dismissed", true);
                    Owner.IoSubViewModel.LayerNeedsNewLayer -= NewLayerNeeded;
                })
                .WithFollowUpAdvice(new Advice("GotIt", new LocalizedString("GOT_IT_CHECK_SETTINGS")));

            IAdvisor.Current.SendAdvice(NeedsNewLayerNestedAdviceKey, advice);
        }
    }
}
