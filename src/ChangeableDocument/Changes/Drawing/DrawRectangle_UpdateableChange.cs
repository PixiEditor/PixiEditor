using ChangeableDocument.Changeables;
using ChangeableDocument.ChangeInfos;
using ChunkyImageLib;
using ChunkyImageLib.DataHolders;

namespace ChangeableDocument.Changes.Drawing
{
    internal class DrawRectangle_UpdateableChange : IUpdateableChange
    {
        private Guid layerGuid;
        private ShapeData rect;
        private ChunkStorage? storedChunks;
        public DrawRectangle_UpdateableChange(Guid layerGuid, ShapeData rectangle)
        {
            this.layerGuid = layerGuid;
            this.rect = rectangle;
        }

        public void Initialize(Document target) { }

        public void Update(ShapeData updatedRectangle)
        {
            rect = updatedRectangle;
        }

        public IChangeInfo? ApplyTemporarily(Document target)
        {
            Layer layer = (Layer)target.FindMemberOrThrow(layerGuid);
            var oldChunks = layer.LayerImage.FindAffectedChunks();
            layer.LayerImage.CancelChanges();
            layer.LayerImage.DrawRectangle(rect);
            var newChunks = layer.LayerImage.FindAffectedChunks();
            newChunks.UnionWith(oldChunks);
            return new LayerImageChunks_ChangeInfo()
            {
                Chunks = newChunks,
                LayerGuid = layerGuid
            };
        }

        public IChangeInfo? Apply(Document target)
        {
            Layer layer = (Layer)target.FindMemberOrThrow(layerGuid);
            var changes = ApplyTemporarily(target);
            storedChunks = new ChunkStorage(layer.LayerImage, layer.LayerImage.FindAffectedChunks());
            layer.LayerImage.CommitChanges();
            return changes;
        }

        public IChangeInfo? Revert(Document target)
        {
            if (storedChunks == null)
                throw new Exception("No stored chunks to revert to");
            Layer layer = (Layer)target.FindMemberOrThrow(layerGuid);
            storedChunks.ApplyChunksToImage(layer.LayerImage);
            storedChunks.Dispose();
            storedChunks = null;
            var changes = new LayerImageChunks_ChangeInfo()
            {
                Chunks = layer.LayerImage.FindAffectedChunks(),
                LayerGuid = layerGuid,
            };
            layer.LayerImage.CommitChanges();
            return changes;
        }
    }
}
