using Avalonia.Media;
using Avalonia.Platform.Storage;
using PixiEditor.Models.IO;
using Drawie.Numerics;
using PixiEditor.ViewModels.Document;

namespace PixiEditor.Models.Files;

internal abstract class IoFileType
{
    /// <summary>
    /// Gets or sets file type extensions e.g. {jpg,jpeg}
    /// </summary>
    public abstract string[] Extensions { get; }
    
    /// <summary>
    /// Gets file type's main extensions e.g. jpeg
    /// </summary>
    public string PrimaryExtension { get => Extensions.FirstOrDefault(); }

    /// <summary>
    /// Gets or sets name displayed before extension e.g. JPEG Files
    /// </summary>
    public abstract string DisplayName { get; }

    public virtual SolidColorBrush EditorColor { get; } = new SolidColorBrush(Color.FromRgb(100, 100, 100));
    
    public abstract FileTypeDialogDataSet.SetKind SetKind { get; }

    public FilePickerFileType SaveFilter
    {
        get
        {
            return new FilePickerFileType(DisplayName) { Patterns = ExtensionsFormattedForDialog };
        }
    }

    public List<string> ExtensionsFormattedForDialog
    {
        get { return Extensions.Select(GetExtensionFormattedForDialog).ToList(); }
    }

    public virtual bool CanSave => true;
    
    string GetExtensionFormattedForDialog(string extension)
    {
        return "*" + extension;
    }

    public abstract Task<SaveResult> TrySaveAsync(string pathWithExtension, DocumentViewModel document, ExportConfig config, ExportJob? job);
    public abstract SaveResult TrySave(string pathWithExtension, DocumentViewModel document, ExportConfig config, ExportJob? job);
}
