using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.CombineSeparate;

namespace PixiEditor.ViewModels.Document;

public static class CompatibilityUtility
{
    public static object UpgradeInputValueToCurrentVersion(object oldObject, Version serializedVersion,
        string nodeUniqueName, string inputProperty, Dictionary<string, object> allValues)
    {
        if (nodeUniqueName == $"PixiEditor.{CombineColorNode.UniqueName}")
        {
            return UpgradeCombineColorNode(inputProperty, oldObject, serializedVersion, allValues);
        }

        return oldObject;
    }

    private static object UpgradeCombineColorNode(string inputProperty, object oldObject,
        Version serializedVersion, Dictionary<string, object> allValues)
    {
        if (serializedVersion is { Major: 2, Minor: 0 })
        {
            if (allValues.TryGetValue(CombineColorNode.ModePropertyName, out var modeRaw) &&
                modeRaw is int modeInt && Enum.IsDefined(typeof(CombineSeparateColorMode), modeInt) &&
                (CombineSeparateColorMode)modeInt is var mode)
            {
                if (inputProperty == CombineColorNode.V1PropertyName)
                {
                    if (oldObject is double f)
                    {
                        return mode switch
                        {
                            CombineSeparateColorMode.RGB => f * 255f,
                            CombineSeparateColorMode.HSV => f * 360f,
                            CombineSeparateColorMode.HSL => f * 360f,
                        };
                    }
                }
                else if (inputProperty == CombineColorNode.V2PropertyName || inputProperty == CombineColorNode.V3PropertyName)
                {
                    if (oldObject is double f)
                    {
                        return mode switch
                        {
                            CombineSeparateColorMode.RGB => f * 255f,
                            CombineSeparateColorMode.HSV => f * 100,
                            CombineSeparateColorMode.HSL => f * 100,
                        };
                    }
                }
                else if (inputProperty == CombineColorNode.APropertyName)
                {
                    if (oldObject is double f)
                    {
                        return f * 255f;
                    }
                }
            }
        }

        return oldObject;
    }
}
