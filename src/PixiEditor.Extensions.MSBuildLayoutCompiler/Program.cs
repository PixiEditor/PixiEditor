using System.Reflection;

string assemblyPath = args[0];
string outputPath = args[1];

Console.WriteLine($"Building layouts from path: {Path.GetFullPath(assemblyPath)} to {Path.GetFullPath(outputPath)}");

Assembly assembly = Assembly.LoadFrom(assemblyPath);
var exportedTypes = assembly.GetExportedTypes();

exportedTypes.Where(IsLayoutElement).ToList().ForEach(x =>
{
    string path = Path.Combine(outputPath, x.Name + ".layout");
    if(Directory.Exists(outputPath) == false)
    {
        Directory.CreateDirectory(outputPath);
    }

    File.WriteAllBytes(path, GenerateLayoutFile(x));
});


byte[] GenerateLayoutFile(Type type)
{
    Console.WriteLine($"Generating layout for {type.Name}");
    object instance = Activator.CreateInstance(type, TryGetConstructorArgs(type));
    MethodInfo buildNativeMethod = type.GetMethod("BuildNative");
    var compiled = buildNativeMethod.Invoke(instance, null);
    object bytesArray = compiled.GetType().GetMethod("SerializeBytes").Invoke(compiled, null);
    byte[] bytes = (byte[])bytesArray;
    return bytes;
}

bool IsLayoutElement(Type type)
{
    if(type.BaseType == null) return false;
    if(type.BaseType.Name.Contains("StatefulElement") || type.BaseType.Name.Contains("StatelessElement")) return true;

    return IsLayoutElement(type.BaseType);
}

object?[] TryGetConstructorArgs(Type handler)
{
    ConstructorInfo[] constructors = handler.GetConstructors();
    if (constructors.Length == 0)
    {
        return Array.Empty<object>();
    }

    ConstructorInfo constructor = constructors[0];
    ParameterInfo[] parameters = constructor.GetParameters();
    if (parameters.Length == 0)
    {
        return Array.Empty<object>();
    }

    return parameters.Select(x => x.ParameterType.IsValueType ? Activator.CreateInstance(x.ParameterType) : null).ToArray();
}
