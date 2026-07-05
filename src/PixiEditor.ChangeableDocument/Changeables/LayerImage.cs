using Drawie.Numerics;
using PixiEditor.Common;

namespace PixiEditor.ChangeableDocument.Changeables;

public class LayerImage : IDisposable, ICloneable, ICacheable
{
    public ChunkyImage Main { get; set; }
    public List<ChunkyImage>? Additional { get; set; }

    public LayerImage(VecI startSize)
    {
        Main = new ChunkyImage(startSize);
        Additional = new List<ChunkyImage>();
    }

    public LayerImage(ChunkyImage img)
    {
        Main = img;
        Additional = new List<ChunkyImage>();
    }

    public void Dispose()
    {
        Main.Dispose();
        if (Additional != null)
        {
            foreach (var image in Additional)
            {
                image.Dispose();
            }
        }
    }

    public object Clone()
    {
        return new LayerImage((ChunkyImage)Main.Clone())
        {
            Additional = Additional?.Select(x => (ChunkyImage)x.Clone()).ToList()
        };
    }

    public int GetCacheHash()
    {
        HashCode code = new HashCode();
        int hash = Main.GetCacheHash();
        code.Add(hash);
        if (Additional != null)
        {
            foreach (var image in Additional)
            {
                hash = image.GetCacheHash();
                code.Add(hash);
            }
        }
        return code.ToHashCode();
    }

    public int CreateAdditionalDataLayer()
    {
        var newImage = new ChunkyImage(Main.LatestSize);
        Additional ??= new List<ChunkyImage>();
        Additional.Add(newImage);
        return Additional.Count - 1;
    }

    public void CommitChanges()
    {
        Main.CommitChanges();
        if (Additional != null)
        {
            foreach (var image in Additional)
            {
                image.CommitChanges();
            }
        }
    }
}
