using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace PixiEditor.WasmApi.Gen;

public class MethodBodyRewriter : CSharpSyntaxRewriter
{
    public SemanticModel MethodSemanticModel { get; }

    public MethodBodyRewriter(SemanticModel methodSemanticModel)
    {
        MethodSemanticModel = methodSemanticModel;
    }

    public override SyntaxNode? VisitIdentifierName(IdentifierNameSyntax node)
    {
        var symbol = MethodSemanticModel.GetSymbolInfo(node).Symbol;

        if (symbol is not INamedTypeSymbol { Kind: SymbolKind.NamedType } namedTypeSymbol)
        {
            return base.VisitIdentifierName(node);
        }

        var newIdentifier = SyntaxFactory.ParseName($"{namedTypeSymbol.ToDisplayString()} ");

        return newIdentifier;
    }
    
    // seems like above doesn't work for elements that are after return statement, TODO: fix this
}
