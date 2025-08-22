using System.Collections;
using System.Collections.Immutable;
using System.Collections.Specialized;
using System.Runtime.Serialization;

namespace PixiEditor.Models.Structures;

public class ObservableHashSet<T> : ISet<T>, IReadOnlySet<T>, IDeserializationCallback, ISerializable, INotifyCollectionChanged
{
    private readonly HashSet<T> setImplementation;

    public ObservableHashSet()
    {
        setImplementation = new HashSet<T>();
    }

    public ObservableHashSet(IEnumerable<T> collection)
    {
        setImplementation = new HashSet<T>(collection);
    }
    
    public bool Add(T item)
    {
        var isAdded = setImplementation.Add(item);

        if (isAdded)
        {
            CallCollectionChanged(NotifyCollectionChangedAction.Add, item);
        }
        
        return isAdded;
    }

    void ICollection<T>.Add(T item) => Add(item);

    public void Clear()
    {
        setImplementation.Clear();
        CallCollectionChanged(NotifyCollectionChangedAction.Reset, Array.Empty<T>(), setImplementation.ToList());
    }

    public bool Remove(T item)
    {
        var isRemoved = setImplementation.Remove(item);

        if (isRemoved)
        {
            CallCollectionChanged(NotifyCollectionChangedAction.Remove, item);
        }
        
        return isRemoved;
    }

    /// <summary>
    /// Not implemented
    /// </summary>
    /// <exception cref="NotSupportedException">This method is not implemented.</exception>
    public void ExceptWith(IEnumerable<T> other)
    {
        throw new NotSupportedException();
    }

    /// <summary>
    /// Not implemented
    /// </summary>
    /// <exception cref="NotSupportedException">This method is not implemented.</exception>
    public void IntersectWith(IEnumerable<T> other)
    {
        throw new NotSupportedException();
    }

    /// <summary>
    /// Not implemented
    /// </summary>
    /// <exception cref="NotSupportedException">This method is not implemented.</exception>
    public void SymmetricExceptWith(IEnumerable<T> other)
    {
        throw new NotSupportedException();
    }

    public void UnionWith(IEnumerable<T> other)
    {
        var allOther = other.ToImmutableHashSet();
        var addedOnly = allOther.Except(setImplementation);
        
        setImplementation.UnionWith(allOther);
        CallCollectionChanged(
            NotifyCollectionChangedAction.Reset, 
            addedOnly.ToList());
    }

    public void ReplaceBy(IEnumerable<T> other)
    {
        var otherOriginal = other.ToHashSet();
        var original = setImplementation.ToImmutableHashSet();
        var removed = original.Except(otherOriginal);

        setImplementation.Clear();
        setImplementation.UnionWith(otherOriginal);
        CallCollectionChanged(NotifyCollectionChangedAction.Replace, otherOriginal.ToList(), removed.ToList());
    }

    public bool IsProperSubsetOf(IEnumerable<T> other) => setImplementation.IsProperSubsetOf(other);

    public bool IsProperSupersetOf(IEnumerable<T> other) => setImplementation.IsProperSupersetOf(other);

    public bool IsSubsetOf(IEnumerable<T> other) => setImplementation.IsSubsetOf(other);

    public bool IsSupersetOf(IEnumerable<T> other) => setImplementation.IsSupersetOf(other);

    public bool Overlaps(IEnumerable<T> other) => setImplementation.Overlaps(other);

    public bool SetEquals(IEnumerable<T> other) => setImplementation.SetEquals(other);

    public bool Contains(T item) => setImplementation.Contains(item);

    public void CopyTo(T[] array, int arrayIndex) => setImplementation.CopyTo(array, arrayIndex);

    public IEnumerator<T> GetEnumerator() => setImplementation.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)setImplementation).GetEnumerator();

    public int Count => setImplementation.Count;

    bool ICollection<T>.IsReadOnly => ((ISet<T>)setImplementation).IsReadOnly;

    void IDeserializationCallback.OnDeserialization(object? sender) => setImplementation.OnDeserialization(sender);

    void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context) => setImplementation.GetObjectData(info, context);

    private void CallCollectionChanged(NotifyCollectionChangedAction action) =>
        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(action));
    
    private void CallCollectionChanged(NotifyCollectionChangedAction action, IList added, IList removed) =>
        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(action, added, removed));

    private void CallCollectionChanged(NotifyCollectionChangedAction action, IList changed) =>
        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(action, changed));

    private void CallCollectionChanged(NotifyCollectionChangedAction action, T item) =>
        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(action, item));

    public event NotifyCollectionChangedEventHandler? CollectionChanged;
}
