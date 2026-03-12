using Avalonia.Platform.Storage;
using PixiEditor.Models.Files;
using PixiEditor.Models.IO;
using PixiEditor.UI.Common.Localization;

namespace PixiEditor.Helpers;

internal class SupportedFilesHelper
{
    public static string[] AllSupportedExtensions { get; private set; }
    public static string[] PrimaryExtensions { get; private set; }

    public static List<IoFileType> FileTypes { get; private set; }

    public static void InitFileTypes(IEnumerable<IoFileType> fileTypes)
    {
        FileTypes = fileTypes.ToList();

        AllSupportedExtensions = FileTypes.SelectMany(i => i.Extensions).ToArray();
        PrimaryExtensions = FileTypes.Select(i => i.PrimaryExtension).ToArray();
    }

    public static string FixFileExtension(string pathWithOrWithoutExtension, IoFileType requestedType)
    {
        if (requestedType == null)
            throw new ArgumentException("A valid filetype is required", nameof(requestedType));

        var typeFromPath = ParseImageFormat(Path.GetExtension(pathWithOrWithoutExtension));
        if (typeFromPath != null && typeFromPath == requestedType)
            return pathWithOrWithoutExtension;
        return AppendExtension(pathWithOrWithoutExtension, requestedType);
    }

    public static string AppendExtension(string path, IoFileType data)
    {
        string ext = data.Extensions.First();
        string filename = Path.GetFileName(path);
        if (filename.Length + ext.Length > 255)
            filename = filename.Substring(0, 255 - ext.Length);
        filename += ext;
        return Path.Combine(Path.GetDirectoryName(path), filename);
    }

    public static bool IsSupported(string path)
    {
        var ext = Path.GetExtension(path.ToLower());
        if (string.IsNullOrEmpty(ext))
        {
            ext = $".{path.ToLower()}";
        }

        return IsExtensionSupported(ext);
    }

    public static bool IsExtensionSupported(string fileExtension)
    {
        return AllSupportedExtensions.Contains(fileExtension, StringComparer.OrdinalIgnoreCase);
    }

    public static IoFileType? ParseImageFormat(string extension)
    {
        var allExts = FileTypes;
        var fileData = allExts.SingleOrDefault(i => i.Extensions.Contains(extension, StringComparer.OrdinalIgnoreCase));
        return fileData;
    }

    public static List<IoFileType> GetAllSupportedFileTypes(FileTypeDialogDataSet.SetKind setKind)
    {
        if (setKind.HasFlag(FileTypeDialogDataSet.SetKind.Any))
        {
            return FileTypes.ToList();
        }

        var separatedKinds = setKind.GetFlags();
        List<IoFileType> allExts = new();
        foreach (var separatedKind in separatedKinds)
        {
            if(separatedKind == FileTypeDialogDataSet.SetKind.None) continue;

            allExts.AddRange(FileTypes.Where(i => i.SetKind.HasFlag(separatedKind)));
        }

        return allExts.Distinct().ToList();
    }

    public static List<FilePickerFileType> BuildSaveFilter(
        FileTypeDialogDataSet.SetKind setKind = FileTypeDialogDataSet.SetKind.Any)
    {
        var allSupportedExtensions = GetAllSupportedFileTypes(setKind).Where(x => x.CanSave).ToList();
        var filter = allSupportedExtensions.Select(i => i.SaveFilter).ToList();

        return filter;
    }

    public static (IoFileType? type, string path) GetSaveFileTypeAndPath(FileTypeDialogDataSet.SetKind setKind,
        IStorageFile file, FilePickerFileType fileType)
    {
        var allSupportedExtensions = GetAllSupportedFileTypes(setKind);

        var localPath = file.TryGetLocalPath();

        if (localPath is null)
            // Once we add Android/Browser support, something like ExportFilePopup might need a rework anyways
            throw new NullReferenceException(
                $"{nameof(GetSaveFileTypeAndPath)}() currently does not support platforms that do not support TryGetLocalPath().");

        var extension = Path.GetExtension(localPath);
        var fromProvidedFileType = GetIoFileType(allSupportedExtensions, fileType);

        if (extension == string.Empty)
            return FallbackFileType();

        var fromExtensionType = allSupportedExtensions.SingleOrDefault(i =>
            i.CanSave && i.Extensions.Contains(extension, StringComparer.OrdinalIgnoreCase));

        return fromExtensionType is null
            ? FallbackFileType()
            : (fromExtensionType, localPath);

        // Do not use Path.ChangeExtension, use might choose a file like 'interesting.file.name' where we don't want to change the extension from .name
        (IoFileType type, string path) FallbackFileType() =>
            (fromProvidedFileType, $"{localPath}{fromProvidedFileType.PrimaryExtension}");
    }

    private static IoFileType? GetIoFileType(List<IoFileType> fromTypes, FilePickerFileType fileType)
    {
        foreach (var pattern in fileType.Patterns.Select(x => x.TrimStart('*')))
        {
            var foundType =
                fromTypes.FirstOrDefault(x => x.Extensions.Contains(pattern, StringComparer.OrdinalIgnoreCase));
            if (foundType != null)
                return foundType;
        }

        return null;
    }

    public static List<FilePickerFileType> BuildOpenFilter()
    {
        var any = new FileTypeDialogDataSet(FileTypeDialogDataSet.SetKind.Any).GetFormattedTypes(true);
        return any.ToList();
    }

    public static bool IsRasterFormat(string fileExtension)
    {
        return FileTypes.Any(i =>
            i.Extensions.Contains(fileExtension, StringComparer.OrdinalIgnoreCase) &&
            i.SetKind.HasFlag(FileTypeDialogDataSet.SetKind.Image));
    }
}
