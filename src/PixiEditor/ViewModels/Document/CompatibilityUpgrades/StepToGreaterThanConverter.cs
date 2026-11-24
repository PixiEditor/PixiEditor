using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.Enums;
using PixiEditor.UI.Common.Localization;
using PixiEditor.ViewModels.Document.Nodes;

namespace PixiEditor.ViewModels.Document.CompatibilityUpgrades;

/// <summary>Preserves old CPU Step output by converting non-contextful nodes to GreaterThan.</summary>
internal class StepToGreaterThanConverter(DocumentViewModel documentViewModel) : IGraphUpgrader
{
    public event Action? UpgradeCompleted;
    public LocalizedString UpgradeText { get; } = new LocalizedString("STEP");

    public void Upgrade()
    {
        using var block = documentViewModel.Operations.StartChangeBlock();
        PerformImmediateCompatibilityConversion();
        UpgradeCompleted?.Invoke();
    }

    public void PerformImmediateCompatibilityConversion()
    {
        foreach (var node in documentViewModel.NodeGraph.AllNodes.OfType<MathNodeViewModel>())
        {
            var mode = node.FindInputProperty(MathNode.ModePropertyName);
            if (mode.Value is MathNodeMode.Step && mode.ConnectedOutput is null &&
                !Color255to1RangeConverter.IsContextful(node))
            {
                documentViewModel.Operations.SetNodeInputPropertyValue(node.Id,
                    MathNode.ModePropertyName, MathNodeMode.GreaterThan);
            }
        }
    }
}
