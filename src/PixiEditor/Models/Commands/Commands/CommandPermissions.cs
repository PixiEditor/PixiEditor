namespace PixiEditor.Models.Commands.Commands;

public enum CommandPermissions
{
    /// <summary>
    ///     Only the registering extension can use this command.
    /// </summary>
    Owner,

    /// <summary>
    ///     Only extensions explicitly whitelisted by the registering extension can use this command.
    /// </summary>
    Explicit,

    /// <summary>
    ///     Only extensions that are part of the same family can use this command. A family is a group under the same
    ///     unique name prefix.
    /// </summary>
    Family,

    /// <summary>
    ///     Any extension can use this command.
    /// </summary>
    Public,
}
