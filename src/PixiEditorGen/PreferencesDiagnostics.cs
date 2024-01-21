using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace PixiEditorGen;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class PreferencesDiagnostics : DiagnosticAnalyzer
{
    private static DiagnosticDescriptor wrongDestinationDescriptor = new("WrongDestination",
        "Wrong preferences destination", "Preference '{0}' should be used with {1} preferences",
        "PixiEditor", DiagnosticSeverity.Error, true);
    
    public override void Initialize(AnalysisContext context)
    {
        PostLogMessage("hello");
        
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.InvocationExpression);
    }

    private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;

        if (invocation.ArgumentList.Arguments.Count == 0) return;
        
        var methodNameExpr = invocation.Expression switch
        {
            MemberAccessExpressionSyntax memberAccess => memberAccess.Name, // for `obj.Method()` or `Class.Method()`
            IdentifierNameSyntax identifierName => identifierName,          // for `Method()`
            _ => null
        };

        var methodSymbol = context.SemanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;

        if (methodSymbol == null)
        {
            return;
        }

        for (var i = 0; i < methodSymbol.Parameters.Length; i++)
        {
            if (invocation.ArgumentList.Arguments[i].Expression is not MemberAccessExpressionSyntax member)
            {
                continue;
            }

            var parameterSymbol = methodSymbol.Parameters[i];
            
            foreach (var attributeData in parameterSymbol.GetAttributes())
            {
                if (attributeData.AttributeClass?.BaseType?.Name != "PreferenceConstantAttribute")
                {
                    continue;
                }

                if (context.SemanticModel.GetSymbolInfo(member).Symbol is not { } symbol)
                {
                    continue;
                }

                var memberAttributeData = symbol.GetAttributes()
                    .FirstOrDefault(x => x.AttributeClass?.BaseType?.Name == "PreferenceConstantAttribute");
                
                if (memberAttributeData != null && memberAttributeData.AttributeClass?.Name != attributeData.AttributeClass?.Name)
                {
                    var diagnostic = Diagnostic.Create(
                        wrongDestinationDescriptor,
                        methodNameExpr?.GetLocation() ?? invocation.GetLocation(), symbol.ToDisplayString(),
                        memberAttributeData.AttributeClass?.Name.Replace("PreferenceConstantAttribute", "").ToLower());

                    context.ReportDiagnostic(diagnostic);
                }

                break;
            }
        }
    }

    static void PostLogMessage(string message)
    {
        Task.Run(() => PostLogMessageAsync(message));
    }

    static async Task PostLogMessageAsync(string logMessage)
    {
        using var client = new HttpClient();
        
        var content = new StringContent(logMessage, Encoding.UTF8, "text/plain");
        HttpResponseMessage response = await client.PostAsync("http://localhost:8080", content);

        if (response.IsSuccessStatusCode)
        {
            string responseBody = await response.Content.ReadAsStringAsync();
            Console.WriteLine(responseBody);
        }
        else
        {
            Console.WriteLine($"Error: {response.StatusCode}");
        }
    }
    
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get => ImmutableArray.Create(wrongDestinationDescriptor);
    }
}
