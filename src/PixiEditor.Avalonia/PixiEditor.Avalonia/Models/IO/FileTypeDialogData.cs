using System.Collections.Generic;
using System.Linq;
using PixiEditor.Models.Enums;
using PixiEditor.Models.Files;

namespace PixiEditor.Models.IO;

internal class FileTypeDialogData
{
    public FileType FileType { get; set; }

    /// <summary>
    /// Gets or sets file type extensions e.g. {jpg,jpeg}
    /// </summary>
    public List<string> Extensions { get; set; }

    /// <summary>
    /// Gets file type's main extensions e.g. jpeg
    /// </summary>
    public string PrimaryExtension { get => Extensions.FirstOrDefault(); }

    /// <summary>
    /// Gets or sets name displayed before extension e.g. JPEG Files
    /// </summary>
    public string DisplayName { get; set; }

    public FileTypeDialogData(FileType fileType)
    {
        FileType = fileType;
        Extensions = new List<string>();
        Extensions.Add("." + FileType.ToString().ToLower());
        if (FileType == FileType.Jpeg)
            Extensions.Add(".jpg");

        if (fileType == FileType.Pixi)
            DisplayName = "PixiEditor Files";
        else
            DisplayName = FileType.ToString() + " Images";
    }

    public string SaveFilter
    {
        get { return DisplayName + "|" + GetExtensionFormattedForDialog(PrimaryExtension); }
    }

    public string ExtensionsFormattedForDialog
    {
        get { return string.Join(";", Extensions.Select(i => GetExtensionFormattedForDialog(i))); }
    }

    string GetExtensionFormattedForDialog(string extension)
    {
        return "*" + extension;
    }
}
