namespace PixiEditor.Models.Commands.Exceptions;

[Serializable]
internal class CommandNotFoundException : Exception
{
    public string CommandName { get; set; }

    public CommandNotFoundException(string name) : this(name, null) { }

    public CommandNotFoundException(string name, Exception inner) : base($"No command with the name '{name}' found", inner) { }

    protected CommandNotFoundException(
      System.Runtime.Serialization.SerializationInfo info,
      System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}
