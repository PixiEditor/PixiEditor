using Avalonia.Controls.Embedding;

namespace PixiEditor.Engine.AvaloniaPlatform;

public sealed class PixiEngineTopLevel : EmbeddableControlRoot
{
    internal PixiEngineTopLevelImpl Impl { get; }
    
    internal PixiEngineTopLevel(PixiEngineTopLevelImpl impl)
        : base(impl)
        => Impl = impl;
}
