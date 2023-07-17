using System.Collections.Generic;
using System.IO;
using System.Linq;
using PixiEditor.Models.Enums;
using PixiEditor.Models.Files;
using PixiEditor.Models.IO;

namespace PixiEditor.Helpers;

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

    public static bool IsSupportedFile(string path)
    {
        var ext = Path.GetExtension(path.ToLower());
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

    public static string BuildSaveFilter(bool includePixi)
    {
        var allSupportedExtensions = GetAllSupportedFileTypes(includePixi);
        var filter = string.Join("|", allSupportedExtensions.Select(i => i.SaveFilter));

        return filter;
    }

    public static FileType GetSaveFileTypeFromFilterIndex(bool includePixi, int filterIndex)
    {
        var allSupportedExtensions = GetAllSupportedFileTypes(includePixi);
        //filter index starts at 1 for some reason
        int index = filterIndex - 1;
        if (allSupportedExtensions.Count <= index)
            return FileType.Unset;
        return allSupportedExtensions[index].FileType;
    }

    public static string BuildOpenFilter()
    {
        var any = new FileTypeDialogDataSet(FileTypeDialogDataSet.SetKind.Any).GetFormattedTypes();
        var pixi = new FileTypeDialogDataSet(FileTypeDialogDataSet.SetKind.Pixi).GetFormattedTypes();
        var images = new FileTypeDialogDataSet(FileTypeDialogDataSet.SetKind.Images).GetFormattedTypes();

        var filter = any + "|" + pixi + "|" + images;
        return filter;
    }
}
