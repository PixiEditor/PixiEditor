using Drawie.Backend.Core.ColorsImpl;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.CombineSeparate;
using PixiEditor.ChangeableDocument.Enums;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.Models.Serialization.Factories;
using PixiEditor.ViewModels.Document.CompatibilityUpgrades;
using PixiEditor.ViewModels.Document.Nodes.CombineSeparate;
using PixiEditor.ViewModels.Nodes;

namespace PixiEditor.ViewModels.Document;

internal static class CompatibilityUtility
{
    private static HashSet<string> knownColorInputsToConvertToHalf4 =
    [
        $"PixiEditor.{ColorNode.UniqueName}:" + ColorNode.InputColorPropertyName,
        $"PixiEditor.{LerpColorNode.UniqueName}:" + LerpColorNode.FromPropertyName,
        $"PixiEditor.{LerpColorNode.UniqueName}:" + LerpColorNode.ToPropertyName,
        $"PixiEditor.{SeparateColorNode.UniqueName}:" + SeparateColorNode.ColorPropertyName,
        $"PixiEditor.{ModifyImageRightNode.UniqueName}:" + ModifyImageRightNode.ColorPropertyName
    ];


    public static IGraphUpgrader[] CalculateGraphUpgraders(DocumentViewModel viewModel, Version? serializerVersion,
        List<SerializationFactory> allFactories, (string serializerName, string serializerVersion) serializerData)
    {
        List<IGraphUpgrader> upgraders = new List<IGraphUpgrader>();

        if (serializerVersion is { Major: 2, Minor: 1 } && serializerVersion < new Version(2, 1, 2, 0) &&
            (HasNode<SeparateColorNodeViewModel>(viewModel) || HasNode<CombineColorNodeViewModel>(viewModel)))
        {
            UpgradeNormalizedColorValues(viewModel, allFactories, serializerData);
            var upgrader = new Color255to1RangeConverter(viewModel);
            upgraders.Add(upgrader);
        }

        return upgraders.ToArray();
    }

    private static bool HasNode<T>(DocumentViewModel viewModel) where T : NodeViewModel
    {
        return viewModel.NodeGraph.AllNodes.Any(n => n is T);
    }

    public static object UpgradeInputValueToCurrentVersion(object oldObject, Version? serializedVersion,
        string nodeUniqueName, string inputProperty, Dictionary<string, object> allValues)
    {
        if (serializedVersion is null)
        {
            return oldObject;
        }

        if (serializedVersion < new Version(2, 1, 2, 0))
        {
            if (nodeUniqueName == $"PixiEditor.{MergeNode.UniqueName}")
            {
                return UpgradeMergeNode(inputProperty, oldObject, serializedVersion, allValues);
            }

            return UpgradeColorInputs(oldObject, $"{nodeUniqueName}:{inputProperty}");
        }

        return oldObject;
    }

    private static object UpgradeColorInputs(object oldObject, string inputProperty)
    {
        if (oldObject is Color color && knownColorInputsToConvertToHalf4.Contains(inputProperty))
        {
            return new Vec4D(color.R / 255.0, color.G / 255.0, color.B / 255.0, color.A / 255.0);
        }

        return oldObject;
    }

    private static object UpgradeMergeNode(string inputProperty, object oldObject,
        Version serializedVersion, Dictionary<string, object> allValues)
    {
        if (serializedVersion < new Version(2, 1, 2, 0))
        {
            if (inputProperty == MergeNode.BlendModePropertyName)
            {
                if (oldObject is int blendModeInt && Enum.IsDefined(typeof(BlendMode), blendModeInt))
                {
                    return RenderContext.GetDrawingBlendMode((BlendMode)blendModeInt);
                }
            }
        }

        return oldObject;
    }

    public static void UpgradeNodeAdditionalDataToCurrentVersion(Dictionary<string, object> additionalData,
        Version? parsedVersion, string serializedNodeUniqueNodeName,
        Dictionary<string, object> serializedNodeInputValues)
    {
    }

    private static void UpgradeNormalizedColorValues(DocumentViewModel viewModel,
        List<SerializationFactory> allFactories, (string serializerName, string serializerVersion) serializerData)
    {
        new Color255to1RangeConverter(viewModel).PerformImmediateCompatibilityConversion();
    }
}
