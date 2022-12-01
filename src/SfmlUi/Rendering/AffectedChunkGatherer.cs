using ChunkyImageLib;
using ChunkyImageLib.DataHolders;
using PixiEditor.ChangeableDocument;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.ChangeableDocument.ChangeInfos;
using PixiEditor.ChangeableDocument.ChangeInfos.Drawing;
using PixiEditor.ChangeableDocument.ChangeInfos.Properties;
using PixiEditor.ChangeableDocument.ChangeInfos.Root;
using PixiEditor.ChangeableDocument.ChangeInfos.Structure;
using PixiEditor.DrawingApi.Core.Numerics;

namespace SfmlUi.Rendering;
#nullable enable
internal class AffectedChunkGatherer
{
    private readonly DocumentChangeTracker tracker;

    public HashSet<VecI> MainImageChunks { get; private set; } = new();
    public Dictionary<Guid, HashSet<VecI>> ImagePreviewChunks { get; private set; } = new();
    public Dictionary<Guid, HashSet<VecI>> MaskPreviewChunks { get; private set; } = new();

    public AffectedChunkGatherer(DocumentChangeTracker tracker, IReadOnlyList<IChangeInfo?> changes)
    {
        this.tracker = tracker;
        ProcessChanges(changes);
    }

    private void ProcessChanges(IReadOnlyList<IChangeInfo?> changes)
    {
        foreach (var change in changes)
        {
            switch (change)
            {
                case MaskChunks_ChangeInfo info:
                    if (info.Chunks is null)
                        throw new InvalidOperationException("Chunks must not be null");
                    AddToMainImage(info.Chunks);
                    break;
                case LayerImageChunks_ChangeInfo info:
                    if (info.Chunks is null)
                        throw new InvalidOperationException("Chunks must not be null");
                    AddToMainImage(info.Chunks);
                    break;
                case CreateStructureMember_ChangeInfo info:
                    AddAllToMainImage(info.GuidValue);
                    break;
                case DeleteStructureMember_ChangeInfo info:
                    AddWholeCanvasToMainImage();
                    break;
                case MoveStructureMember_ChangeInfo info:
                    AddAllToMainImage(info.GuidValue);
                    break;
                case Size_ChangeInfo:
                    AddWholeCanvasToMainImage();
                    break;
                case StructureMemberMask_ChangeInfo info:
                    AddWholeCanvasToMainImage();
                    break;
                case StructureMemberBlendMode_ChangeInfo info:
                    AddAllToMainImage(info.GuidValue);
                    break;
                case StructureMemberClipToMemberBelow_ChangeInfo info:
                    AddAllToMainImage(info.GuidValue);
                    break;
                case StructureMemberOpacity_ChangeInfo info:
                    AddAllToMainImage(info.GuidValue);
                    break;
                case StructureMemberIsVisible_ChangeInfo info:
                    AddAllToMainImage(info.GuidValue);
                    break;
                case StructureMemberMaskIsVisible_ChangeInfo info:
                    AddAllToMainImage(info.GuidValue, false);
                    break;
            }
        }
    }

    private void AddAllToMainImage(Guid memberGuid, bool useMask = true)
    {
        var member = tracker.Document.FindMember(memberGuid);
        if (member is IReadOnlyLayer layer)
        {
            var chunks = layer.LayerImage.FindAllChunks();
            if (layer.Mask is not null && layer.MaskIsVisible && useMask)
                chunks.IntersectWith(layer.Mask.FindAllChunks());
            AddToMainImage(chunks);
        }
        else
        {
            AddWholeCanvasToMainImage();
        }
    }

    private void AddToMainImage(HashSet<VecI> chunks)
    {
        MainImageChunks.UnionWith(chunks);
    }

    private void AddWholeCanvasToMainImage()
    {
        AddAllChunks(MainImageChunks);
    }

    private void AddAllChunks(HashSet<VecI> chunks)
    {
        VecI size = new(
            (int)Math.Ceiling(tracker.Document.Size.X / (float)ChunkyImage.FullChunkSize),
            (int)Math.Ceiling(tracker.Document.Size.Y / (float)ChunkyImage.FullChunkSize));
        for (int i = 0; i < size.X; i++)
        {
            for (int j = 0; j < size.Y; j++)
            {
                chunks.Add(new(i, j));
            }
        }
    }
}
