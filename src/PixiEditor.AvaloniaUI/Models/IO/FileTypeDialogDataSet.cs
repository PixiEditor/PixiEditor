using System.Collections.Generic;
using System.Linq;
using Avalonia.Platform.Storage;
using PixiEditor.AvaloniaUI.Helpers;
using PixiEditor.AvaloniaUI.Models.Files;
using PixiEditor.Extensions.Common.Localization;

namespace PixiEditor.AvaloniaUI.Models.IO;

internal class FileTypeDialogDataSet
{
    public enum SetKind { Any, Pixi, Images }
    IEnumerable<IoFileType> fileTypes;
    string displayName;

    public FileTypeDialogDataSet(SetKind kind, IEnumerable<IoFileType> fileTypes = null)
    {
        if (fileTypes == null)
            fileTypes = SupportedFilesHelper.GetAllSupportedFileTypes(true);
        var allSupportedExtensions = fileTypes;
        if (kind == SetKind.Any)
        {
            Init(new LocalizedString("ANY"), allSupportedExtensions);
        }
        else if (kind == SetKind.Pixi)
        {
            Init(new LocalizedString("PIXI_FILE"), new[] { PixiFileType.PixiFile });
        }
        else if (kind == SetKind.Images)
        {
            Init(new LocalizedString("IMAGE_FILES"), allSupportedExtensions, PixiFileType.PixiFile);
        }
    }

    public FileTypeDialogDataSet(string displayName, IEnumerable<IoFileType> fileTypes, IoFileType? fileTypeToSkip = null)
    {
        Init(displayName, fileTypes, fileTypeToSkip);
    }

    private void Init(string displayName, IEnumerable<IoFileType> fileTypes, IoFileType? fileTypeToSkip = null)
    {
        var copy = fileTypes.ToList();
        if (fileTypeToSkip != null)
            copy.RemoveAll(i => i == fileTypeToSkip);
        this.fileTypes = copy;

        this.displayName = displayName;
    }

    public FilePickerFileType[] GetFormattedTypes(bool includeCommon)
    {
        List<FilePickerFileType> types = new();
        if (includeCommon)
        {
            FilePickerFileType common = new FilePickerFileType(displayName);
            common.Patterns = fileTypes.SelectMany(i => i.SaveFilter.Patterns).ToArray();
            types.Add(common);
        }

        types.AddRange(fileTypes.Select(i => i.SaveFilter));
        return types.ToArray();
    }
}
