using System;

namespace PixiEditor.Helpers
{
    public class ExecutionTrigger<T>
    {
        public event EventHandler<T> Triggered;
        public void Execute(object sender, T args)
        {
            Triggered?.Invoke(sender, args);
        }
    }
}
