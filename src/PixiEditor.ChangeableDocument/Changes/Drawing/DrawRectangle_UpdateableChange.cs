using ChunkyImageLib;
using ChunkyImageLib.DataHolders;
using PixiEditor.ChangeableDocument.Changeables;
using PixiEditor.ChangeableDocument.ChangeInfos;

namespace PixiEditor.ChangeableDocument.Changes.Drawing
{
    internal class DrawRectangle_UpdateableChange : UpdateableChange
    {
        private Guid layerGuid;
        private ShapeData rect;
        private CommittedChunkStorage? storedChunks;
        public DrawRectangle_UpdateableChange(Guid layerGuid, ShapeData rectangle)
        {
            this.layerGuid = layerGuid;
            this.rect = rectangle;
        }

        public override void Initialize(Document target) { }

        public void Update(ShapeData updatedRectangle)
        {
            rect = updatedRectangle;
        }

        public override IChangeInfo? ApplyTemporarily(Document target)
        {
            Layer layer = (Layer)target.FindMemberOrThrow(layerGuid);
            var oldChunks = layer.LayerImage.FindAffectedChunks();
            layer.LayerImage.CancelChanges();
            if (!target.Selection.IsEmptyAndInactive)
                layer.LayerImage.ApplyRasterClip(target.Selection.SelectionImage);
            layer.LayerImage.DrawRectangle(rect);
            var newChunks = layer.LayerImage.FindAffectedChunks();
            newChunks.UnionWith(oldChunks);
            return new LayerImageChunks_ChangeInfo()
            {
                Chunks = newChunks,
                LayerGuid = layerGuid
            };
        }

        public override IChangeInfo? Apply(Document target, out bool ignoreInUndo)
        {
            Layer layer = (Layer)target.FindMemberOrThrow(layerGuid);
            var changes = ApplyTemporarily(target);
            storedChunks = new CommittedChunkStorage(layer.LayerImage, ((LayerImageChunks_ChangeInfo)changes!).Chunks!);
            layer.LayerImage.CommitChanges();

            ignoreInUndo = false;
            return changes;
        }

        public override IChangeInfo? Revert(Document target)
        {
            Layer layer = (Layer)target.FindMemberOrThrow(layerGuid);
            storedChunks!.ApplyChunksToImage(layer.LayerImage);
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

        public override void Dispose()
        {
            storedChunks?.Dispose();
        }
    }
}
