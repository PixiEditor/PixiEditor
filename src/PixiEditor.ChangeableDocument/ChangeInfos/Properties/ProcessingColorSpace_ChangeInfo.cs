using Drawie.Backend.Core.Surfaces.ImageData;

namespace PixiEditor.ChangeableDocument.ChangeInfos.Properties;

public record ProcessingColorSpace_ChangeInfo(ColorSpace NewColorSpace) : IChangeInfo;
