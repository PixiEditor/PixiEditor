using System.Reflection;

string assemblyPath = args[0];
string outputPath = args[1];

Console.WriteLine($"Building layouts from path: {Path.GetFullPath(assemblyPath)} to {Path.GetFullPath(outputPath)}");

Assembly assembly = Assembly.LoadFrom(assemblyPath);
var exportedTypes = assembly.GetExportedTypes();

exportedTypes.Where(x => IsLayoutElement(x)).ToList().ForEach(x =>
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
    object instance = Activator.CreateInstance(type);
    MethodInfo buildNativeMethod = type.GetMethod("BuildNative");
    var compiled = buildNativeMethod.Invoke(instance, null);
    object bytesArray = compiled.GetType().GetMethod("SerializeBytes").Invoke(compiled, null);
    byte[] bytes = (byte[])bytesArray;
    return bytes;
}

bool IsLayoutElement(Type type)
{
    if(type.BaseType == null) return false;
    if(type.BaseType.Name.Contains("LayoutElement")) return true;

    return IsLayoutElement(type.BaseType);
}
