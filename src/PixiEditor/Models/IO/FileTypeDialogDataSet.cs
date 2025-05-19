using System.Collections.Generic;
using System.Linq;
using Avalonia.Platform.Storage;
using PixiEditor.Helpers;
using PixiEditor.Models.Files;
using PixiEditor.UI.Common.Localization;

namespace PixiEditor.Models.IO;

internal class FileTypeDialogDataSet
{
    [Flags]
    public enum SetKind
    {
        None = 0,
        Pixi = 1 << 0,
        Image = 1 << 1,
        Video = 1 << 2,
        Vector = 1 << 3,
        Any = ~0,
    }
    IEnumerable<IoFileType> fileTypes;
    string displayName;

    public FileTypeDialogDataSet(SetKind kind, IEnumerable<IoFileType> fileTypes = null)
    {
        if (fileTypes == null)
            fileTypes = SupportedFilesHelper.GetAllSupportedFileTypes(kind);
        var allSupportedExtensions = fileTypes;
        if (kind == SetKind.Any)
        {
            Init(new LocalizedString("ANY"), allSupportedExtensions);
        }
        else if (kind == SetKind.Pixi)
        {
            Init(new LocalizedString("PIXI_FILE"), new[] { PixiFileType.PixiFile });
        }
        else if (kind == SetKind.Image)
        {
            Init(new LocalizedString("IMAGE_FILES"), allSupportedExtensions, PixiFileType.PixiFile);
        }
        else if (kind == SetKind.Video)
        {
            Init(new LocalizedString("VIDEO_FILES"), allSupportedExtensions);
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
