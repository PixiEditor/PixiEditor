using System;
using System.Threading.Tasks;
using PixiEditor.DrawingApi.Core.Surfaces;

namespace PixiEditor.DrawingApi.Core;

public interface IRenderingServer
{
    public Action<Action> Invoke { get; }
}
