using Avalonia.Input;

namespace PixiEditor.Helpers.Constants;

public static class ClipboardDataFormats
{
    public static readonly DataFormat<byte[]>[] PngFormats = [
        DataFormat.CreateBytesPlatformFormat("PNG"), DataFormat.CreateBytesPlatformFormat("image/png"), DataFormat.CreateBytesPlatformFormat("public.png") ];
    public static readonly DataFormat<byte[]> LayerIdList = DataFormat.CreateBytesApplicationFormat("PixiEditor.LayerIdList");
    public static readonly DataFormat<byte[]> PositionFormat = DataFormat.CreateBytesApplicationFormat("PixiEditor.Position");
    public static readonly DataFormat<byte[]> DocumentFormat = DataFormat.CreateBytesApplicationFormat("PixiEditor.Document");
    public static readonly DataFormat<byte[]> NodeIdList = DataFormat.CreateBytesApplicationFormat("PixiEditor.NodeIdList");
    public static readonly DataFormat<byte[]> CelIdList = DataFormat.CreateBytesApplicationFormat("PixiEditor.CelIdList");
    public static readonly DataFormat<byte[]> PixiVectorData = DataFormat.CreateBytesApplicationFormat("PixiEditor.VectorData");
    public static readonly DataFormat<byte[]> UriList = DataFormat.CreateBytesPlatformFormat("text/uri-list");
    public static readonly DataFormat<byte[]> HadSelectionFormat = DataFormat.CreateBytesApplicationFormat("PixiEditor.HadSelection");
}
