using System.Diagnostics;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace PixiEditorGen;

[Generator(LanguageNames.CSharp)]
public class MainVmEnumGenerator : IIncrementalGenerator
{
    const string MainVmName = "PixiEditor.ViewModels.ViewModelMain";
    private const string SubVmName = "PixiEditor.ViewModels.SubViewModels.SubViewModel`1";
    
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var properties = GetProperties(context);
        
        context.RegisterSourceOutput(properties.Collect(), (context, list) =>
        {
            AddEnum(context, list.SelectMany(x => x).ToList());
        });
    }

    private void AddEnum(SourceProductionContext context, List<PropertyDeclarationSyntax> properties)
    {
        var enumDeclaration = SyntaxFactory.EnumDeclaration("MainVmEnum")
            .AddMembers(properties.Select(property => property.Identifier.ValueText.Replace("SubViewModel", "SVM").Replace("ViewModel", "VM")).Select(SyntaxFactory.EnumMemberDeclaration).ToArray());
        
        var namespaceDeclaration = SyntaxFactory
            .NamespaceDeclaration(SyntaxFactory.ParseName("PixiEditor.ViewModels"))
            .AddMembers(enumDeclaration);
        
        context.AddSource("MainVmEnum", namespaceDeclaration.NormalizeWhitespace().ToFullString());
    }

    private IncrementalValuesProvider<List<PropertyDeclarationSyntax>> GetProperties(IncrementalGeneratorInitializationContext context) => context.SyntaxProvider.CreateSyntaxProvider(
        (x, _) =>
        {
            return x is TypeDeclarationSyntax type && type.Identifier.ValueText.Contains("ViewModelMain");
        }, 
        (context, _) =>
        {
            var type = context.Node as TypeDeclarationSyntax;
            var semantic = context.SemanticModel;
            
            var properties = type.Members
                .Where(member => member is PropertyDeclarationSyntax)
                .Cast<PropertyDeclarationSyntax>();

            var subViewModelType = semantic.Compilation.GetTypeByMetadataName(SubVmName);

            var matching = from property in properties
                let symbol = ModelExtensions.GetSymbolInfo(semantic, property.Type).Symbol
                where ((ITypeSymbol)symbol!).BaseType.AssignableTo(subViewModelType!)
                select property;
            
            return matching.ToList();
        });
}
