using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.CombineSeparate;

namespace PixiEditor.Helpers.Extensions;

internal static class CombineSeparateColorModeExtensions
{
    public static (string v1, string v2, string v3) GetLocalizedColorStringNames(this CombineSeparateColorMode mode) => mode switch
    {
        CombineSeparateColorMode.RGB => ("R", "G", "B"),
        CombineSeparateColorMode.HSV => ("H", "S", "V"),
        CombineSeparateColorMode.HSL => ("H", "S", "L"),
    };
}
