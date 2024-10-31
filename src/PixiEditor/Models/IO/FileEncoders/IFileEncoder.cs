using System.IO;
using System.Threading.Tasks;
using ChunkyImageLib;
using Drawie.Backend.Core;

namespace PixiEditor.Models.IO.FileEncoders;

public interface IFileEncoder
{
    public bool SupportsTransparency { get; }
    public Task SaveAsync(Stream stream, Surface bitmap);
    public void Save(Stream stream, Surface bitmap);
}
