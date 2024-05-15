using Microsoft.CodeAnalysis;

namespace PixiEditor.WasmApi.Gen;

public static class TypeConversionTable
{
    public static string[] ConvertTypeToFunctionParams(IParameterSymbol symbol)
    {
        if(IsIntType(symbol.Type))
        {
            return [$"int {symbol.Name}"];
        }

        if(IsLengthType(symbol))
        {
            return[$"int {symbol.Name}Pointer", $"int {symbol.Name}Length"];
        }

        return[$"int {symbol.Name}Pointer"];
    }

    public static bool IsLengthType(IParameterSymbol symbol)
    {
        return symbol.Type.Name.Equals("string", StringComparison.OrdinalIgnoreCase)
               || symbol.Type.Name.Equals("byte[]", StringComparison.OrdinalIgnoreCase)
               || symbol.Type.Name.Equals("span", StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsIntType(ITypeSymbol argSymbol)
    {
        return argSymbol.Name.Equals("int32", StringComparison.OrdinalIgnoreCase);
    }
}
