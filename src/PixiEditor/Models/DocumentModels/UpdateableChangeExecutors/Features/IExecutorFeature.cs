namespace PixiEditor.Models.DocumentModels.UpdateableChangeExecutors.Features;

public interface IExecutorFeature
{
    public bool IsFeatureEnabled<T>();
}
