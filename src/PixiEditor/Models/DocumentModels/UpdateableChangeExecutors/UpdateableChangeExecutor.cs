using Avalonia.Input;
using ChunkyImageLib.DataHolders;
using Microsoft.Extensions.DependencyInjection;
using PixiEditor.ChangeableDocument.Enums;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Numerics;
using PixiEditor.Models.Handlers;
using PixiEditor.Models.Tools;
using Drawie.Numerics;
using PixiEditor.Models.Controllers.InputDevice;
using PixiEditor.Views.Overlays.SymmetryOverlay;

namespace PixiEditor.Models.DocumentModels.UpdateableChangeExecutors;
#nullable enable
internal abstract class UpdateableChangeExecutor
{
    protected IDocument? document;
    protected DocumentInternalParts? internals;
    protected ChangeExecutionController? controller;
    protected IServiceProvider services;
    private bool initialized = false;

    protected Action<UpdateableChangeExecutor>? onEnded;
    public virtual ExecutorType Type => ExecutorType.Regular;
    public virtual ExecutorStartMode StartMode => ExecutorStartMode.RightAway;
    public virtual bool BlocksOtherActions => true;

    public void Initialize(IDocument document, DocumentInternalParts internals, IServiceProvider services,
        ChangeExecutionController controller, Action<UpdateableChangeExecutor> onEnded)
    {
        if (initialized)
            throw new InvalidOperationException();
        initialized = true;

        this.document = document;
        this.internals = internals;
        this.controller = controller;
        this.services = services;
        this.onEnded = onEnded;
    }

    protected T GetHandler<T>()
        where T : IHandler
    {
        return services.GetRequiredService<T>();
    }

    public abstract ExecutionState Start();
    public abstract void ForceStop();
    public virtual void OnPixelPositionChange(VecI pos, MouseOnCanvasEventArgs args) { }
    public virtual void OnPrecisePositionChange(MouseOnCanvasEventArgs args) { }
    public virtual void OnLeftMouseButtonDown(MouseOnCanvasEventArgs args) { }
    public virtual void OnLeftMouseButtonUp(VecD pos) { }
    public virtual void OnOpacitySliderDragStarted() { }
    public virtual void OnOpacitySliderDragged(float newValue) { }
    public virtual void OnOpacitySliderDragEnded() { }
    public virtual void OnSymmetryDragStarted(SymmetryAxisDirection dir) { }
    public virtual void OnSymmetryDragged(SymmetryAxisDragInfo info) { }
    public virtual void OnSymmetryDragEnded(SymmetryAxisDirection dir) { }
    public virtual void OnConvertedKeyDown(Key key) { }
    public virtual void OnConvertedKeyUp(Key key) { }
    public virtual void OnSettingsChanged(string name, object value) { }
    public virtual void OnColorChanged(Color color, bool primary) { }
    public virtual void OnMembersSelected(List<Guid> memberGuids) { }

    protected T[] QueryLayers<T>(VecD pos) where T : IStructureMemberHandler
    {
        var allLayers = document.StructureHelper.GetAllMembers();
        FilterOutInvisible(allLayers);
        var topMostWithinClick = allLayers.Where(x =>
                x is T { TightBounds: not null } &&
                x.TightBounds.Value.ContainsInclusive(pos))
            .OrderByDescending(x => allLayers.IndexOf(x));
        return topMostWithinClick.Cast<T>().ToArray();
    }

    private void FilterOutInvisible(List<IStructureMemberHandler> allLayers)
    {
        allLayers.RemoveAll(x => x is IStructureMemberHandler { IsVisibleBindable: false });

        List<IStructureMemberHandler> toRemove = new List<IStructureMemberHandler>();
        foreach (var layer in allLayers)
        {
            var parents = document.StructureHelper.GetParents(layer.Id);
            if(parents.Any(x => !x.IsVisibleBindable))
            {
                toRemove.Add(layer);
            }
        }

        foreach (var layer in toRemove)
        {
            allLayers.Remove(layer);
        }
    }
}
