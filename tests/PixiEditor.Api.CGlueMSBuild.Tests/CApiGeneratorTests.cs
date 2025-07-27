using Mono.Cecil;

namespace PixiEditor.Api.CGlueMSBuild.Tests;

public class CApiGeneratorTests
{
    [Fact]
    public void TestThatLoadAssemblies()
    {
        CApiGenerator apiGenerator = new CApiGenerator("", "", "", (message) => { });
        AssemblyDefinition assembly = AssemblyDefinition.ReadAssembly("TestAssets/CGlueTestLib.dll");
        var assemblies = apiGenerator.LoadAssemblies(assembly, "TestAssets");

        Assert.Equal(2, assemblies.Count);
    }

    [Fact]
    public void TestThatImportedMethodsAreExtractedCorrectly()
    {
        CApiGenerator apiGenerator = new CApiGenerator("", "", "", (message) => { });
        AssemblyDefinition assembly = AssemblyDefinition.ReadAssembly("TestAssets/CGlueTestLib.dll");
        var assemblies = apiGenerator.LoadAssemblies(assembly, "TestAssets");

        var importedMethods = CApiGenerator.GetImportedMethods(assemblies.SelectMany(a => a.MainModule.Types).ToArray());

        Assert.True(importedMethods.Length > 0);
    }

    [Fact]
    public void TestThatGenerateImportsGeneratesCorrectImports()
    {
        CApiGenerator apiGenerator = new CApiGenerator("", "", "", (message) => { });
        AssemblyDefinition assembly = AssemblyDefinition.ReadAssembly("TestAssets/CGlueTestLib.dll");
        var assemblies = apiGenerator.LoadAssemblies(assembly, "TestAssets");

        var importedMethods = CApiGenerator.GetImportedMethods(assemblies.SelectMany(a => a.MainModule.Types).ToArray());
        string imports = apiGenerator.GenerateImports(importedMethods);

        string sanitizedImports = imports.Replace("\n", "").Replace("\r", "");

        Assert.Contains("__attribute__((import_name(\"subscribe_to_event\")))", sanitizedImports);
        Assert.Contains("void subscribe_to_event(int32_t internalControlId, char* eventName, int32_t eventNameLength);", sanitizedImports);
    }
    
    [Fact]
    public void TestThatGenerateImportsForStringReturnTypeGeneratesConversionCorrectly()
    {
        CApiGenerator apiGenerator = new CApiGenerator("", "", "", (message) => { });
        AssemblyDefinition assembly = AssemblyDefinition.ReadAssembly("TestAssets/CGlueTestLib.dll");
        var assemblies = apiGenerator.LoadAssemblies(assembly, "TestAssets");

        var importedMethods = CApiGenerator.GetImportedMethods(assemblies.SelectMany(a => a.MainModule.Types).ToArray());
        string imports = apiGenerator.GenerateImports([importedMethods.First(x => x.Name == "string_return_method")]);

        string sanitizedImports = imports.Replace("\n", "").Replace("\r", "");

        Assert.Contains("__attribute__((import_name(\"string_return_method\")))", sanitizedImports);
        Assert.Contains("char* string_return_method();", sanitizedImports);
        Assert.Contains("MonoString* internal_string_return_method(){", sanitizedImports);
        Assert.Contains("char* result = string_return_method();", sanitizedImports);
        Assert.Contains("MonoString* mono_result = mono_string_new(mono_domain_get(), result)", sanitizedImports);
        Assert.Contains("return mono_result;", sanitizedImports);
    }

    [Fact]
    public void TestThatGenerateExportsGeneratesCorrectExports()
    {
        CApiGenerator apiGenerator = new CApiGenerator("", "", "", (message) => { });
        AssemblyDefinition assembly = AssemblyDefinition.ReadAssembly("TestAssets/CGlueTestLib.dll");
        var assemblies = apiGenerator.LoadAssemblies(assembly, "TestAssets");

        var exportedMethods = CApiGenerator.GetExportedMethods(assemblies.SelectMany(a => a.MainModule.Types).ToArray());
        string exports = apiGenerator.GenerateExports(exportedMethods);

        string sanitizedExports = exports.Replace("\n", "").Replace("\r", "");

        Assert.Contains("__attribute__((export_name(\"raise_element_event\")))", sanitizedExports);
        Assert.Contains("void raise_element_event(int32_t internalControlId, char* eventName)", sanitizedExports);
        Assert.Contains("MonoMethod* method = lookup_interop_method(\"EventRaised\");", sanitizedExports);
        Assert.Contains("MonoString* mono_eventName = mono_string_new(mono_domain_get(), eventName);", sanitizedExports);
        Assert.Contains("void* args[] = {&internalControlId, mono_eventName};", sanitizedExports);
        Assert.Contains("invoke_interop_method(method, args);", sanitizedExports);
        Assert.Contains("free(method);", sanitizedExports);
    }

    [Fact]
    public void TestThatAttachImportFunctionsGenerateProperly()
    {
        CApiGenerator apiGenerator = new CApiGenerator("", "", "", (message) => { });
        AssemblyDefinition assembly = AssemblyDefinition.ReadAssembly("TestAssets/CGlueTestLib.dll");
        var assemblies = apiGenerator.LoadAssemblies(assembly, "TestAssets");

        var importedMethods = CApiGenerator.GetImportedMethods(assemblies.SelectMany(a => a.MainModule.Types).ToArray());
        string attachCode = apiGenerator.GenerateAttachImportedFunctions(importedMethods);

        string sanitizedImports = attachCode.Replace("\n", "").Replace("\r", "");

        Assert.Contains("void attach_imported_functions()", sanitizedImports);
        Assert.Contains("mono_add_internal_call(\"PixiEditor.Extensions.Sdk.Bridge.Native::subscribe_to_event\", internal_subscribe_to_event);", sanitizedImports);
    }
}
