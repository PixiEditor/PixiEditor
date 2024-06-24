using Avalonia.Media;
using PixiEditor.AvaloniaUI.Models.IO;
using PixiEditor.AvaloniaUI.ViewModels.Document;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.Numerics;

namespace PixiEditor.AvaloniaUI.Models.Files;

internal class PixiFileType : IoFileType
{
    public static PixiFileType PixiFile { get; } = new PixiFileType();
    public override string DisplayName => new LocalizedString("PIXI_FILE");
    public override string[] Extensions => new[] { ".pixi" };

    public override SolidColorBrush EditorColor { get;  } = new SolidColorBrush(new Color(255, 226, 1, 45));

    public override SaveResult TrySave(string pathWithExtension, DocumentViewModel document, VecI? exportSize = null)
    {
        try
        {
            Parser.PixiParser.Serialize(document.ToSerializable(), pathWithExtension);
        }
        catch (UnauthorizedAccessException e)
        {
            return SaveResult.SecurityError;
        }
        catch (IOException)
        {
            return SaveResult.IoError;
        }
        catch
        {
            return SaveResult.UnknownError;
        }

        return SaveResult.Success;
    }
}
