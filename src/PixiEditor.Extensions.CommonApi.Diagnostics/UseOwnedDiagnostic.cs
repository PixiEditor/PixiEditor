using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace PixiEditor.Extensions.CommonApi.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class UseOwnedDiagnostic : DiagnosticAnalyzer
{
    public const string DiagnosticId = "UseOwned";

    public static readonly DiagnosticDescriptor Descriptor = new(DiagnosticId, "Use .Owned() method",
        "Use {0}.Owned{1}() to declare a Setting using the property name", DiagnosticConstants.Category,
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

        var typeInfo = semantics.GetTypeInfo(declaration.Type, context.CancellationToken);

        if (!DiagnosticHelpers.IsSettingType(typeInfo))
        {
            return;
        }
        
        // TODO: Also handle => new()
        if (declaration.Initializer is not { Value: BaseObjectCreationExpressionSyntax { ArgumentList.Arguments: { Count: > 0 } arguments } initializerExpression })
        {
            return;
        }

        var nameArgument = arguments.First();

        var operation = semantics.GetOperation(nameArgument.Expression);

        bool isLiteralMatch = operation is ILiteralOperation { ConstantValue.Value: string s1 } &&
                              s1 == declaration.Identifier.ValueText;

        bool isNameOfMatch = operation is INameOfOperation { ConstantValue.Value: string s2 } &&
                             s2 == declaration.Identifier.ValueText;
        
        if (!isLiteralMatch && !isNameOfMatch)
        {
            return;
        }
        
        var genericType = string.Empty;

        var fallbackValueArgument = arguments.Skip(1).FirstOrDefault();

        var settingType = ((GenericNameSyntax)declaration.Type).TypeArgumentList.Arguments.First();
        if (fallbackValueArgument == null || !SymbolEqualityComparer.Default.Equals(
                semantics.GetTypeInfo(fallbackValueArgument.Expression).Type,
                semantics.GetTypeInfo(settingType).Type))
        {
            genericType = $"<{settingType}>";
        }

        var diagnostic = Diagnostic.Create(Descriptor, initializerExpression.GetLocation(),
            typeInfo.Type?.Name, // LocalSetting or Synced Setting
            genericType);
        
        context.ReportDiagnostic(diagnostic);
    }
}
