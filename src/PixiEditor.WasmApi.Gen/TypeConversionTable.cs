using System;
using Microsoft.CodeAnalysis;

namespace PixiEditor.Api.Gen;

public static class TypeConversionTable
{
    public static string[] ConvertTypeToFunctionParams(IParameterSymbol symbol)
    {
        if(IsLengthType(symbol))
        {
            return [$"int {symbol.Name}Pointer", $"int {symbol.Name}Length"];
        }

        return [$"int {symbol.Name}Pointer"];
    }

    public static bool IsLengthType(IParameterSymbol symbol)
    {
        return symbol.Type.Name.Equals("string", StringComparison.OrdinalIgnoreCase)
               || symbol.Type.Name.Equals("byte[]", StringComparison.OrdinalIgnoreCase)
               || symbol.Type.Name.Equals("span", StringComparison.OrdinalIgnoreCase);
    }
}
