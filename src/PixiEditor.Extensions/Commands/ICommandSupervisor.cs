namespace PixiEditor.Extensions.Commands;

public interface ICommandSupervisor
{
    public bool ValidateCommandPermissions(string commandName, Extension invoker);
}
