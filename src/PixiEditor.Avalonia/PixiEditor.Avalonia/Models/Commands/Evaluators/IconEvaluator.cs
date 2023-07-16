using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Avalonia.Media.Imaging;
using PixiEditor.Models.Commands.Commands;

namespace PixiEditor.Models.Commands.Evaluators;

internal class IconEvaluator : Evaluator<Bitmap>
{
    public static IconEvaluator Default { get; } = new CommandNameEvaluator();

    public override Bitmap CallEvaluate(Command command, object parameter) =>
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

        path = path.ToLower();

        if (path.StartsWith("/"))
        {
            path = path[1..];
        }

        return path;
    }

    [DebuggerDisplay("IconEvaluator.Default")]
    private class CommandNameEvaluator : IconEvaluator
    {
        public static string[] resources = GetResourceNames();

        public static Dictionary<string, Bitmap> images = new();

        public override Bitmap CallEvaluate(Command command, object parameter)
        {
            string path = GetDefaultPath(command);

            if (resources.Contains(path))
            {
                var image = images.GetValueOrDefault(path);

                if (image == null)
                {
                    using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"pack://application:,,,/{path}")!;
                    image = new Bitmap(stream);
                    images.Add(path, image);
                }

                return image;
            }

            return null;
        }

        private static string[] GetResourceNames()
        {
            var assembly = Assembly.GetExecutingAssembly();
            string resName = assembly.GetName().Name + ".g.resources";
            using var stream = assembly.GetManifestResourceStream(resName);
            using var reader = new System.Resources.ResourceReader(stream);

            return reader.Cast<DictionaryEntry>().Select(entry =>
                (string)entry.Key).ToArray();
        }
    }
}
