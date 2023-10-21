using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using PixiEditor.AvaloniaUI.Models.Commands.Commands;

namespace PixiEditor.AvaloniaUI.Models.Commands.Evaluators;

internal class IconEvaluator : Evaluator<IImage>
{
    public static IconEvaluator Default { get; } = new CommandNameEvaluator();

    public override IImage? CallEvaluate(Command command, object parameter) =>
        base.CallEvaluate(command, parameter ?? command);

    public static string GetDefaultPath(Command command)
    {
        string path;

        if (command.IconPath != null)
        {
            if (command.IconPath.StartsWith('@'))
            {
                path = command.IconPath[1..];
            }
            else if (command.IconPath.StartsWith('$'))
            {
                path = $"Images/Commands/{command.IconPath[1..].Replace('.', '/')}.png";
            }
            else
            {
                path = $"Images/{command.IconPath}";
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

    [DebuggerDisplay("IconEvaluator.Default")]
    private class CommandNameEvaluator : IconEvaluator
    {
        public static Dictionary<string, Bitmap> images = new();

        public override IImage? CallEvaluate(Command command, object parameter)
        {
            string path = GetDefaultPath(command);

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
    }
}
