namespace PixiEditor.ChangeableDocument.Gen;
internal struct TypeWithName
{
    public TypeWithName(string type, string fullNamespace, string name)
    {
        Type = type;
        FullNamespace = fullNamespace;
        Name = name;
    }

    public string Type { get; }
    public string FullNamespace { get; }
    public string TypeWithNamespace => FullNamespace + "." + Type;
    public string Name { get; }
}
