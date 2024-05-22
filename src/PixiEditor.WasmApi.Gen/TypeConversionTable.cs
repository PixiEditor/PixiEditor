using Microsoft.CodeAnalysis;

namespace PixiEditor.WasmApi.Gen;

public static class TypeConversionTable
{
    public static string[] ConvertTypeToFunctionParams(IParameterSymbol symbol)
    {
        if(IsValuePassableType(symbol.Type, out string? typeName))
        {
            return [$"{typeName} {symbol.Name}"];
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

    public static bool IsValuePassableType(ITypeSymbol argSymbol, out string? typeName)
    {
        if (argSymbol.Name.Equals("int32", StringComparison.OrdinalIgnoreCase))
        {
            typeName = "int";
            return true;
        }

        if (argSymbol.Name.Equals("double", StringComparison.OrdinalIgnoreCase))
        {
            typeName = "double";
            return true;
        }

        typeName = null;
        return false;
    }
    
    public static bool IsValuePassableReturnType(ITypeSymbol argSymbol, out string? typeName)
    {
        if (argSymbol.Name.Equals("int32", StringComparison.OrdinalIgnoreCase))
        {
            typeName = "int";
            return true;
        }

        if (argSymbol.Name.Equals("double", StringComparison.OrdinalIgnoreCase))
        {
            typeName = "double";
            return true;
        }
        
        /*if (argSymbol.Name.Equals("float", StringComparison.OrdinalIgnoreCase))
        {
            typeName = "float";
            return true;
        }
        
        if (argSymbol.Name.Equals("string", StringComparison.OrdinalIgnoreCase))
        {
            typeName = "string";
            return true;
        }*/

        typeName = null;
        return false;
    }
}
