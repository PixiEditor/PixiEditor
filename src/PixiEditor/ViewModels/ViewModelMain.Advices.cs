using PixiEditor.Models.AdvisorSystem;
using PixiEditor.UI.Common.Localization;

namespace PixiEditor.ViewModels;

internal partial class ViewModelMain
{
    private void RegisterAdvices()
    {
        Advice advice = new Advice("NeedsNewLayer", new LocalizedString("DRAW_NESTED_ADVICE"));
        advice.UserDismissed += () => IoSubViewModel.LayerNeedsNewLayer -= NewLayerNeeded;
        IAdvisor.Current.RegisterAdvice("NeedsNewLayer", advice);

        IoSubViewModel.LayerNeedsNewLayer += NewLayerNeeded;
    }

    private void NewLayerNeeded()
    {
        IAdvisor.Current.RequestAdvice("NeedsNewLayer");
    }
}
