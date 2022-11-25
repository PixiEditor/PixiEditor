namespace PixiEditor.ChangeableDocument.Gen;
internal struct NamespacedType
{
    public NamespacedType(string name, string fullNamespace)
    {
        Name = name;
        FullNamespace = fullNamespace;
    }

    public string Name { get; }
    public string FullNamespace { get; }
    public string NameWithNamespace => FullNamespace + "." + Name;
}
