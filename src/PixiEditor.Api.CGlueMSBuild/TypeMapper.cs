using Mono.Cecil;

namespace PixiEditor.Api.CGlueMSBuild;

public static class TypeMapper
{
    private static Dictionary<string, string> cTypeMap = new Dictionary<string, string>
    {
        { "void", "void" },
        { "boolean", "bool" },
        { "byte", "uint8_t" },
        { "sbyte", "int8_t" },
        { "int16", "int16_t" },
        { "uint16", "uint16_t" },
        { "int32", "int32_t" },
        { "uint32", "uint32_t" },
        { "int64", "int64_t" },
        { "uint64", "uint64_t" },
        { "single", "float" },
        { "double", "double" },
        { "char", "char" },
        { "string", "char*" },
        { "intptr", "uint8_t*" } // byte array
    };

    private static Dictionary<string, string> monoTypeMap = new Dictionary<string, string>
    {
        { "void", "void" },
        { "boolean", "bool" },
        { "byte", "uint8_t" },
        { "sbyte", "int8_t" },
        { "int16", "int16_t" },
        { "uint16", "uint16_t" },
        { "int32", "int32_t" },
        { "uint32", "uint32_t" },
        { "int64", "int64_t" },
        { "uint64", "uint64_t" },
        { "single", "float" },
        { "double", "double" },
        { "char", "char" },
        { "string", "MonoString*" },
        { "intptr", "uint8_t*" } // byte array
    };

    private static Dictionary<string, string[]> monoToCConversionMap = new Dictionary<string, string[]>
    {
        { "MonoString*", ["mono_string_to_utf8({0})", "strlen({0})"] },
    };

    private static Dictionary<string, string[]> cToMonoConversionMap = new Dictionary<string, string[]>
    {
        { "char*", ["mono_string_new(mono_domain_get(), {0})"] }
    };

    public static string MapToCType(TypeReference type)
    {
        if(cTypeMap.TryGetValue(type.Name.ToLower(), out var mapType))
        {
            return mapType;
        }

        throw new NotSupportedException($"Type {type.FullName} is not supported.");
    }

    public static string MapToMonoType(TypeReference type)
    {
        if(monoTypeMap.TryGetValue(type.Name.ToLower(), out var mapType))
        {
            return mapType;
        }

        throw new NotSupportedException($"Type {type.FullName} is not supported.");
    }

    public static string[] MapToCTypeParam(TypeReference type, string name, bool extractLength = true)
    {
        if (extractLength && IsLengthType(type))
        {
            return [$"{MapToCType(type)} {name}", $"int32_t {name}Length"];
        }

        return [$"{MapToCType(type)} {name}"];
    }

    public static string MapToMonoTypeParam(TypeReference parameterParameterType, string parameterName)
    {
        return $"{MapToMonoType(parameterParameterType)} {parameterName}";
    }

    private static bool IsLengthType(TypeReference type)
    {
        return type.Name.Equals("string", StringComparison.InvariantCultureIgnoreCase) || type.IsArray;
    }

    public static bool RequiresConversion(TypeReference parameterParameterType)
    {
        return IsLengthType(parameterParameterType);
    }

    public static ConvertedParam[] ConvertMonoToCType(TypeReference parameterParameterType, string inputVarName, string outputVarName)
    {
        if (IsLengthType(parameterParameterType))
        {
            string monoType = MapToMonoType(parameterParameterType);
            string[] converted = monoToCConversionMap[monoType];
            return
            [
                new ConvertedParam($"{outputVarName}_data", MapToCType(parameterParameterType), ToConversion(converted[0], inputVarName)),
                new ConvertedParam($"{outputVarName}_length", "int32_t", ToConversion(converted[1], $"{outputVarName}_data"))
            ];
        }

        return [];
    }

    public static ConvertedParam[] ConvertCToMonoType(TypeReference parameterParameterType, string inputVarName, string outputVarName)
    {
        if (IsLengthType(parameterParameterType))
        {
            string cType = MapToCType(parameterParameterType);
            string[] converted = cToMonoConversionMap[cType];
            return
            [
                new ConvertedParam(outputVarName, MapToMonoType(parameterParameterType), ToConversion(converted[0], inputVarName))
            ];
        }

        return [];
    }

    private static string ToConversion(string method, string varName)
    {
        return string.Format(method, varName);
    }
}
