using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PixiEditor.Models.Commands.Commands;

namespace PixiEditor.Models.Commands.Evaluators;

internal class IconEvaluator : Evaluator<ImageSource>
{
    public static IconEvaluator Default { get; } = new CommandNameEvaluator();

    public override ImageSource CallEvaluate(Command command, object parameter) =>
        base.CallEvaluate(command, parameter ?? command);

    [DebuggerDisplay("IconEvaluator.Default")]
    private class CommandNameEvaluator : IconEvaluator
    {
        public static string[] resources = GetResourceNames();

        public static Dictionary<string, BitmapImage> images = new();

        public override ImageSource CallEvaluate(Command command, object parameter)
        {
            string path;

            if (command.IconPath != null)
            {
                if (command.IconPath.StartsWith('@'))
                {
                    path = command.IconPath[1..];
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

            if (resources.Contains(path))
            {
                var image = images.GetValueOrDefault(path);

                if (image == null)
                {
                    image = new BitmapImage(new($"pack://application:,,,/{path}"));
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
