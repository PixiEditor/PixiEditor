using PixiEditor.Models.DocumentModels.Autosave.Enums;
using PixiEditor.Views.UserControls;

namespace PixiEditor.Models.DocumentModels.Autosave;

#nullable enable
internal interface IAutosaverJob
{
    event EventHandler? OnCompleted;
    AutosaveState CorrespondingState { get; }
    void OnUpdateableChangeEnded();
    void Start();
    void ForceStop();
}
