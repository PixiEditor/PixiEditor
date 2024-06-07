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

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseOwnedFix))]
[Shared]
public class UseOwnedFix : CodeFixProvider
{
    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

        var diagnostic = context.Diagnostics.First();
        var diagnosticSpan = diagnostic.Location.SourceSpan;
        
        var syntax = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<PropertyDeclarationSyntax>().First();

        var action = CodeAction.Create(UseOwnedDiagnostic.Title, c => CreateChangedDocument(context.Document, syntax, c), UseOwnedDiagnostic.Title);
        
        context.RegisterCodeFix(action, diagnostic);
    }

    private static async Task<Document> CreateChangedDocument(Document document, PropertyDeclarationSyntax declaration, CancellationToken token)
    {
        var invocationExpression = GetNewInvocation(declaration);
        var root = await document.GetSyntaxRootAsync(token);

        var newRoot = root!.ReplaceNode(declaration.Initializer.Value, invocationExpression);

        // TODO: The initializer part does not have it's generic type replaced
        return document.WithSyntaxRoot(newRoot);
    }

    private static InvocationExpressionSyntax GetNewInvocation(PropertyDeclarationSyntax declaration)
    {
        var settingType = (GenericNameSyntax)declaration.Type;
        var originalInvocation = (BaseObjectCreationExpressionSyntax)declaration.Initializer!.Value;
        
        var classIdentifier = SyntaxFactory.IdentifierName(settingType.Identifier); // Removes the <> part
        var ownedIdentifier = SyntaxFactory.GenericName(SyntaxFactory.Identifier("Owned"), settingType.TypeArgumentList);
        
        var accessExpression = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, classIdentifier, ownedIdentifier);
        var invocationExpression = SyntaxFactory.InvocationExpression(accessExpression, SkipArgument(originalInvocation.ArgumentList!));
        return invocationExpression;
    }

    private static ArgumentListSyntax SkipArgument(ArgumentListSyntax original)
    {
        var list = new SeparatedSyntaxList<ArgumentSyntax>();

        list = original.Arguments.Skip(1).Aggregate(list, (current, argument) => current.Add(argument));

        return SyntaxFactory.ArgumentList(list);
    }

    public override ImmutableArray<string> FixableDiagnosticIds { get; } = [UseOwnedDiagnostic.DiagnosticId];
}
