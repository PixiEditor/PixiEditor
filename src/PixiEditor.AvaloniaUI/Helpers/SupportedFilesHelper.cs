using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia.Platform.Storage;
using PixiEditor.AvaloniaUI.Models.Files;
using PixiEditor.AvaloniaUI.Models.IO;

namespace PixiEditor.AvaloniaUI.Helpers;

internal class SupportedFilesHelper
{
    static Dictionary<FileType, FileTypeDialogData> fileTypeDialogsData;
    static List<FileTypeDialogData> allFileTypeDialogsData;
    public static string[] AllSupportedExtensions { get; private set; }
    public static string[] PrimaryExtensions { get; private set; }

    static SupportedFilesHelper()
    {
        fileTypeDialogsData = new Dictionary<FileType, FileTypeDialogData>();
        allFileTypeDialogsData = new List<FileTypeDialogData>();

        var allFormats = Enum.GetValues(typeof(FileType)).Cast<FileType>().ToList();

        foreach (var format in allFormats)
        {
            var fileTypeDialogData = new FileTypeDialogData(format);
            if (format != FileType.Unset)
                fileTypeDialogsData[format] = fileTypeDialogData;

            allFileTypeDialogsData.Add(fileTypeDialogData);
        }

        AllSupportedExtensions = fileTypeDialogsData.SelectMany(i => i.Value.Extensions).ToArray();
        PrimaryExtensions = fileTypeDialogsData.Select(i => i.Value.PrimaryExtension).ToArray();
    }

    public static FileTypeDialogData GetFileTypeDialogData(FileType type)
    {
        return allFileTypeDialogsData.Where(i => i.FileType == type).Single();
    }

    public static string FixFileExtension(string pathWithOrWithoutExtension, FileType requestedType)
    {
        if (requestedType == FileType.Unset)
            throw new ArgumentException("A valid filetype is required", nameof(requestedType));

        var typeFromPath = SupportedFilesHelper.ParseImageFormat(Path.GetExtension(pathWithOrWithoutExtension));
        if (typeFromPath != FileType.Unset && typeFromPath == requestedType)
            return pathWithOrWithoutExtension;
        return AppendExtension(pathWithOrWithoutExtension, SupportedFilesHelper.GetFileTypeDialogData(requestedType));
    }

    public static string AppendExtension(string path, FileTypeDialogData data)
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
        return AllSupportedExtensions.Contains(fileExtension);
    }
    public static FileType ParseImageFormat(string extension)
    {
        var allExts = fileTypeDialogsData.Values.ToList();
        var fileData = allExts.Where(i => i.Extensions.Contains(extension)).SingleOrDefault();
        if (fileData != null)
            return fileData.FileType;
        return FileType.Unset;
    }

    public static List<FileTypeDialogData> GetAllSupportedFileTypes(bool includePixi)
    {
        var allExts = fileTypeDialogsData.Values.ToList();
        if (!includePixi)
            allExts.RemoveAll(item => item.FileType == FileType.Pixi);
        return allExts;
    }

    public static List<FilePickerFileType> BuildSaveFilter(bool includePixi)
    {
        var allSupportedExtensions = GetAllSupportedFileTypes(includePixi);
        var filter = allSupportedExtensions.Select(i => i.SaveFilter).ToList();

        return filter;
    }

    public static FileType GetSaveFileType(bool includePixi, IStorageFile file)
    {
        var allSupportedExtensions = GetAllSupportedFileTypes(includePixi);

        if (file is null)
            return FileType.Unset;

        string extension = Path.GetExtension(file.Path.LocalPath);
        return allSupportedExtensions.Single(i => i.Extensions.Contains(extension)).FileType;
    }

    public static List<FilePickerFileType> BuildOpenFilter()
    {
        var any = new FileTypeDialogDataSet(FileTypeDialogDataSet.SetKind.Any).GetFormattedTypes(true);
        return any.ToList();
    }
}
