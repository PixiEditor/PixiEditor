using System.Collections.Generic;

namespace PixiEditor.Models.Controllers;

/// <summary>
///     A class that holds startup command line arguments + custom passed ones.
/// </summary>
public static class StartupArgs
{
    public static List<string> Args { get; set; }
}