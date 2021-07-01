using PixiEditor.Models.IO;

namespace PixiEditor.Models.Layers
{
    public class ReferenceLayer : Layer
    {
        private double referenceWidth;

        public double ReferenceWidth
        {
            get => referenceWidth;
            set => SetProperty(ref referenceWidth, value);
        }

        private double referenceHeight;

        public double ReferenceHeight
        {
            get => referenceHeight;
            set => SetProperty(ref referenceHeight, value);
        }

        private string path;

        public string Path
        {
            get => path;
            set => SetProperty(ref path, value);
        }

        public ReferenceLayer()
            : this("Reference Layer")
        {
        }

        public ReferenceLayer(string name)
            : base(name)
        {
        }

        public ReferenceLayer(string name, string path)
            : this(name)
        {
            LayerBitmap = Importer.ImportImage(path);
        }

        public void Update()
        {
            LayerBitmap = Importer.ImportImage(path);
        }
    }
}
