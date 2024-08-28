using PixiEditor.DrawingApi.Core.Shaders.Generation.Expressions;

namespace PixiEditor.DrawingApi.Core.Shaders.Generation;

public interface IMultiValueVariable
{
    public ShaderExpressionVariable GetValueAt(int index);
}
