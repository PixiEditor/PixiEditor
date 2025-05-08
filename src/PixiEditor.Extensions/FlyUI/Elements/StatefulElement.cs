using System.Collections.Immutable;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using PixiEditor.Extensions.CommonApi.FlyUI;
using PixiEditor.Extensions.CommonApi.FlyUI.State;

namespace PixiEditor.Extensions.FlyUI.Elements;

public abstract class StatefulElement<TState> : LayoutElement, /*IPropertyDeserializable,*/
    IStatefulElement<Control, TState> where TState : IState<Control>
{
    private TState? _state;
    private ContentPresenter _presenter = null!;
    private ILayoutElement<Control> _content = null!;

    IState<Control> IStatefulElement<Control>.State
    {
        get
        {
            if (_state == null)
            {
                _state = CreateState();
                _state.StateChanged += () =>
                {
                    var newState = State.Build();
                    ContentFromLayout(newState);
                };
            }

            return _state;
        }
    }

    public TState State => (TState)((IStatefulElement<Control>)this).State;

    // TODO: Move actual Avalonia implementation to PixiEditor itself.
    public override Control BuildNative()
    {
        CreateNativeControl();
        return _presenter;
    }

    protected override Control CreateNativeControl()
    {
        _presenter ??= new ContentPresenter();
        _content = State.Build();
        Control control = _content.BuildNative();

        SubscribeBasicEvents(control);

        _presenter.Content = control;

        return control;
    }

    public abstract TState CreateState();

    // TODO: Maybe we don't have to redraw whole tree if parent changed, just detach from old parent and attach to new one?
    // React seems not to do that, so there might be a reason for that, however, it would be good to check out the topic more
    public void ContentFromLayout(ILayoutElement<Control> newTree)
    {
        PerformDiff(_content, newTree);
    }

    private void PerformDiff(ILayoutElement<Control> oldNode, ILayoutElement<Control> newNode,
        IChildHost? parent = null)
    {
        // Check if the node types are the same
        bool isSameType = oldNode.GetType() == newNode.GetType();

        if (isSameType)
        {
            ApplyProperties(newNode, oldNode);
        }
        else
        {
            // Replace the entire node if the types are different
            parent?.RemoveChild(oldNode);
            oldNode = newNode;
            parent?.AddChild(oldNode);
            return;
        }

        // Check if the node supports children
        if (oldNode is IChildHost oldDeserializable && newNode is IChildHost newDeserializable)
        {
            // Perform diff for children
            using var oldChildren = oldDeserializable.GetEnumerator();
            using var newChildren = newDeserializable.GetEnumerator();

            while (oldChildren.MoveNext() && newChildren.MoveNext())
            {
                PerformDiff(oldChildren.Current, newChildren.Current, oldDeserializable);
            }

            while (oldChildren.MoveNext())
            {
                oldDeserializable.RemoveChild(oldChildren.Current);
            }

            while (newChildren.MoveNext())
            {
                oldDeserializable.AddChild(newChildren.Current);
            }

            if (oldChildren.Current == null && newChildren.Current != null &&
                oldDeserializable.Count() < newDeserializable.Count())
            {
                oldDeserializable.AddChild(newChildren.Current);
            }
            else if (oldChildren.Current != null && newChildren.Current == null &&
                     oldDeserializable.Count() > newDeserializable.Count())
            {
                oldDeserializable.RemoveChild(oldChildren.Current);
            }
        }
    }

    private void ApplyProperties(ILayoutElement<Control> from, ILayoutElement<Control> to)
    {
        if (to is IPropertyDeserializable propertyDeserializable && from is IPropertyDeserializable fromProps)
        {
            // TODO: Find a way to only apply changed properties, current solution shouldn't be a problem for most cases, but this
            // might cause unnecessary redraws, binding fires and other stuff that might be expensive if we have a lot of elements
            propertyDeserializable.DeserializeProperties(fromProps.GetProperties().ToImmutableList());
        }
    }

    /*IEnumerable<object> IPropertyDeserializable.GetProperties()
    {
        if (_content == null)
        {
            Dispatcher.UIThread.Invoke(() =>
            {
                BuildNative();
            });
        }

        yield return _content;
    }

    void IPropertyDeserializable.DeserializeProperties(IEnumerable<object> values)
    {
        if (values == null || !values.Any())
        {
            return;
        }

        object first = values.ElementAt(0);
        if(first is ILayoutElement<Control> layoutElement)
        {
            ContentFromLayout(layoutElement);
        }
    }*/
}
