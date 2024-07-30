namespace PixiEditor.Extensions.CommonApi.FlyUI.Events;

public class ElementEventArgs
{
    public object Sender { get; set; } 
    public static ElementEventArgs Empty { get; } = new ElementEventArgs();
}

public class ElementEventArgs<TEventArgs> : ElementEventArgs where TEventArgs : ElementEventArgs
{
    public static new ElementEventArgs<TEventArgs> Empty { get; } = new ElementEventArgs<TEventArgs>();
}
