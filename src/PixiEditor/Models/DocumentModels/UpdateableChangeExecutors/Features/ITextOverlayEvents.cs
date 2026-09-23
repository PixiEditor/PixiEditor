using Drawie.Backend.Core.Text;

namespace PixiEditor.Models.DocumentModels.UpdateableChangeExecutors.Features;

public interface ITextOverlayEvents :IExecutorFeature
{
    public void OnTextChanged(RichText text);
}
