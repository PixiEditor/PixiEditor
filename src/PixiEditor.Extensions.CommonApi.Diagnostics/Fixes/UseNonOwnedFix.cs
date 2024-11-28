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

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseNonOwnedFix))]
[Shared]
public class UseNonOwnedFix : CodeFixProvider
{
    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

        var diagnostic = context.Diagnostics.First();
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        var syntax = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<PropertyDeclarationSyntax>().First();

        var action = CodeAction.Create(UseNonOwnedDiagnostic.Title, c => CreateChangedDocument(context.Document, syntax, c), UseNonOwnedDiagnostic.Title);

        context.RegisterCodeFix(action, diagnostic);
    }

    private static async Task<Document> CreateChangedDocument(Document document, PropertyDeclarationSyntax declaration,
        CancellationToken token)
    {
        var invocationExpression = await GetNewInvocation(document, declaration, token);
        var root = await document.GetSyntaxRootAsync(token);

        var newRoot = root!.ReplaceNode(declaration.Initializer!.Value, invocationExpression);

        // TODO: The initializer part does not have it's generic type replaced
        return document.WithSyntaxRoot(newRoot);
    }

    private static async Task<InvocationExpressionSyntax> GetNewInvocation(Document document, PropertyDeclarationSyntax declaration,
        CancellationToken token)
    {
        var settingType = (GenericNameSyntax)declaration.Type;
        var originalInvocation = (BaseObjectCreationExpressionSyntax)declaration.Initializer!.Value;

        var classIdentifier = SyntaxFactory.IdentifierName(settingType.Identifier); // Removes the <> part
        var ownedIdentifier = SyntaxFactory.GenericName(SyntaxFactory.Identifier("NonOwned"), settingType.TypeArgumentList);

        var accessExpression = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, classIdentifier, ownedIdentifier);

        var prefixArgument = await GetPrefixArgument(document, token, originalInvocation);

        var arguments = SkipArgumentAndAdd(originalInvocation.ArgumentList!, prefixArgument);

        var invocationExpression = SyntaxFactory.InvocationExpression(accessExpression, arguments);
        return invocationExpression;
    }

    private static async ValueTask<ArgumentSyntax> GetPrefixArgument(Document document, CancellationToken token,
        BaseObjectCreationExpressionSyntax originalInvocation)
    {
        var semantics = await document.GetSemanticModelAsync(token);
        var originalFirstArgument = originalInvocation.ArgumentList!.Arguments.First();
        var originalName = (string?)semantics!.GetOperation(originalFirstArgument.Expression)?.ConstantValue.Value;

        if (originalName is null)
        {
            throw new NullReferenceException($"Could not determine original name. First argument {originalFirstArgument.ToString()}");
        }
        
        var prefix = DiagnosticHelpers.GetPrefix(originalName)!;
        var prefixLiteral = SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(prefix));
        var prefixArgument = SyntaxFactory.Argument(prefixLiteral);

        return prefixArgument;
    }

    private static ArgumentListSyntax SkipArgumentAndAdd(ArgumentListSyntax original, ArgumentSyntax toPrepend)
    {
        var list = new SeparatedSyntaxList<ArgumentSyntax>();

        list = list.Add(toPrepend);
        list = original.Arguments.Skip(1).Aggregate(list, (current, argument) => current.Add(argument));

        return SyntaxFactory.ArgumentList(list);
    }

    public override ImmutableArray<string> FixableDiagnosticIds { get; } = [UseNonOwnedDiagnostic.DiagnosticId];
}
