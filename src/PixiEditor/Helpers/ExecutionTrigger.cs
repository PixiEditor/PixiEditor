namespace PixiEditor.Helpers;

internal class ExecutionTrigger<T>
{
    public event EventHandler<T> Triggered;
    public void Execute(object sender, T args)
    {
        Triggered?.Invoke(sender, args);
    }
}
