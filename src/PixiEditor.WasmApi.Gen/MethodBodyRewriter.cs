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

        if(memberSymbol.Kind != SymbolKind.Property && memberSymbol.Kind != SymbolKind.Field)
        {
            return base.VisitMemberAccessExpression(node);
        }

        if(!memberSymbol.IsStatic)
        {
            return base.VisitMemberAccessExpression(node);
        }

        var fullyQualifiedName = memberSymbol.ToDisplayString();

        var newMemberAccess = SyntaxFactory.ParseExpression(fullyQualifiedName);

        return newMemberAccess;
    }
}
