using System.Globalization;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using PixiEditor.AvaloniaUI.Models.Commands.Attributes.Evaluators;
using PixiEditor.AvaloniaUI.Models.Commands.Commands;
using PixiEditor.AvaloniaUI.Models.Commands.Search;

namespace PixiEditor.AvaloniaUI.Helpers;

internal static class IconEvaluators
{
    public static Dictionary<string, Bitmap> images = new();

    [Evaluator.Icon("PixiEditor.BitmapIcon")]
    public static IImage GetBitmapIcon(object parameter)
    {
        string path = GetDefaultPath(parameter as Command);

        var image = images.GetValueOrDefault(path);
        if (image is not null)
            return image;
            
        Uri uri = new($"avares://{Assembly.GetExecutingAssembly().GetName().Name}/{path}");
        if (!AssetLoader.Exists(uri))
            return null;
            
        image = new Bitmap(AssetLoader.Open(uri));
        images.Add(path, image);

        return image;
    }
    
    public static string GetDefaultPath(Command command)
    {
        string path;

        if (command.Icon != null)
        {
            if (command.Icon.StartsWith('@'))
            {
                path = command.Icon[1..];
            }
            else if (command.Icon.StartsWith('$'))
            {
                path = $"Images/Commands/{command.Icon[1..].Replace('.', '/')}.png";
            }
            else
            {
                path = $"Images/{command.Icon}";
            }
        }
        else
        {
            path = $"Images/Commands/{command.InternalName.Replace('.', '/')}.png";
        }

        if (path.StartsWith("/"))
        {
            path = path[1..];
        }

        return path;
    }
}
