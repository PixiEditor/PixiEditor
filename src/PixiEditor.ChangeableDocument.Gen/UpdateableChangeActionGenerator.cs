using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace PixiEditor.ChangeableDocument.Gen;
[Generator]
public class UpdateableChangeActionGenerator : IIncrementalGenerator
{
    private const string ActionsNamespace = "PixiEditor.ChangeableDocument.Actions";
    private const string ConstructorAttribute = "GenerateUpdateableChangeActionsAttribute";
    private const string UpdateMethodAttribute = "UpdateChangeMethodAttribute";
    private static NamespacedType ConstructorAttributeType = new NamespacedType(ConstructorAttribute, ActionsNamespace);
    private static NamespacedType UpdateMethodAttributeType = new NamespacedType(UpdateMethodAttribute, ActionsNamespace);

    private static Result<(IMethodSymbol, IMethodSymbol)>? TransformSyntax(GeneratorSyntaxContext context, CancellationToken cancelToken)
    {
        ClassDeclarationSyntax containingClass;
        ConstructorDeclarationSyntax constructorSyntax;
        // make sure we are actually working with a constructor
        if (context.Node is ConstructorDeclarationSyntax constructor)
        {
            if (!Helpers.MethodHasAttribute(context, cancelToken, constructor, ConstructorAttributeType))
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
            return Result<(IMethodSymbol, IMethodSymbol)>.Error
                ("The GenerateUpdateableChangeActions and UpdateChangeMethodAttribute can only be used inside UpdateableChanges");
        }

        // here we are sure we are inside an updateable change, time to find the update method
        MethodDeclarationSyntax? methodSyntax = null;
        var members = containingClass.Members.Where(node => node is MethodDeclarationSyntax);
        const string errorMessage = $"Update method isn't marked with {UpdateMethodAttribute}";
        if (!members.Any())
            return Result<(IMethodSymbol, IMethodSymbol)>.Error(errorMessage);
        foreach (var member in members)
        {
            cancelToken.ThrowIfCancellationRequested();
            var method = (MethodDeclarationSyntax)member;
            bool hasAttr = Helpers.MethodHasAttribute(context, cancelToken, method, UpdateMethodAttributeType);
            if (hasAttr)
            {
                methodSyntax = method;
                break;
            }
        }
        if (methodSyntax is null)
        {
            return Result<(IMethodSymbol, IMethodSymbol)>.Error(errorMessage);
        }

        // finally, get symbols
        var methodSymbol = context.SemanticModel.GetDeclaredSymbol(methodSyntax, cancelToken);
        var contructorSymbol = context.SemanticModel.GetDeclaredSymbol(constructorSyntax, cancelToken);
        if (contructorSymbol is not IMethodSymbol || methodSymbol is not IMethodSymbol)
            return null;
        return ((IMethodSymbol)contructorSymbol, (IMethodSymbol)methodSymbol);
    }

    private static Result<(NamedSourceCode, NamedSourceCode)> GenerateActions
        (Result<(IMethodSymbol, IMethodSymbol)>? prevResult, CancellationToken cancelToken)
    {
        if (prevResult!.Value.ErrorText is not null)
            return Result<(NamedSourceCode, NamedSourceCode)>.Error(prevResult.Value.ErrorText);
        var (constructor, update) = prevResult.Value.Value;

        var constructorInfo = Helpers.ExtractMethodInfo(constructor!);
        var updateInfo = Helpers.ExtractMethodInfo(update!);

        var maybeStartUpdateAction = Helpers.CreateStartUpdateChangeAction(constructorInfo, updateInfo);
        if (maybeStartUpdateAction.ErrorText is not null)
            return Result<(NamedSourceCode, NamedSourceCode)>.Error(maybeStartUpdateAction.ErrorText);

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
                        null));
                return;
            }

            var (act1, act2) = namedActions.Value;
            context.AddSource(act1.Name, act1.Code);
            context.AddSource(act2.Name, act2.Code);
        });
    }
}
