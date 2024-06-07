using System;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PixiEditor.Extensions.CommonApi.Diagnostics.Diagnostics;

namespace PixiEditor.Extensions.CommonApi.Diagnostics.Fixes;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseGenericEnumerableForListArrayFix))]
[Shared]
public class UseGenericEnumerableForListArrayFix : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds { get; } =
        [UseGenericEnumerableForListArrayDiagnostic.DiagnosticId];
    
    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

        var diagnostic = context.Diagnostics.First();
        var diagnosticSpan = diagnostic.Location.SourceSpan;
        
        var syntax = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<GenericNameSyntax>().First();

        var typeArgument = syntax.TypeArgumentList.Arguments.First();
        
        var title = $"Use IEnumerable<{GetTargetTypeName(typeArgument)}>";
        // TODO: equivalenceKey only works for types with the same name. Is there some way to make this generic?
        var action = CodeAction.Create(title, c => CreateChangedDocument(context.Document, syntax, c), title);
        
        context.RegisterCodeFix(action, diagnostic);
    }

    public override FixAllProvider GetFixAllProvider()
    {
        return WellKnownFixAllProviders.BatchFixer;
    }

    private static async Task<Document> CreateChangedDocument(Document document, GenericNameSyntax syntax, CancellationToken token)
    {
        var typeArgument = syntax.TypeArgumentList.Arguments.First();
        var genericType = (TypeSyntax)GetNewGenericType(typeArgument);

        var typeList = SyntaxFactory.TypeArgumentList(SyntaxFactory.SeparatedList([genericType]));
        var replacement = SyntaxFactory.GenericName(syntax.Identifier, typeList);
        
        var root = await document.GetSyntaxRootAsync(token);

        if (root == null)
        {
            throw new Exception("Document root was null. No code fix for you sadly :(");
        }

        var newRoot = root.ReplaceNode(syntax, replacement);
        
        return document.WithSyntaxRoot(newRoot);
    }
    private static GenericNameSyntax GetNewGenericType(TypeSyntax typeSyntax)
    {
        var targetTypeName = GetTargetTypeName(typeSyntax);
        
        var identifierToken = SyntaxFactory.Identifier("IEnumerable");
        var separatedList = SyntaxFactory.SeparatedList([SyntaxFactory.ParseTypeName(targetTypeName)]);
        var typeList = SyntaxFactory.TypeArgumentList(separatedList);
        
        return SyntaxFactory.GenericName(identifierToken, typeList);
    }

    private static string GetTargetTypeName(TypeSyntax typeSyntax) => typeSyntax switch
        {
            ArrayTypeSyntax array => array.ElementType.ToString(),
            GenericNameSyntax genericName => genericName.TypeArgumentList.Arguments.First().ToString(),
            _ => throw new ArgumentException(
                $"{nameof(typeSyntax)} must either be a ArrayTypeSyntax or GenericNameSyntax")
        };
}
