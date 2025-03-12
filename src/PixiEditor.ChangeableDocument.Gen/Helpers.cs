using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace PixiEditor.ChangeableDocument.Gen;

internal static class Helpers
{
    private static SymbolDisplayFormat typeWithGenerics =
        new SymbolDisplayFormat(genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters | SymbolDisplayGenericsOptions.IncludeTypeConstraints);
    public static string CreateMakeChangeAction(MethodInfo changeConstructorInfo)
    {
        string actionName = changeConstructorInfo.ContainingClass.Name.Split('_')[0] + "_Action";
        List<TypeWithName> constructorArgs = changeConstructorInfo.Arguments;
        List<TypeWithName> properties = constructorArgs.Select(static typeWithName =>
            {
                return new TypeWithName(typeWithName.Type, typeWithName.FullNamespace, VariableNameIntoPropertyName(typeWithName.Name), typeWithName.Nullable);
            }).ToList();

        var propToVar = MatchMembers(properties, constructorArgs);

        StringBuilder sb = new();
         
        sb.AppendLine("using Drawie.Backend.Core.Numerics;");
        sb.AppendLine("namespace PixiEditor.ChangeableDocument.Actions.Generated;\n");
        sb.AppendLine("[System.Runtime.CompilerServices.CompilerGenerated]");
        sb.AppendLine($"public record class {actionName} : PixiEditor.ChangeableDocument.Actions.IMakeChangeAction");
        sb.AppendLine("{");
        sb.Append($"public {actionName}");
        AppendArgumentList(sb, constructorArgs);
        AppendConstructorBody(sb, propToVar);
        sb.AppendLine("// Properties");
        AppendProperties(sb, properties);
        sb.AppendLine("// Changes");
        AppendCreateCorrespondingChange(sb, changeConstructorInfo.ContainingClass, properties);
        sb.AppendLine("}");

        return sb.ToString();
    }

    public static Result<string> CreateStartUpdateChangeAction
        (MethodInfo changeConstructorInfo, MethodInfo updateMethodInfo, ClassDeclarationSyntax containingClass, bool isCancelable)
    {
        string actionName = changeConstructorInfo.ContainingClass.Name.Split('_')[0] + "_Action";
        List<TypeWithName> constructorArgs = changeConstructorInfo.Arguments;
        List<TypeWithName> properties = constructorArgs.Select(static typeWithName =>
        {
            return new TypeWithName(typeWithName.Type, typeWithName.FullNamespace, VariableNameIntoPropertyName(typeWithName.Name), typeWithName.Nullable);
        }).ToList();

        var constructorAssignments = MatchMembers(properties, constructorArgs);
        var updatePropsToPass = MatchMembers(updateMethodInfo.Arguments, properties).Select(pair => pair.Item2).ToList();
        if (updatePropsToPass.Count != updateMethodInfo.Arguments.Count)
            return Result<string>.Error("Couldn't match update method arguments with constructor arguments", containingClass.SyntaxTree, containingClass.Span);

        StringBuilder sb = new();

        sb.AppendLine("using Drawie.Backend.Core.Numerics;");
        sb.AppendLine("namespace PixiEditor.ChangeableDocument.Actions.Generated;");
        sb.AppendLine($"public record class {actionName} : PixiEditor.ChangeableDocument.Actions.IStartOrUpdateChangeAction" + (isCancelable ? ", PixiEditor.ChangeableDocument.Actions.ICancelableAction" : ""));
        sb.AppendLine("{");
        sb.Append($"public {actionName}");
        AppendArgumentList(sb, constructorArgs);
        AppendConstructorBody(sb, constructorAssignments);
        AppendProperties(sb, properties);
        AppendCreateUpdateableCorrespondingChange(sb, changeConstructorInfo.ContainingClass, properties);
        AppendUpdateCorrespondingChange(sb, updateMethodInfo.Name, changeConstructorInfo.ContainingClass, updatePropsToPass);
        sb.AppendLine($@"
bool PixiEditor.ChangeableDocument.Actions.IStartOrUpdateChangeAction.IsChangeTypeMatching(PixiEditor.ChangeableDocument.Changes.Change change)
{{
    return change is {changeConstructorInfo.ContainingClass.NameWithNamespace};
}}
");
        sb.AppendLine("}");

        return sb.ToString();
    }

    public static string CreateEndChangeAction(MethodInfo changeConstructorInfo)
    {
        string actionName = "End" + changeConstructorInfo.ContainingClass.Name.Split('_')[0] + "_Action";
        return 
$@"namespace PixiEditor.ChangeableDocument.Actions.Generated;

public record class {actionName} : PixiEditor.ChangeableDocument.Actions.IEndChangeAction
{{
    bool PixiEditor.ChangeableDocument.Actions.IEndChangeAction.IsChangeTypeMatching(PixiEditor.ChangeableDocument.Changes.Change change)
    {{
        return change is {changeConstructorInfo.ContainingClass.NameWithNamespace};
    }}
}}
";
    }

    public static MethodInfo ExtractMethodInfo(IMethodSymbol method)
    {
        List<TypeWithName> variables = method.Parameters.Select(static parameter =>
        {
            return new TypeWithName(
                parameter.Type.ToDisplayString(typeWithGenerics),
                parameter.Type.ContainingNamespace.ToDisplayString(),
                parameter.Name,
                parameter.NullableAnnotation is NullableAnnotation.Annotated
                );
        }).ToList();
        string changeName = method.ContainingType.Name;

        string changeFullNamespace = method.ContainingNamespace.ToDisplayString();
        return new MethodInfo(method.Name, variables, new NamespacedType(changeName, changeFullNamespace));
    }

    private static void AppendConstructorBody(StringBuilder sb, List<(TypeWithName, TypeWithName)> assignments)
    {
        sb.AppendLine("{");
        foreach (var (property, variable) in assignments)
        {
            sb.Append("this.").Append(property.Name).Append(" = ").Append(variable.Name).AppendLine(";");
        }
        sb.AppendLine("}");
    }

    private static List<(TypeWithName, TypeWithName)> MatchMembers(List<TypeWithName> list1, List<TypeWithName> list2)
    {
        List<(TypeWithName, TypeWithName)> paired = new();
        for (int i = list1.Count - 1; i >= 0; i--)
        {
            for (int j = list2.Count - 1; j >= 0; j--)
            {
                if (list1[i].TypeWithNamespace == list2[j].TypeWithNamespace &&
                    list1[i].Name.ToLower() == list2[j].Name.ToLower())
                {
                    paired.Add((list1[i], list2[j]));
                }
            }
        }
        paired.Reverse();
        return paired;
    }

    private static void AppendArgumentList(StringBuilder sb, List<TypeWithName> variables)
    {
        sb.Append("(");
        for (int i = 0; i < variables.Count; i++)
        {
            sb.Append(variables[i].TypeWithNamespace);

            if (variables[i].Nullable)
            {
                sb.Append("?");
            }
            
            sb.Append(" ").Append(variables[i].Name);
            if (i != variables.Count - 1)
                sb.Append(", ");
        }
        sb.AppendLine(")");
    }

    private static void AppendUpdateCorrespondingChange
        (StringBuilder sb, string updateMethodName, NamespacedType corrChangeType, List<TypeWithName> propertiesToPass)
    {
        sb.AppendLine("void PixiEditor.ChangeableDocument.Actions.IStartOrUpdateChangeAction.UpdateCorrespodingChange(PixiEditor.ChangeableDocument.Changes.UpdateableChange change)");
        sb.AppendLine("{");
        sb.Append($"(({corrChangeType.NameWithNamespace})change).{updateMethodName}(");
        for (int i = 0; i < propertiesToPass.Count; i++)
        {
            sb.Append(propertiesToPass[i].Name);
            if (i != propertiesToPass.Count - 1)
                sb.Append(", ");
        }
        sb.AppendLine(");");
        sb.AppendLine("}");
    }

    private static void AppendCreateUpdateableCorrespondingChange
        (StringBuilder sb, NamespacedType corrChangeType, List<TypeWithName> propertiesToPass)
    {
        sb.AppendLine("PixiEditor.ChangeableDocument.Changes.UpdateableChange PixiEditor.ChangeableDocument.Actions.IStartOrUpdateChangeAction.CreateCorrespondingChange()");
        sb.AppendLine("{");
        sb.Append($"return new {corrChangeType.NameWithNamespace}(");
        for (int i = 0; i < propertiesToPass.Count; i++)
        {
            sb.Append(propertiesToPass[i].Name);
            if (i != propertiesToPass.Count - 1)
                sb.Append(", ");
        }
        sb.AppendLine(");");
        sb.AppendLine("}");
    }

    private static void AppendCreateCorrespondingChange
        (StringBuilder sb, NamespacedType corrChangeType, List<TypeWithName> propertiesToPass)
    {
        sb.AppendLine("PixiEditor.ChangeableDocument.Changes.Change PixiEditor.ChangeableDocument.Actions.IMakeChangeAction.CreateCorrespondingChange()");
        sb.AppendLine("{");
        sb.Append($"return new {corrChangeType.NameWithNamespace}(");
        for (int i = 0; i < propertiesToPass.Count; i++)
        {
            sb.Append(propertiesToPass[i].Name);
            if (i != propertiesToPass.Count - 1)
                sb.Append(", ");
        }
        sb.AppendLine(");");
        sb.AppendLine("}");
    }

    private static void AppendProperties(StringBuilder sb, List<TypeWithName> properties)
    {
        foreach (var typeWithName in properties)
        {
            sb.AppendLine($"public {typeWithName.TypeWithNamespace}{(typeWithName.Nullable ? "?" : "")} {typeWithName.Name} {{ get; init; }}");
        }
    }

    private static string VariableNameIntoPropertyName(string varName)
    {
        string lowerCaseName = varName.Substring(0, 1).ToUpperInvariant() + varName.Substring(1);
        return lowerCaseName;
    }

    public static bool IsConstructorWithAttribute(SyntaxNode node, CancellationToken token)
    {
        return node is ConstructorDeclarationSyntax constructor && constructor.AttributeLists.Count > 0;
    }

    public static bool IsMethodWithAttribute(SyntaxNode node, CancellationToken token)
    {
        return node is MethodDeclarationSyntax method && method.AttributeLists.Count > 0;
    }

    public static bool IsInheritedFrom(INamedTypeSymbol classSymbol, NamespacedType type)
    {
        while (classSymbol.BaseType is not null)
        {
            if (classSymbol.BaseType.ToDisplayString() == type.NameWithNamespace)
                return true;
            classSymbol = classSymbol.BaseType;
        }
        return false;
    }

    public static bool MethodHasAttribute
        (GeneratorSyntaxContext context, CancellationToken cancelToken, BaseMethodDeclarationSyntax method, NamespacedType attributeType)
    {
        foreach (var attrList in method.AttributeLists)
        {
            foreach (var attribute in attrList.Attributes)
            {
                cancelToken.ThrowIfCancellationRequested();
                var symbol = context.SemanticModel.GetSymbolInfo(attribute, cancelToken);
                if (symbol.Symbol is not IMethodSymbol methodSymbol)
                    continue;
                if (methodSymbol.ContainingType.ToDisplayString() != attributeType.NameWithNamespace)
                    continue;
                return true;
            }
        }
        return false;
    }
}
