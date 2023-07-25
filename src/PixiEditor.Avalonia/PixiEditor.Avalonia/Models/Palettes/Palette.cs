#nullable enable
using System.Collections.Generic;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using PixiEditor.Extensions.Palettes;
using PixiEditor.Helpers;
namespace PixiEditor.Models.DataHolders.Palettes;

internal class Palette : ObservableObject, IPalette
{
    private string _name = "";

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }
    public List<PaletteColor> Colors { get; set; }

    private string? fileName;

    public string? FileName
    {
        get => fileName;
        set
        {
            fileName = ReplaceInvalidChars(value);
            OnPropertyChanged(nameof(FileName));
        }
    }

    public bool IsFavourite { get; set; }

    public PaletteListDataSource Source { get; }

    public Palette(string name, List<PaletteColor> colors, string? fileName, PaletteListDataSource source)
    {
        Name = name;
        Colors = colors;
        FileName = fileName;
        Source = source;
    }

    public static string? ReplaceInvalidChars(string? filename)
    {
        return filename == null ? null : string.Join("_", filename.Split(Path.GetInvalidFileNameChars()));
    }
}
