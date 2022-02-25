using ChunkyImageLib.Operations;
using SkiaSharp;

namespace ChunkyImageLib
{
    public class ChunkyImage
    {
        //const int chunkSize = 32;
        private Queue<IOperation> queuedOperations = new Queue<IOperation>();

        private ImageData image;
        private SKSurface imageSurface;

        private ImageData pendingImage;
        private SKSurface pendingImageSurface;

        public ChunkyImage(int width, int height)
        {
            image = new ImageData(width, height, SKColorType.RgbaF16);
            pendingImage = new ImageData(width, height, SKColorType.RgbaF16);
            imageSurface = image.CreateSKSurface();
            pendingImageSurface = image.CreateSKSurface();
        }

        public ImageData GetCurrentImageData()
        {
            ProcessQueue();
            return pendingImage;
        }

        public void DrawRectangle(int x, int y, int width, int height)
        {
            queuedOperations.Enqueue(new RectangleOperation(x, y, width, height));
        }

        public void CancelChanges()
        {
            queuedOperations.Clear();
            image.CopyTo(pendingImage);
        }

        public void CommitChanges()
        {
            ProcessQueue();
            pendingImage.CopyTo(image);
        }

        private void ProcessQueue()
        {
            foreach (var operation in queuedOperations)
            {
                if (operation is RectangleOperation rect)
                {
                    using SKPaint black = new() { Color = SKColors.Black };
                    pendingImageSurface.Canvas.DrawRect(rect.X, rect.Y, rect.Width, rect.Height, black);
                }
            }
            queuedOperations.Clear();
        }
        /*
        private List<(int, int)> GetAffectedChunks(IOperation operation)
        {
            if (operation is RectangleOperation rect)
                return GetAffectedChunks(rect);
            return new List<(int, int)>();
        }

        private List<(int, int)> GetAffectedChunks(RectangleOperation rect)
        {
            int startX = (int)Math.Floor(rect.X / (float)chunkSize);
            int startY = (int)Math.Floor(rect.Y / (float)chunkSize);
            int endX = (int)Math.Floor((rect.X + rect.Width - 1) / (float)chunkSize);
            int endY = (int)Math.Floor((rect.Y + rect.Height - 1) / (float)chunkSize);
            List<(int, int)> chunks = new();
            for (int i = startX; i <= endX; i++)
            {
                chunks.Add((i, startY));
                chunks.Add((i, endY));
            }
            for (int i = startY + 1; i < endY; i++)
            {
                chunks.Add((startX, i));
                chunks.Add((endX, i));
            }
            return chunks;
        }*/
    }
}
