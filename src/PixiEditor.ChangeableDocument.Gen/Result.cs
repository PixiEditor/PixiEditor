using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace PixiEditor.ChangeableDocument.Gen;
internal struct Result<T>
{
    public string? ErrorText { get; }
    public SyntaxTree? SyntaxTree { get; }
    public TextSpan? Span { get; }
    public T? Value { get; }

    private Result(string? error, T? value, SyntaxTree? syntaxTree, TextSpan? span)
    {
        ErrorText = error;
        Value = value;
        SyntaxTree = syntaxTree;
        Span = span;
    }
    public static Result<T> Error(string text, SyntaxTree tree, TextSpan span)
    {
        return new Result<T>(text, default(T), tree, span);
    }

    public static implicit operator Result<T>(T value)
    {
        return new Result<T>(null, value, null, null);
    }
}
