namespace PixiEditor.ChangeableDocument.Gen;
internal struct Result<T>
{
    public string? ErrorText { get; }
    public T? Value { get; }

    private Result(string? error, T? value)
    {
        ErrorText = error;
        Value = value;
    }
    public static Result<T> Error(string text)
    {
        return new Result<T>(text, default(T));
    }

    public static implicit operator Result<T>(T value)
    {
        return new Result<T>(null, value);
    }
}
