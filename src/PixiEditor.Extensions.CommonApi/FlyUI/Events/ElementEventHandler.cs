namespace PixiEditor.Extensions.CommonApi.FlyUI.Events;

public delegate void ElementEventHandler(ElementEventArgs args);

public delegate void ElementEventHandler<in TEventArgs>(TEventArgs args)
    where TEventArgs : ElementEventArgs<TEventArgs>;
