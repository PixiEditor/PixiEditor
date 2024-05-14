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

    public override SyntaxNode VisitInvocationExpression(InvocationExpressionSyntax node)
    {
        /*var methodSymbol = MethodSemanticModel.GetSymbolInfo(node).Symbol as IMethodSymbol;

        var fullyQualifiedName = methodSymbol.ToDisplayString();

        var newInvocation = SyntaxFactory.ParseExpression($"{fullyQualifiedName}({string.Join(", ", node.ArgumentList.Arguments)})");

        return newInvocation;*/

        return base.VisitInvocationExpression(node);
    }

    public override SyntaxNode VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
    {
        var memberSymbol = MethodSemanticModel.GetSymbolInfo(node).Symbol;

        if(memberSymbol.Kind != SymbolKind.Field && memberSymbol.Kind != SymbolKind.Method)
        {
            return base.VisitMemberAccessExpression(node);
        }

        string fullyQualifiedName = memberSymbol.ToDisplayString();

        if (memberSymbol is { Kind: SymbolKind.Method, IsStatic: false })
        {
            var genericArguments = ((IMethodSymbol)memberSymbol).TypeArguments;

            var genericArgumentsString = genericArguments.Length > 0
                ? $"<{string.Join(", ", genericArguments.Select(x => x.ToDisplayString()))}>"
                : string.Empty;

            string caller = node.Expression.ToFullString();

            fullyQualifiedName = $"{caller}.{memberSymbol.Name}{genericArgumentsString}";
        }

        var newMemberAccess = SyntaxFactory.ParseExpression(fullyQualifiedName);

        return newMemberAccess;
    }
}
