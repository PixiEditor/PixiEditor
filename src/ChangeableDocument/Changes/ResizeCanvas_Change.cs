using ChangeableDocument.Changeables;
using ChangeableDocument.ChangeInfos;
using ChunkyImageLib;
using ChunkyImageLib.DataHolders;

namespace ChangeableDocument.Changes
{
    internal class ResizeCanvas_Change : IChange
    {
        private Vector2i originalSize;
        private Dictionary<Guid, CommittedChunkStorage> deletedChunks = new();
        private CommittedChunkStorage? selectionChunkStorage;
        private Vector2i newSize;
        public ResizeCanvas_Change(Vector2i size)
        {
            newSize = size;
        }
        public void Initialize(Document target)
        {
            originalSize = target.Size;
        }

        private void ForEachLayer(Folder folder, Action<Layer> action)
        {
            foreach (var child in folder.Children)
            {
                if (child is Layer layer)
                {
                    action(layer);
                }
                else if (child is Folder innerFolder)
                    ForEachLayer(innerFolder, action);
            }
        }

        public IChangeInfo? Apply(Document target, out bool ignoreInUndo)
        {
            if (originalSize == newSize)
            {
                ignoreInUndo = true;
                return null;
            }

            target.Size = newSize;

            ForEachLayer(target.StructureRoot, (layer) =>
            {
                layer.LayerImage.Resize(newSize);
                deletedChunks.Add(layer.GuidValue, new CommittedChunkStorage(layer.LayerImage, layer.LayerImage.FindAffectedChunks()));
                layer.LayerImage.CommitChanges();
            });

            target.Selection.SelectionImage.Resize(newSize);
            selectionChunkStorage = new(target.Selection.SelectionImage, target.Selection.SelectionImage.FindAffectedChunks());
            target.Selection.SelectionImage.CommitChanges();

            ignoreInUndo = false;
            return new Size_ChangeInfo();
        }

        public IChangeInfo? Revert(Document target)
        {
            if (originalSize == newSize)
                return null;

            target.Size = originalSize;
            ForEachLayer(target.StructureRoot, (layer) =>
            {
                layer.LayerImage.Resize(originalSize);
                deletedChunks[layer.GuidValue].ApplyChunksToImage(layer.LayerImage);
                layer.LayerImage.CommitChanges();
            });

            target.Selection.SelectionImage.Resize(originalSize);
            selectionChunkStorage!.ApplyChunksToImage(target.Selection.SelectionImage);
            target.Selection.SelectionImage.CommitChanges();
            selectionChunkStorage.Dispose();
            selectionChunkStorage = null;

            foreach (var stored in deletedChunks)
                stored.Value.Dispose();
            deletedChunks = new();

            return new Size_ChangeInfo();
        }

        public void Dispose()
        {
            foreach (var layer in deletedChunks)
                layer.Value.Dispose();
            selectionChunkStorage?.Dispose();
        }
    }
}
