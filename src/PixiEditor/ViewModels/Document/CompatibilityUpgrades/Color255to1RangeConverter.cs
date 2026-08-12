using Drawie.Backend.Core.Shaders.Generation.Expressions;
using PixiEditor.ChangeableDocument.Changeables.Graph;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.CombineSeparate;
using PixiEditor.ChangeableDocument.Enums;
using PixiEditor.UI.Common.Localization;
using PixiEditor.ViewModels.Document.Nodes;
using PixiEditor.ViewModels.Document.Nodes.CombineSeparate;
using PixiEditor.ViewModels.Nodes;

namespace PixiEditor.ViewModels.Document.CompatibilityUpgrades;

internal class Color255to1RangeConverter : IGraphUpgrader
{
    public event Action? UpgradeCompleted;
    public DocumentViewModel DocumentViewModel { get; }
    public LocalizedString UpgradeText { get; } = new LocalizedString("UPGRADE_COLOR_255_TO_1_RANGE");


    public Color255to1RangeConverter(DocumentViewModel documentViewModel)
    {
        DocumentViewModel = documentViewModel;
    }

    public void Upgrade()
    {
        using var block = DocumentViewModel.Operations.StartChangeBlock();
        var separateNodes = DocumentViewModel.NodeGraph.AllNodes.OfType<SeparateColorNodeViewModel>();
        foreach (var node in separateNodes)
        {
            if (!IsContextful(node))
            {
                DocumentViewModel.Operations.SetNodeInputPropertyValue(node.Id,
                    SeparateColorNode.NormalizedValuesPropertyName, true);
                if (node.FindInputProperty(SeparateColorNode.ModePropertyName).Value is CombineSeparateColorMode mode)
                {
                    ConvertOutput(node.FindOutputProperty(SeparateColorNode.V1PropertyName), mode);
                    ConvertOutput(node.FindOutputProperty(SeparateColorNode.V2PropertyName), mode);
                    ConvertOutput(node.FindOutputProperty(SeparateColorNode.V3PropertyName), mode);
                    ConvertOutput(node.FindOutputProperty(SeparateColorNode.APropertyName), mode);
                }
            }
        }

        var combineNodes = DocumentViewModel.NodeGraph.AllNodes.OfType<CombineColorNodeViewModel>();

        foreach (var node in combineNodes)
        {
            if (!IsContextful(node))
            {
                DocumentViewModel.Operations.SetNodeInputPropertyValue(node.Id,
                    CombineColorNode.NormalizedValuesPropertyName, true);
                if (node.FindInputProperty(CombineColorNode.ModePropertyName).Value is CombineSeparateColorMode mode)
                {
                    ConvertValue(node.FindInputProperty(CombineColorNode.V1PropertyName), mode);
                    ConvertValue(node.FindInputProperty(CombineColorNode.V2PropertyName), mode);
                    ConvertValue(node.FindInputProperty(CombineColorNode.V3PropertyName), mode);
                    ConvertValue(node.FindInputProperty(CombineColorNode.APropertyName), mode);
                }
            }
        }

        UpgradeCompleted?.Invoke();
    }

    public void PerformImmediateCompatibilityConversion()
    {
        var documentViewModel = DocumentViewModel;
        var separateNodes = documentViewModel.NodeGraph.AllNodes.OfType<SeparateColorNodeViewModel>();
        foreach (var node in separateNodes)
        {
            if (!IsContextful(node))
            {
                documentViewModel.Operations.SetNodeInputPropertyValue(node.Id,
                    SeparateColorNode.NormalizedValuesPropertyName, false);
            }
            else
            {
                if (node.FindInputProperty(SeparateColorNode.ModePropertyName).Value is CombineSeparateColorMode mode)
                {
                    ConvertValue(node.FindOutputProperty(SeparateColorNode.V1PropertyName), mode);
                    ConvertValue(node.FindOutputProperty(SeparateColorNode.V2PropertyName), mode);
                    ConvertValue(node.FindOutputProperty(SeparateColorNode.V3PropertyName), mode);
                    ConvertValue(node.FindOutputProperty(SeparateColorNode.APropertyName), mode);
                }
            }
        }

        var combineNodes = documentViewModel.NodeGraph.AllNodes.OfType<CombineColorNodeViewModel>();

        foreach (var node in combineNodes)
        {
            if (!IsContextful(node))
            {
                documentViewModel.Operations.SetNodeInputPropertyValue(node.Id,
                    CombineColorNode.NormalizedValuesPropertyName, false);
            }
            else
            {
                if (node.FindInputProperty(CombineColorNode.ModePropertyName).Value is CombineSeparateColorMode mode)
                {
                    ConvertValue(node.FindInputProperty(CombineColorNode.V1PropertyName), mode);
                    ConvertValue(node.FindInputProperty(CombineColorNode.V2PropertyName), mode);
                    ConvertValue(node.FindInputProperty(CombineColorNode.V3PropertyName), mode);
                    ConvertValue(node.FindInputProperty(CombineColorNode.APropertyName), mode);
                }
            }
        }
    }

    private void ConvertValue(NodePropertyViewModel prop, CombineSeparateColorMode mode)
    {
        if (prop.Value is double value)
        {
            DocumentViewModel.Operations.SetNodeInputPropertyValue(prop.Node.Id, prop.PropertyName,
                Round(value / GetValueToDivideByMode(mode, prop.PropertyName)));
        }
    }

    private static bool IsContextful(NodeViewModel node)
    {
        bool isContextful = false;
        node.TraverseForwards(prop =>
        {
            if (prop is ModifyImageRightNodeViewModel)
            {
                isContextful = true;
                return Traverse.Exit;
            }

            return Traverse.Further;
        });

        return isContextful;
    }

    private void ConvertOutput(NodePropertyViewModel? output, CombineSeparateColorMode combineSeparateColorMode)
    {
        if (output == null)
            return;

        foreach (var prop in output.ConnectedInputs)
        {
            if (prop.Node is MathNodeViewModel mathNode)
            {
                if (mathNode.FindInputProperty(MathNode.ModePropertyName).Value is MathNodeMode mode)
                {
                    if (mode == MathNodeMode.Divide)
                    {
                        if (prop.PropertyName == MathNode.XPropertyName)
                        {
                            var otherProp = mathNode.FindInputProperty(MathNode.YPropertyName);
                            if (otherProp.Value is double otherValue)
                            {
                                DocumentViewModel.Operations.SetNodeInputPropertyValue(mathNode.Id,
                                    MathNode.YPropertyName,
                                    Round(otherValue /
                                          GetValueToDivideByMode(combineSeparateColorMode, output.PropertyName)));
                            }
                        }
                    }
                }
            }
        }
    }

    private static double GetValueToDivideByMode(CombineSeparateColorMode mode, string propPropertyName)
    {
        if (mode == CombineSeparateColorMode.RGB || propPropertyName == "A") return 255.0;

        if (mode is CombineSeparateColorMode.HSV or CombineSeparateColorMode.HSL)
        {
            return propPropertyName == "R" ? 360.0 : 100.0;
        }

        return 0.0;
    }

    private static double Round(double value)
    {
        return Math.Round(value, 2);
    }
}
