namespace PixiEditor.Helpers.UI;

internal class ExecutionTrigger
{
    public event EventHandler Triggered;

    public void Execute(object sender)
    {
        Triggered?.Invoke(sender, EventArgs.Empty);
    }
}

internal class ExecutionTrigger<T>
{
    public event EventHandler<T> Triggered;

    public void Execute(object sender, T args)
    {
        Triggered?.Invoke(sender, args);
    }
}
