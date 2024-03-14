using PixiEditor.Models.DocumentModels.Autosave.Enums;

namespace PixiEditor.Models.DocumentModels.Autosave.Structs;

public struct LastUserFileAutosaveData
{
    public DateTime Time { get; set; }
    
    public UserFileAutosaveResult SaveResult { get; set; }
}
