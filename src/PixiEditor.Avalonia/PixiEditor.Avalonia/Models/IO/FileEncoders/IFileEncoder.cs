using System.IO;
using System.Threading.Tasks;
using ChunkyImageLib;

namespace PixiEditor.Models.IO.FileEncoders;

public interface IFileEncoder
{
    public bool SupportsTransparency { get; }
    public Task Save(Stream stream, Surface bitmap);
}
