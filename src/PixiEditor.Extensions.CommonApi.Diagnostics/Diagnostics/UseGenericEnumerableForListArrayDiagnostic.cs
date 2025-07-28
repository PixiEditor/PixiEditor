using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace PixiEditor.Extensions.CommonApi.Diagnostics.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class UseGenericEnumerableForListArrayDiagnostic : DiagnosticAnalyzer
{
    private const string ListNamespace = "System.Collections.Generic";
    private const string ListName = "List";
    
    public const string DiagnosticId = "UseGenericEnumerableForListArray";
    
    public static DiagnosticDescriptor UseGenericEnumerableForListArrayDescriptor { get; } =
        new(DiagnosticId, "Use IEnumerable<T> in Setting instead of List/Array",
            "Use IEnumerable<{0}> instead of {1} to allow passing any IEnumerable<{0}> for the value. Use the {2} extension from PixiEditor.Extensions.CommonApi.UserPreferences.Settings to access the Setting as a {1}.",
            "PixiEditor.CommonAPI", DiagnosticSeverity.Info, true);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.GenericName);
    }

    private void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        var semanticModel = context.SemanticModel;
        
        var name = (GenericNameSyntax)context.Node;
        var typeInfo = semanticModel.GetTypeInfo(name, context.CancellationToken);

        if (!DiagnosticHelpers.IsSettingType(typeInfo))
        {
            return;
        }
        
        var typeArgument = name.TypeArgumentList.Arguments.First();

        var isArrayOrList = GetInfo(context.SemanticModel, typeArgument, out var targetTypeName, out var extensionMethod, context.CancellationToken);
        
        if (!isArrayOrList)
        {
            return;
        }

        var diagnostic = Diagnostic.Create(
            UseGenericEnumerableForListArrayDescriptor,
            name.GetLocation(),
            targetTypeName,
            typeArgument.ToString(),
            extensionMethod
        );
        
        context.ReportDiagnostic(diagnostic);
    }

    private static bool GetInfo(SemanticModel semanticModel, TypeSyntax typeArgument, out string? targetTypeName, out string? extensionMethod, CancellationToken cancellationToken = default)
    {
        bool isArrayOrList = false;
        targetTypeName = null;
        extensionMethod = null;
        
        if (typeArgument is ArrayTypeSyntax array)
        {
            isArrayOrList = true;
            targetTypeName = array.ElementType.ToString();
            extensionMethod = ".AsArray()";
        }
        else if (typeArgument is GenericNameSyntax genericName)
        {
            var argumentSymbol = semanticModel.GetTypeInfo(typeArgument, cancellationToken);

            if (argumentSymbol.Type?.ContainingNamespace.ToString() != ListNamespace ||
                argumentSymbol.Type?.Name != ListName)
            {
                return isArrayOrList;
            }

            extensionMethod = ".AsList()";
            isArrayOrList = true;
            targetTypeName = genericName.TypeArgumentList.Arguments.First().ToString();
        }

        return isArrayOrList;
    }

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        [UseGenericEnumerableForListArrayDescriptor];
}
