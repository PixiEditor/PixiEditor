using Avalonia.Reactive;

namespace PixiEditor.UI.Common.Extensions;

public static class ObservableSubscribeExtensions
{
    public static void Subscribe<T>(this IObservable<T> observable, Action<T> onNext)
    {
        _ = observable.Subscribe(new AnonymousObserver<T>(onNext));
    }
}
