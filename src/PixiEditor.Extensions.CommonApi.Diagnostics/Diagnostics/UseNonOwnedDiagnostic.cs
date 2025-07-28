using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace PixiEditor.Extensions.CommonApi.Diagnostics.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class UseNonOwnedDiagnostic : DiagnosticAnalyzer
{
    public const string DiagnosticId = "UseNonOwned";

    public const string Title = "Use .NonOwned() method";
    
    public static readonly DiagnosticDescriptor Descriptor = new(DiagnosticId, "Use .NonOwned() method",
        "Use {0}.NonOwned{1}() to declare a Setting using the property name that is owned by another extension", DiagnosticConstants.Category,
        DiagnosticSeverity.Info, true);
    
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        [Descriptor];
    
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeDeclaration, SyntaxKind.PropertyDeclaration);
    }

    private static void AnalyzeDeclaration(SyntaxNodeAnalysisContext context)
    {
        var semantics = context.SemanticModel;
        var declaration = (PropertyDeclarationSyntax)context.Node;

        var typeInfo = ModelExtensions.GetTypeInfo(semantics, declaration.Type, context.CancellationToken);

        if (!DiagnosticHelpers.IsSettingType(typeInfo))
        {
            return;
        }
        
        if (declaration.Initializer is not { Value: BaseObjectCreationExpressionSyntax { ArgumentList.Arguments: { Count: > 0 } arguments } initializerExpression } ||
            DoesNotMatch(semantics, arguments, declaration))
        {
            return;
        }
        
        var diagnostic = GetDiagnostic(arguments, declaration, semantics, initializerExpression, typeInfo);

        context.ReportDiagnostic(diagnostic);
    }

    private static Diagnostic GetDiagnostic(SeparatedSyntaxList<ArgumentSyntax> arguments, PropertyDeclarationSyntax declaration,
        SemanticModel semantics, BaseObjectCreationExpressionSyntax initializerExpression, TypeInfo typeInfo)
    {
        var genericType = string.Empty;

        var fallbackValueArgument = arguments.Skip(1).FirstOrDefault();

        var settingType = ((GenericNameSyntax)declaration.Type).TypeArgumentList.Arguments.First();
        if (fallbackValueArgument == null || !SymbolEqualityComparer.Default.Equals(
                ModelExtensions.GetTypeInfo(semantics, fallbackValueArgument.Expression).Type,
                ModelExtensions.GetTypeInfo(semantics, settingType).Type))
        {
            genericType = $"<{settingType}>";
        }

        var diagnostic = Diagnostic.Create(Descriptor, initializerExpression.GetLocation(),
            typeInfo.Type?.Name, // LocalSetting or Synced Setting
            genericType);
        return diagnostic;
    }

    private static bool DoesNotMatch(SemanticModel semantics, SeparatedSyntaxList<ArgumentSyntax> arguments,
        PropertyDeclarationSyntax declaration)
    {
        var nameArgument = arguments.First();

        var operation = semantics.GetOperation(nameArgument.Expression);

        if (operation?.ConstantValue.Value is not string constant || DiagnosticHelpers.GetKey(constant) is not { } key)
        {
            return true;
        }
        
        var declarationName = declaration.Identifier.ValueText;
        
        return key != declarationName;
    }
}
