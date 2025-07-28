using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace PixiEditor.Extensions.Gen;

internal static class Helpers
{
    public static bool IsLayoutElementOrStateType(SyntaxNode classSymbol, CancellationToken token)
    {
        return classSymbol is ClassDeclarationSyntax classDeclarationSyntax &&
               (IsTypeName(classDeclarationSyntax, "LayoutElement")
               || IsTypeName(classDeclarationSyntax, "State"));
    }

    private static bool IsTypeName(ClassDeclarationSyntax classDeclarationSyntax, string name)
    {
        return classDeclarationSyntax.BaseList?.Types.Any(x => x.Type is IdentifierNameSyntax identifierNameSyntax && identifierNameSyntax.Identifier.Text == name) == true;
    }

    public static bool IsLayoutElementOrStateType(Compilation classSymbol, CancellationToken token)
    {
        return classSymbol.SyntaxTrees.Any(x => x.GetRoot().DescendantNodes().Any(y => IsLayoutElementOrStateType(y, token)));
    }
}
