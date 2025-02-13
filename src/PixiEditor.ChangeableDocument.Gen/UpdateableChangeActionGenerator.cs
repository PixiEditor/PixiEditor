using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace PixiEditor.ChangeableDocument.Gen;
[Generator]
public class UpdateableChangeActionGenerator : IIncrementalGenerator
{
    private const string AttributesNamespace = "PixiEditor.ChangeableDocument.Actions.Attributes";
    private const string ConstructorAttribute = "GenerateUpdateableChangeActionsAttribute";
    private const string UpdateMethodAttribute = "UpdateChangeMethodAttribute";
    private static NamespacedType constructorAttributeType = new NamespacedType(ConstructorAttribute, AttributesNamespace);
    private static NamespacedType updateMethodAttributeType = new NamespacedType(UpdateMethodAttribute, AttributesNamespace);

    private static Result<(IMethodSymbol, IMethodSymbol, ClassDeclarationSyntax, bool)>? TransformSyntax
        (GeneratorSyntaxContext context, CancellationToken cancelToken)
    {
        ClassDeclarationSyntax containingClass;
        ConstructorDeclarationSyntax constructorSyntax;
        // make sure we are actually working with a constructor
        if (context.Node is ConstructorDeclarationSyntax constructor)
        {
            if (!Helpers.MethodHasAttribute(context, cancelToken, constructor, constructorAttributeType))
                return null;
            containingClass = (ClassDeclarationSyntax)constructor.Parent!;
            constructorSyntax = constructor;
        }
        else
        {
            return null;
        }

        var classSymbol = (INamedTypeSymbol)context.SemanticModel.GetDeclaredSymbol(containingClass)!;
        if (!Helpers.IsInheritedFrom(classSymbol, new("UpdateableChange", "PixiEditor.ChangeableDocument.Changes")))
        {
            return Result<(IMethodSymbol, IMethodSymbol, ClassDeclarationSyntax, bool)>.Error
                ("The GenerateUpdateableChangeActions and UpdateChangeMethodAttribute can only be used inside UpdateableChanges", containingClass.SyntaxTree, containingClass.Span);
        }
        
        bool isCancelable = Helpers.IsInheritedFrom(classSymbol, new("CancelableUpdateableChange", "PixiEditor.ChangeableDocument.Changes"));

        // here we are sure we are inside an updateable change, time to find the update method
        MethodDeclarationSyntax? methodSyntax = null;
        var members = containingClass.Members.Where(node => node is MethodDeclarationSyntax).ToList();
        const string errorMessage = $"Update method isn't marked with {UpdateMethodAttribute}";
        if (!members.Any())
        {
            return Result<(IMethodSymbol, IMethodSymbol, ClassDeclarationSyntax, bool)>.Error
                (errorMessage, containingClass.SyntaxTree, containingClass.Span);
        }
        foreach (var member in members)
        {
            cancelToken.ThrowIfCancellationRequested();
            var method = (MethodDeclarationSyntax)member;
            bool hasAttr = Helpers.MethodHasAttribute(context, cancelToken, method, updateMethodAttributeType);
            if (hasAttr)
            {
                methodSyntax = method;
                break;
            }
        }
        if (methodSyntax is null)
        {
            return Result<(IMethodSymbol, IMethodSymbol, ClassDeclarationSyntax, bool)>.Error
                (errorMessage, containingClass.SyntaxTree, containingClass.Span);
        }

        // finally, get symbols
        var methodSymbol = context.SemanticModel.GetDeclaredSymbol(methodSyntax, cancelToken);
        var constructorSymbol = context.SemanticModel.GetDeclaredSymbol(constructorSyntax, cancelToken);
        if (constructorSymbol is not IMethodSymbol || methodSymbol is not IMethodSymbol)
            return null;
        
        return ((IMethodSymbol)constructorSymbol, (IMethodSymbol)methodSymbol, containingClass, isCancelable);
    }

    private static Result<(NamedSourceCode, NamedSourceCode)> GenerateActions
        (Result<(IMethodSymbol, IMethodSymbol, ClassDeclarationSyntax, bool)>? prevResult, CancellationToken cancelToken)
    {
        if (prevResult!.Value.ErrorText is not null)
        {
            return Result<(NamedSourceCode, NamedSourceCode)>.Error
                (prevResult.Value.ErrorText, prevResult.Value.SyntaxTree!, (TextSpan)prevResult.Value.Span!);
        }
        var (constructor, update, containingClass, isCancelable) = prevResult.Value.Value;

        var constructorInfo = Helpers.ExtractMethodInfo(constructor!);
        var updateInfo = Helpers.ExtractMethodInfo(update!);

        var maybeStartUpdateAction = Helpers.CreateStartUpdateChangeAction(constructorInfo, updateInfo, containingClass, isCancelable);
        if (maybeStartUpdateAction.ErrorText is not null)
        {
            return Result<(NamedSourceCode, NamedSourceCode)>.Error
                (maybeStartUpdateAction.ErrorText, maybeStartUpdateAction.SyntaxTree!, (TextSpan)maybeStartUpdateAction.Span!);
        }

        var endAction = Helpers.CreateEndChangeAction(constructorInfo);

        return (
            new NamedSourceCode(constructorInfo.ContainingClass.Name + "StartUpdate", maybeStartUpdateAction.Value!),
            new NamedSourceCode(constructorInfo.ContainingClass.Name + "End", endAction)
            );
    }

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // find the contrustor and the update method using the attributes
        var constructorSymbolProvider = context.SyntaxProvider.CreateSyntaxProvider(
            predicate: static (node, token) => Helpers.IsMethodWithAttribute(node, token) || Helpers.IsConstructorWithAttribute(node, token),
            transform: TransformSyntax
        ).Where(a => a is not null);

        // generate the source code of actions
        var actionSourceCodeProvider = constructorSymbolProvider.Select(GenerateActions);

        // add the source code into compiler input
        context.RegisterSourceOutput(actionSourceCodeProvider, static (context, namedActions) =>
        {
            if (namedActions.ErrorText is not null)
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        new DiagnosticDescriptor("AGErr", "", namedActions.ErrorText, "UpdateableActionGenerator", DiagnosticSeverity.Error, true),
                        Location.Create(namedActions.SyntaxTree!, (TextSpan)namedActions.Span!)));
                return;
            }

            var (act1, act2) = namedActions.Value;
            context.AddSource(act1.Name, act1.Code);
            context.AddSource(act2.Name, act2.Code);
        });
    }
}
