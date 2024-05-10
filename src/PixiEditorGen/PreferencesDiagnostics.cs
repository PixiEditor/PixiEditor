using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace PixiEditorGen;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class PreferencesDiagnostics : DiagnosticAnalyzer
{
    private const string Attribute = "PreferenceConstantAttribute";
    
    private static DiagnosticDescriptor wrongDestinationDescriptor = new("WrongDestination",
        "Wrong preferences destination", "Preference '{0}' should be used with {1} preferences",
        "PixiEditor", DiagnosticSeverity.Error, true);
    
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.InvocationExpression);
    }

    private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        var invocationExpression = (InvocationExpressionSyntax)context.Node;

        if (!TryGetMethodSymbol(context, invocationExpression, out var methodSymbol, out var methodNameSyntax))
        {
            return;
        }

        for (var i = 0; i < methodSymbol.Parameters.Length; i++)
        {
            if (invocationExpression.ArgumentList.Arguments[i].Expression is not MemberAccessExpressionSyntax member)
            {
                continue;
            }

            var parameter = methodSymbol.Parameters[i];

            if (context.SemanticModel.GetSymbolInfo(member).Symbol is not { } symbol)
            {
                return;
            }

            CheckAttributes(context, symbol, invocationExpression, methodNameSyntax, parameter);
        }
    }

    private static bool TryGetMethodSymbol(SyntaxNodeAnalysisContext context, InvocationExpressionSyntax invocation, [NotNullWhen(true)] out IMethodSymbol? symbol, out SimpleNameSyntax? nameSyntax)
    {
        symbol = null;
        nameSyntax = null;
        if (invocation.ArgumentList.Arguments.Count == 0) return false;
        
        nameSyntax = invocation.Expression switch
        {
            MemberAccessExpressionSyntax memberAccess => memberAccess.Name, // for `obj.Method()` or `Class.Method()`
            IdentifierNameSyntax identifierName => identifierName,          // for `Method()`
            _ => null
        };

        symbol = context.SemanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;

        return symbol != null;
    }
    
    private static void CheckAttributes(SyntaxNodeAnalysisContext context, ISymbol symbol, InvocationExpressionSyntax invocation, SimpleNameSyntax? methodNameExpr, ISymbol parameterSymbol)
    {
        bool hasCorrectUsage = false;
        bool hasIncorrectUsage = false;
        AttributeData? incorrectAttributeData = null;
        
        var attributes = parameterSymbol.GetAttributes();
        for (var i = 0; i < attributes.Length; i++)
        {
            var attributeData = attributes[i];
            if (attributeData.AttributeClass?.BaseType?.Name != Attribute)
            {
                continue;
            }

            var correctAttribute = symbol.GetAttributes().FirstOrDefault(x =>
                x.AttributeClass?.BaseType?.Name == Attribute &&
                x.AttributeClass?.Name == attributeData.AttributeClass?.Name);

            if (correctAttribute != null)
            {
                hasCorrectUsage = true;
                break;
            }

            incorrectAttributeData =
                symbol.GetAttributes().FirstOrDefault(x => x.AttributeClass?.BaseType?.Name == Attribute);
            hasIncorrectUsage = true;
            i++;
        }

        if (hasCorrectUsage || !hasIncorrectUsage)
        {
            return;
        }

        var diagnostic = Diagnostic.Create(
            wrongDestinationDescriptor,
            methodNameExpr?.GetLocation() ?? invocation.GetLocation(), symbol?.ToDisplayString(),
            incorrectAttributeData?.AttributeClass?.Name.Replace(Attribute, "").ToLower());

        context.ReportDiagnostic(diagnostic);
    }

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get => ImmutableArray.Create(wrongDestinationDescriptor);
    }
}
