using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using PixiEditor.Extensions.IO;

namespace PixiEditor.AvaloniaUI.Models.AppExtensions.Services;

public static class FileFilterExtensions
{
    public static FilePickerOpenOptions ToAvaloniaFileFilters(this FileFilter filter)
    {
        FilePickerOpenOptions options = new();
        var avaloniaFilters = new List<FilePickerFileType>();
        foreach (var filterItem in filter.Filters)
        {
            avaloniaFilters.Add(new FilePickerFileType(filterItem.Name) { Patterns = filterItem.Extensions });
        }

        options.FileTypeFilter = avaloniaFilters;
        return options;
    }
}
