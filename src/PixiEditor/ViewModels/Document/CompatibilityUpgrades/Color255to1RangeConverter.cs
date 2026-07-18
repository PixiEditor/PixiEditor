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
        var separateNodes = DocumentViewModel.NodeGraph.AllNodes.OfType<SeparateColorNodeViewModel>();
        foreach (var node in separateNodes)
        {
            if (!IsContextful(node))
            {
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

    private void ConvertValue(NodePropertyViewModel prop, CombineSeparateColorMode mode)
    {
        if (prop.Value is double value)
        {
            prop.Value = value / GetValueToDivideByMode(prop.PropertyName, mode);
        }
    }

    private bool IsContextful(NodeViewModel node)
    {
        bool isContextful = false;
        node.TraverseForwards(prop =>
        {
            if (prop is ModifyImageRightNodeViewModel mathNode)
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
                MathNodeMode? mode = mathNode.FindInputProperty(MathNode.ModePropertyName).Value as MathNodeMode?;
                if (mode.HasValue)
                {
                    if (mode.Value == MathNodeMode.Divide)
                    {
                        if (prop.PropertyName == MathNode.XPropertyName)
                        {
                            var otherProp = mathNode.FindInputProperty(MathNode.YPropertyName);
                            if (otherProp.Value is double otherValue)
                            {
                                otherProp.Value = otherValue / GetValueToDivideByMode(output.PropertyName, combineSeparateColorMode);
                            }
                        }
                    }
                }
            }
        }
    }

    private static double GetValueToDivideByMode(string inputPropertyName, CombineSeparateColorMode mode)
    {
        return mode switch
        {
            CombineSeparateColorMode.RGB => 255.0,
            CombineSeparateColorMode.HSV => 360.0,
            CombineSeparateColorMode.HSL => 360.0,
            _ => 1
        };
    }
}
