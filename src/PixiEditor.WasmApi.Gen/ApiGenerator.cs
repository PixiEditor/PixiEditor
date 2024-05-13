using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace PixiEditor.Api.Gen;

[Generator]
public class ApiGenerator : IIncrementalGenerator
{
    private const string ApiFunctionAttributeName = "ApiFunctionAttribute";
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var methods = context.SyntaxProvider.CreateSyntaxProvider(CouldBeApiImplAsync, GetApiFunctionMethodOrNull)
            .Where(x => x is not null)
            .Collect();

        context.RegisterSourceOutput(methods, GenerateLinkerCode);
    }

    private void GenerateLinkerCode(SourceProductionContext ctx, ImmutableArray<IMethodSymbol?> symbols)
    {
        if (symbols.IsDefaultOrEmpty) return;

        List<string> linkingMethodsCode = new List<string>();

        foreach (IMethodSymbol? method in symbols)
        {
            if (method == null) continue;

            linkingMethodsCode.Add(GenerateLinkingCodeForMethod(method));
        }

        ctx.AddSource($"{TargetNamespace}.{TargetClassName}.g.cs", BuildFinalCode(linkingMethodsCode));
    }

    private string GenerateLinkingCodeForMethod(IMethodSymbol method)
    {
        AttributeData importName = method.GetAttributes().First(x => x.NamedArguments[0].Key == "Name");
        string name = (string)importName.NamedArguments[0].Value.Value;

        ImmutableArray<IParameterSymbol> arguments = method.Parameters;

        List<string> convertedParams = new List<string>();
        foreach (var argSymbol in arguments)
        {
            convertedParams.AddRange(TypeConversionTable.ConvertTypeToFunctionParams(argSymbol));
        }

        ParameterListSyntax paramList = SyntaxFactory.ParseParameterList(string.Join(",", convertedParams));

        SyntaxList<StatementSyntax> statements = BuildFunctionBody(method, paramList);

        BlockSyntax body = SyntaxFactory.Block(statements);

        var methodExpression = SyntaxFactory.AnonymousMethodExpression(paramList, body);

        SyntaxFactory.ParseStatement($"Linker.DefineFunction(\"env\", {name})
    }

    private SyntaxList<StatementSyntax> BuildFunctionBody(IMethodSymbol method, ParameterListSyntax paramList)
    {
        SyntaxList<StatementSyntax> syntaxes = new SyntaxList<StatementSyntax>();
        foreach (SyntaxReference? reference in method.DeclaringSyntaxReferences)
        {
            if (reference.GetSyntax() is StatementSyntax statementSyntax)
                syntaxes = syntaxes.Add(statementSyntax);
        }

        return syntaxes;
    }

    private bool CouldBeApiImplAsync(SyntaxNode node, CancellationToken cancellation)
    {
        if (node is not AttributeSyntax attribute)
            return false;

        string? name = ExtractName(attribute.Name);

        return name is "ApiFunction" or ApiFunctionAttributeName;
    }

    private string? ExtractName(NameSyntax? attributeName)
    {
        return attributeName switch
        {
            SimpleNameSyntax ins => ins.Identifier.Text,
            QualifiedNameSyntax qns => qns.Right.Identifier.Text,
            _ => null
        };
    }

    private IMethodSymbol? GetApiFunctionMethodOrNull(GeneratorSyntaxContext context, CancellationToken cancelToken)
    {
        AttributeSyntax member = (AttributeSyntax)context.Node;

        if (member.Parent?.Parent is not MethodDeclarationSyntax methodDeclarationSyntax)
            return null;

        var symbol = context.SemanticModel.GetDeclaredSymbol(member, cancelToken);

        if (symbol is IMethodSymbol methodSymbol)
        {
            if (methodSymbol.ReceiverType == null)
                return null;

            return methodSymbol is null || !IsApiFunction(methodSymbol) ? null : methodSymbol;
        }
    }

    private bool IsApiFunction(IMethodSymbol methodSymbol)
    {
        return methodSymbol.GetAttributes().Any(x => x.AttributeClass is {
            Name: ApiFunctionAttributeName,
            ContainingNamespace: {
                Name: "PixiEditor.Extensions.WasmRuntime",
                ContainingNamespace.IsGlobalNamespace: true
        } });
    }
}

class ApiFunction
{

}
