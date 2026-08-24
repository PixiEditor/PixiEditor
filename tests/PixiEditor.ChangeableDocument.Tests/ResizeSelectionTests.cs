using Drawie.Backend.Core;
using Drawie.Backend.Core.Bridge;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Vector;
using Drawie.Numerics;
using Drawie.Skia;
using OneOf;
using OneOf.Types;
using PixiEditor.ChangeableDocument.Changeables;
using PixiEditor.ChangeableDocument.ChangeInfos;
using PixiEditor.ChangeableDocument.ChangeInfos.Drawing;
using PixiEditor.ChangeableDocument.Changes.Root;
using PixiEditor.ChangeableDocument.Enums;

namespace PixiEditor.ChangeableDocument.Tests;

public class ResizeSelectionTests
{
    static ResizeSelectionTests()
    {
        if (!DrawingBackendApi.HasBackend)
            DrawingBackendApi.SetupBackend(new SkiaDrawingBackend(), new ImmediateRenderingDispatcher());
    }

    [Fact]
    public void ResizeCanvasTranslatesConstrainsAndRestoresSelection()
    {
        using Document document = CreateDocument(
            new VecI(100, 100),
            new RectD(0, 0, 60, 60));
        using ResizeCanvas_Change change = new(new VecI(60, 60), ResizeAnchor.BottomRight);

        Assert.True(change.InitializeAndValidate(document));

        var applied = change.Apply(document, true, out bool ignoreInUndo);

        Assert.False(ignoreInUndo);
        Assert.Equal(new RectD(0, 0, 20, 20), document.Selection.SelectionPath.TightBounds);
        DisposeSelectionInfos(applied);

        var reverted = change.Revert(document);

        Assert.Equal(new RectD(0, 0, 60, 60), document.Selection.SelectionPath.TightBounds);
        DisposeSelectionInfos(reverted);

        var reapplied = change.Apply(document, false, out ignoreInUndo);

        Assert.False(ignoreInUndo);
        Assert.Equal(new RectD(0, 0, 20, 20), document.Selection.SelectionPath.TightBounds);
        DisposeSelectionInfos(reapplied);
    }

    [Fact]
    public void ResizeImageScalesConstrainsAndRestoresSelection()
    {
        RectD originalSelectionBounds = new(-20, -10, 120, 100);
        using Document document = CreateDocument(new VecI(100, 80), originalSelectionBounds);
        using ResizeImage_Change change = new(new VecI(50, 40), ResamplingMethod.NearestNeighbor);

        Assert.True(change.InitializeAndValidate(document));

        var applied = change.Apply(document, true, out bool ignoreInUndo);

        Assert.False(ignoreInUndo);
        Assert.Equal(new RectD(0, 0, 50, 40), document.Selection.SelectionPath.TightBounds);
        DisposeSelectionInfos(applied);

        var reverted = change.Revert(document);

        Assert.Equal(originalSelectionBounds, document.Selection.SelectionPath.TightBounds);
        DisposeSelectionInfos(reverted);

        var reapplied = change.Apply(document, false, out ignoreInUndo);

        Assert.False(ignoreInUndo);
        Assert.Equal(new RectD(0, 0, 50, 40), document.Selection.SelectionPath.TightBounds);
        DisposeSelectionInfos(reapplied);
    }

    private static Document CreateDocument(VecI size, RectD selectionBounds)
    {
        Document document = new() { Size = size };
        VectorPath selection = new() { FillType = PathFillType.EvenOdd };
        selection.AddRect(selectionBounds);

        document.Selection.SelectionPath.Dispose();
        document.Selection.SelectionPath = selection;
        return document;
    }

    private static void DisposeSelectionInfos(OneOf<None, IChangeInfo, List<IChangeInfo>> infos)
    {
        infos.Switch(
            static (None _) => { },
            static (IChangeInfo info) => DisposeSelectionInfo(info),
            static (List<IChangeInfo> changes) => changes.ForEach(DisposeSelectionInfo));
    }

    private static void DisposeSelectionInfo(IChangeInfo info)
    {
        if (info is Selection_ChangeInfo selection)
            selection.NewPath.Dispose();
    }

    private sealed class ImmediateRenderingDispatcher : IRenderingDispatcher
    {
        public Action<Action> Invoke { get; } = action => action();

        public Task<TResult> InvokeAsync<TResult>(Func<TResult> function)
        {
            return Task.FromResult(function());
        }

        public Task<TResult> InvokeInBackgroundAsync<TResult>(Func<TResult> function)
        {
            return Task.FromResult(function());
        }

        public Task InvokeInBackgroundAsync(Action function)
        {
            function();
            return Task.CompletedTask;
        }

        public IDisposable EnsureContext()
        {
            return new EmptyDisposable();
        }
    }

    private sealed class EmptyDisposable : IDisposable
    {
        public void Dispose()
        {
        }
    }
}
