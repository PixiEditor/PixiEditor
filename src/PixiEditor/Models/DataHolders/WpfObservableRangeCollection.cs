using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Data;

namespace PixiEditor.Models.DataHolders;

internal class WpfObservableRangeCollection<T> : RangeObservableCollection<T>
{
    public bool SuppressNotify { get; set; } = false;
    DeferredEventsCollection _deferredEvents;

    public WpfObservableRangeCollection()
    {
    }

    public WpfObservableRangeCollection(IEnumerable<T> collection) : base(collection)
    {
    }

    public WpfObservableRangeCollection(List<T> list) : base(list)
    {
    }


    /// <summary>
    /// Raise CollectionChanged event to any listeners.
    /// Properties/methods modifying this ObservableCollection will raise
    /// a collection changed event through this virtual method.
    /// </summary>
    /// <remarks>
    /// When overriding this method, either call its base implementation
    /// or call <see cref="BlockReentrancy"/> to guard against reentrant collection changes.
    /// </remarks>
    protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        if (SuppressNotify) return;
#pragma warning disable SA1312 // Variable names should begin with lower-case letter
        var _deferredEvents = (ICollection<NotifyCollectionChangedEventArgs>)typeof(RangeObservableCollection<T>)
#pragma warning restore SA1312 // Variable names should begin with lower-case letter
            .GetField("_deferredEvents", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(this);
        if (_deferredEvents != null)
        {
            _deferredEvents.Add(e);
            return;
        }

        foreach (var handler in GetHandlers())
        {
            if (IsRange(e) && handler.Target is CollectionView cv)
                cv.Refresh();
            else
                handler(this, e);
        }
    }

    protected override IDisposable DeferEvents() => new DeferredEventsCollection(this);

    bool IsRange(NotifyCollectionChangedEventArgs e) => e.NewItems?.Count > 1 || e.OldItems?.Count > 1;

    IEnumerable<NotifyCollectionChangedEventHandler> GetHandlers()
    {
        var info = typeof(ObservableCollection<T>).GetField(
            nameof(CollectionChanged),
            BindingFlags.Instance | BindingFlags.NonPublic);
        var @event = (MulticastDelegate)info.GetValue(this);
        return @event?.GetInvocationList()
                   .Cast<NotifyCollectionChangedEventHandler>()
                   .Distinct()
               ?? Enumerable.Empty<NotifyCollectionChangedEventHandler>();
    }

    class DeferredEventsCollection : List<NotifyCollectionChangedEventArgs>, IDisposable
    {
        private readonly WpfObservableRangeCollection<T> _collection;

        public DeferredEventsCollection(WpfObservableRangeCollection<T> collection)
        {
            Debug.Assert(collection != null);
            Debug.Assert(collection._deferredEvents == null);
            _collection = collection;
            _collection._deferredEvents = this;
        }

        public void Dispose()
        {
            _collection._deferredEvents = null;

            var handlers = _collection
                .GetHandlers()
                .ToLookup(h => h.Target is CollectionView);

            foreach (var handler in handlers[false])
            {
                foreach (var e in this)
                {
                    handler(_collection, e);
                }
            }

            foreach (var cv in handlers[true]
                         .Select(h => h.Target)
                         .Cast<CollectionView>()
                         .Distinct())
            {
                cv.Refresh();
            }
        }
    }
}
