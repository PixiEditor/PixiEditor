using PixiEditor.Extensions.Common.Localization;

namespace PixiEditor.Models.Files;

internal class Mp4FileType : VideoFileType
{
    public override string[] Extensions { get; } = { ".mp4" };
    public override string DisplayName => new LocalizedString("MP4_FILE");
}
