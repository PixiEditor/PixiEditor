using System.Collections.Generic;

namespace PixiEditor.Models.Commands;

internal partial class CommandNameList
{
    partial void AddCommands();

    partial void AddEvaluators();

    partial void AddGroups();
    
    public Dictionary<Type, List<(string, Type[])>> Commands { get; }
    
    public Dictionary<Type, List<(string, Type[])>> Evaluators { get; }
    
    public List<Type> Groups { get; }

    public CommandNameList()
    {
        Commands = new();
        Evaluators = new();
        Groups = new();
        AddCommands();
        AddEvaluators();
        AddGroups();
    }
}
