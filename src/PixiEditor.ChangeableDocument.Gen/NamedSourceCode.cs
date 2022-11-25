namespace PixiEditor.ChangeableDocument.Gen;
internal struct NamedSourceCode
{
    public NamedSourceCode(string name, string code)
    {
        Name = name;
        Code = code;
    }

    public string Name { get; }
    public string Code { get; }
}
