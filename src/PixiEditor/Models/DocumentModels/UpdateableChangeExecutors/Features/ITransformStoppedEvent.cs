namespace PixiEditor.Models.DocumentModels.UpdateableChangeExecutors.Features;

public interface ITransformStoppedEvent : IExecutorFeature
{
    public void OnTransformStopped();
}
