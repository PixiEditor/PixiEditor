namespace PixiEditor.ChangeableDocument.ChangeInfos.Vectors;

    public record VectorShape_ChangeInfo(Guid LayerId, AffectedArea Affected, bool UpdatePreviews = true) : IChangeInfo;
