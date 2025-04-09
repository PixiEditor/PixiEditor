using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using PixiEditor.Helpers.Extensions;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces;
using PixiEditor.Helpers;
using PixiEditor.Models.Clipboard;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.Models.Commands.Attributes.Evaluators;
using PixiEditor.Models.Commands.Search;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.Handlers;
using PixiEditor.Models.IO;
using PixiEditor.Models.Layers;
using Drawie.Numerics;
using PixiEditor.Helpers.Constants;
using PixiEditor.Models.Commands;
using PixiEditor.UI.Common.Fonts;
using PixiEditor.ViewModels.Dock;
using PixiEditor.ViewModels.Document;

namespace PixiEditor.ViewModels.SubViewModels;
#nullable enable
[Command.Group("PixiEditor.Clipboard", "CLIPBOARD")]
internal class ClipboardViewModel : SubViewModel<ViewModelMain>
{
    private ConcurrentDictionary<string, Task> clipboardTasks = new();
    private bool canPasteImage;
    private string lastTextInClipboard;
    private bool areNodesInClipboard;
    private bool areCelsInClipboard;
    private bool hasImageInClipboard;

    public ClipboardViewModel(ViewModelMain owner)
        : base(owner)
    {
        Application.Current.ForDesktopMainWindow((mainWindow) =>
        {
            ClipboardController.Initialize(mainWindow.Clipboard);
        });
    }

    [Command.Basic("PixiEditor.Clipboard.Cut", "CUT", "CUT_DESCRIPTIVE", CanExecute = "PixiEditor.Selection.IsNotEmpty",
        Key = Key.X, Modifiers = KeyModifiers.Control,
        MenuItemPath = "EDIT/CUT", MenuItemOrder = 2, Icon = PixiPerfectIcons.Scissors, AnalyticsTrack = true)]
    public async Task Cut()
    {
        var doc = Owner.DocumentManagerSubViewModel.ActiveDocument;
        if (doc is null)
            return;
        await Copy();
        doc.Operations.DeleteSelectedPixels(doc.AnimationDataViewModel.ActiveFrameBindable, true);
    }

    [Command.Basic("PixiEditor.Clipboard.PasteAsNewLayer", true, "PASTE_AS_NEW_LAYER", "PASTE_AS_NEW_LAYER_DESCRIPTIVE",
        CanExecute = "PixiEditor.Clipboard.CanPaste", Key = Key.V, Modifiers = KeyModifiers.Control,
        ShortcutContexts = [typeof(ViewportWindowViewModel), typeof(LayersDockViewModel)],
        Icon = PixiPerfectIcons.PasteAsNewLayer, AnalyticsTrack = true)]
    [Command.Basic("PixiEditor.Clipboard.Paste", false, "PASTE", "PASTE_DESCRIPTIVE",
        CanExecute = "PixiEditor.Clipboard.CanPaste", Key = Key.V, Modifiers = KeyModifiers.Shift,
        MenuItemPath = "EDIT/PASTE", MenuItemOrder = 4, Icon = PixiPerfectIcons.Paste, AnalyticsTrack = true)]
    public void Paste(bool pasteAsNewLayer)
    {
        if (Owner.DocumentManagerSubViewModel.ActiveDocument is null)
            return;

        var doc = Owner.DocumentManagerSubViewModel.ActiveDocument;

        Dispatcher.UIThread.InvokeAsync(async () =>
        {
            Guid[] guids = doc.StructureHelper.GetAllLayers().Select(x => x.Id).ToArray();
            await ClipboardController.TryPasteFromClipboard(doc, Owner.DocumentManagerSubViewModel, pasteAsNewLayer);

            doc.Operations.InvokeCustomAction(() =>
            {
                Guid[] newGuids = doc.StructureHelper.GetAllLayers().Select(x => x.Id).ToArray();

                var diff = newGuids.Except(guids).ToArray();
                if (diff.Length > 0)
                {
                    doc.Operations.ClearSoftSelectedMembers();
                    doc.Operations.SetSelectedMember(diff[0]);

                    for (int i = 1; i < diff.Length; i++)
                    {
                        doc.Operations.AddSoftSelectedMember(diff[i]);
                    }
                }
            });
        });
    }

    [Command.Basic("PixiEditor.Clipboard.PasteReferenceLayer", "PASTE_REFERENCE_LAYER",
        "PASTE_REFERENCE_LAYER_DESCRIPTIVE", CanExecute = "PixiEditor.Clipboard.CanPaste",
        Icon = PixiPerfectIcons.PasteReferenceLayer, AnalyticsTrack = true)]
    public void PasteReferenceLayer(IDataObject data)
    {
        var doc = Owner.DocumentManagerSubViewModel.ActiveDocument;

        Dispatcher.UIThread.InvokeAsync(async () =>
        {
            DataImage imageData =
                (data == null
                    ? await ClipboardController.GetImagesFromClipboard()
                    : ClipboardController.GetImage(new[] { data })).First();
            using var surface = imageData.Image;

            var bitmap = imageData.Image.ToWriteableBitmap();

            byte[] pixels = bitmap.ExtractPixels();

            doc.Operations.ImportReferenceLayer(
                pixels.ToImmutableArray(),
                imageData.Image.Size);

            if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow!.Activate();
            }
        });
    }

    [Command.Internal("PixiEditor.Clipboard.PasteReferenceLayerFromPath")]
    public void PasteReferenceLayer(string path)
    {
        var doc = Owner.DocumentManagerSubViewModel.ActiveDocument;

        // TODO: Exception handling would probably be good
        var bitmap = Importer.GetPreviewSurface(path);
        byte[] pixels = bitmap.ToWriteableBitmap().ExtractPixels();

        doc.Operations.ImportReferenceLayer(
            pixels.ToImmutableArray(),
            new VecI(bitmap.Size.X, bitmap.Size.Y));
    }

    [Command.Basic("PixiEditor.Clipboard.PasteColor", false, "PASTE_COLOR", "PASTE_COLOR_DESCRIPTIVE",
        CanExecute = "PixiEditor.Clipboard.CanPasteColor", IconEvaluator = "PixiEditor.Clipboard.PasteColorIcon",
        AnalyticsTrack = true)]
    [Command.Basic("PixiEditor.Clipboard.PasteColorAsSecondary", true, "PASTE_COLOR_SECONDARY",
        "PASTE_COLOR_SECONDARY_DESCRIPTIVE", CanExecute = "PixiEditor.Clipboard.CanPasteColor",
        IconEvaluator = "PixiEditor.Clipboard.PasteColorIcon", AnalyticsTrack = true)]
    public void PasteColor(bool secondary)
    {
        Dispatcher.UIThread.InvokeAsync(async () =>
        {
            if (!ColorHelper.ParseAnyFormat(
                    (await ClipboardController.Clipboard.GetTextAsync())?.Trim() ?? string.Empty,
                    out var result))
            {
                return;
            }

            if (!secondary)
            {
                Owner.ColorsSubViewModel.PrimaryColor = result.Value;
            }
            else
            {
                Owner.ColorsSubViewModel.SecondaryColor = result.Value;
            }
        });
    }

    [Command.Basic("PixiEditor.Clipboard.PasteNodes", "PASTE_NODES", "PASTE_NODES_DESCRIPTIVE",
        ShortcutContexts = [typeof(NodeGraphDockViewModel)], Key = Key.V, Modifiers = KeyModifiers.Control,
        CanExecute = "PixiEditor.Clipboard.CanPasteNodes", Icon = PixiPerfectIcons.Paste, AnalyticsTrack = true)]
    public void PasteNodes()
    {
        var doc = Owner.DocumentManagerSubViewModel.ActiveDocument;
        if (doc is null)
            return;

        Dispatcher.UIThread.InvokeAsync(async () =>
        {
            Guid[] toDuplicate = await ClipboardController.GetNodeIds();

            List<Guid> newIds = new();

            Dictionary<Guid, Guid> nodeMapping = new();

            using var block = doc.Operations.StartChangeBlock();

            foreach (var nodeId in toDuplicate)
            {
                Guid? newId = doc.Operations.DuplicateNode(nodeId);
                if (newId != null)
                {
                    newIds.Add(newId.Value);
                    nodeMapping.Add(nodeId, newId.Value);
                }
            }

            if (newIds.Count == 0)
                return;

            block.ExecuteQueuedActions();

            ConnectRelatedNodes(doc, nodeMapping);

            doc.Operations.InvokeCustomAction(() =>
            {
                foreach (var node in doc.NodeGraph.AllNodes)
                {
                    node.IsNodeSelected = false;
                }

                foreach (var node in newIds)
                {
                    var nodeInstance = doc.NodeGraph.AllNodes.FirstOrDefault(x => x.Id == node);
                    if (nodeInstance != null)
                    {
                        nodeInstance.IsNodeSelected = true;
                    }
                }
            });
        });
    }

    [Command.Basic("PixiEditor.Clipboard.PasteCels", "PASTE_CELS", "PASTE_CELS_DESCRIPTIVE",
        CanExecute = "PixiEditor.Clipboard.CanPasteCels", Key = Key.V, Modifiers = KeyModifiers.Control,
        ShortcutContexts = [typeof(TimelineDockViewModel)], Icon = PixiPerfectIcons.Paste, AnalyticsTrack = true)]
    public void PasteCels()
    {
        var doc = Owner.DocumentManagerSubViewModel.ActiveDocument;
        if (doc is null)
            return;

        Dispatcher.UIThread.InvokeAsync(async () =>
        {
            var cels = await ClipboardController.GetCelIds();

            if (cels.Length == 0)
                return;

            using var block = doc.Operations.StartChangeBlock();

            List<Guid> newCels = new();
            List<ICelHandler> celsToSelect = new();

            int minStartFrame = int.MaxValue;

            foreach (var cel in cels)
            {
                var foundCel = doc.AnimationDataViewModel.AllCels.FirstOrDefault(x => x.Id == cel);
                if (foundCel == null)
                    continue;

                celsToSelect.Add(foundCel);
                minStartFrame = Math.Min(minStartFrame, foundCel.StartFrameBindable);
            }

            int delta = doc.AnimationDataViewModel.ActiveFrameBindable - minStartFrame;

            foreach (var cel in celsToSelect)
            {
                int celFrame = cel.StartFrameBindable + delta;
                Guid? newCel = doc.AnimationDataViewModel.CreateCel(cel.LayerGuid,
                    celFrame, cel.LayerGuid,
                    cel.StartFrameBindable);
                if (newCel != null)
                {
                    int duration = cel.DurationBindable;
                    doc.Operations.ChangeCelLength(newCel.Value, celFrame, duration);
                    newCels.Add(newCel.Value);
                }
            }

            doc.Operations.InvokeCustomAction(() =>
            {
                foreach (var cel in doc.AnimationDataViewModel.AllCels)
                {
                    cel.IsSelected = false;
                }

                foreach (var cel in newCels)
                {
                    var celInstance = doc.AnimationDataViewModel.AllCels.FirstOrDefault(x => x.Id == cel);
                    if (celInstance != null)
                    {
                        celInstance.IsSelected = true;
                    }
                }
            });
        });
    }

    [Command.Basic("PixiEditor.Clipboard.Copy", "COPY", "COPY_DESCRIPTIVE",
        CanExecute = "PixiEditor.Clipboard.CanCopy",
        Key = Key.C, Modifiers = KeyModifiers.Control,
        ShortcutContexts = [typeof(ViewportWindowViewModel), typeof(LayersDockViewModel)],
        MenuItemPath = "EDIT/COPY", MenuItemOrder = 3, Icon = PixiPerfectIcons.Copy, AnalyticsTrack = true)]
    public async Task Copy()
    {
        var doc = Owner.DocumentManagerSubViewModel.ActiveDocument;
        if (doc is null)
            return;

        await ClipboardController.CopyToClipboard(doc);

        hasImageInClipboard = true;
    }

    [Command.Basic("PixiEditor.Clipboard.CopyVisible", "COPY_VISIBLE", "COPY_VISIBLE_DESCRIPTIVE",
        CanExecute = "PixiEditor.Clipboard.CanCopy",
        Key = Key.C, Modifiers = KeyModifiers.Shift,
        MenuItemPath = "EDIT/COPY_VISIBLE", MenuItemOrder = 3, Icon = PixiPerfectIcons.Copy, AnalyticsTrack = true)]
    public async Task CopyVisible()
    {
        var doc = Owner.DocumentManagerSubViewModel.ActiveDocument;
        if (doc is null)
            return;

        await ClipboardController.CopyVisibleToClipboard(
            doc, Owner.WindowSubViewModel.ActiveWindow is ViewportWindowViewModel viewportWindowViewModel
                ? viewportWindowViewModel.RenderOutputName
                : null);

        hasImageInClipboard = true;
    }

    [Command.Basic("PixiEditor.Clipboard.CopyNodes", "COPY_NODES", "COPY_NODES_DESCRIPTIVE",
        Key = Key.C, Modifiers = KeyModifiers.Control,
        ShortcutContexts = [typeof(NodeGraphDockViewModel)],
        CanExecute = "PixiEditor.Clipboard.CanCopyNodes",
        Icon = PixiPerfectIcons.Copy, AnalyticsTrack = true)]
    public async Task CopySelectedNodes()
    {
        var doc = Owner.DocumentManagerSubViewModel.ActiveDocument;
        if (doc is null)
            return;

        var selectedNodes = doc.NodeGraph.AllNodes.Where(x => x.IsNodeSelected).Select(x => x.Id).ToArray();
        if (selectedNodes.Length == 0)
            return;

        await ClipboardController.CopyNodes(selectedNodes);

        areNodesInClipboard = true;
    }

    [Command.Basic("PixiEditor.Clipboard.CopyCels", "COPY_CELS",
        "COPY_CELS_DESCRIPTIVE", CanExecute = "PixiEditor.Clipboard.CanCopyCels",
        ShortcutContexts = [typeof(TimelineDockViewModel)],
        Key = Key.C, Modifiers = KeyModifiers.Control, Icon = PixiPerfectIcons.Copy, AnalyticsTrack = true)]
    public async Task CopySelectedCels()
    {
        var doc = Owner.DocumentManagerSubViewModel.ActiveDocument;
        if (doc is null)
            return;

        var selectedCels = doc.AnimationDataViewModel.AllCels.Where(x => x.IsSelected).Select(x => x.Id).ToArray();
        if (selectedCels.Length == 0)
            return;

        await ClipboardController.CopyCels(selectedCels);

        areCelsInClipboard = true;
    }


    [Command.Basic("PixiEditor.Clipboard.CopyPrimaryColorAsHex", CopyColor.PrimaryHEX, "COPY_COLOR_HEX",
        "COPY_COLOR_HEX_DESCRIPTIVE", IconEvaluator = "PixiEditor.Clipboard.CopyColorIcon", AnalyticsTrack = true)]
    [Command.Basic("PixiEditor.Clipboard.CopyPrimaryColorAsRgb", CopyColor.PrimaryRGB, "COPY_COLOR_RGB",
        "COPY_COLOR_RGB_DESCRIPTIVE", IconEvaluator = "PixiEditor.Clipboard.CopyColorIcon", AnalyticsTrack = true)]
    [Command.Basic("PixiEditor.Clipboard.CopySecondaryColorAsHex", CopyColor.SecondaryHEX,
        "COPY_COLOR_SECONDARY_HEX",
        "COPY_COLOR_SECONDARY_HEX_DESCRIPTIVE", IconEvaluator = "PixiEditor.Clipboard.CopyColorIcon",
        AnalyticsTrack = true)]
    [Command.Basic("PixiEditor.Clipboard.CopySecondaryColorAsRgb", CopyColor.SecondardRGB,
        "COPY_COLOR_SECONDARY_RGB",
        "COPY_COLOR_SECONDARY_RGB_DESCRIPTIVE", IconEvaluator = "PixiEditor.Clipboard.CopyColorIcon",
        AnalyticsTrack = true)]
    [Command.Filter("PixiEditor.Clipboard.CopyColorToClipboard", "COPY_COLOR_TO_CLIPBOARD", "COPY_COLOR",
        Key = Key.C,
        Modifiers = KeyModifiers.Shift | KeyModifiers.Alt, AnalyticsTrack = true)]
    public async Task CopyColorAsHex(CopyColor color)
    {
        var targetColor = color switch
        {
            CopyColor.PrimaryHEX or CopyColor.PrimaryRGB => Owner.ColorsSubViewModel.PrimaryColor,
            _ => Owner.ColorsSubViewModel.SecondaryColor
        };

        string text = color switch
        {
            CopyColor.PrimaryHEX or CopyColor.SecondaryHEX => targetColor.A == 255
                ? $"#{targetColor.R:X2}{targetColor.G:X2}{targetColor.B:X2}"
                : targetColor.ToString(),
            _ => targetColor.A == 255
                ? $"rgb({targetColor.R},{targetColor.G},{targetColor.B})"
                : $"rgba({targetColor.R},{targetColor.G},{targetColor.B},{targetColor.A})",
        };

        await ClipboardController.Clipboard.SetTextAsync(text);
    }

    [Evaluator.CanExecute("PixiEditor.Clipboard.CanPaste")]
    public bool CanPaste(object parameter)
    {
        if (!Owner.DocumentIsNotNull(null)) return false;

        if (parameter is IDataObject data)
            return ClipboardController.IsImage(data);

        QueueCheckCanPasteImage();
        return canPasteImage;
    }

    [Evaluator.CanExecute("PixiEditor.Clipboard.HasImageInClipboard")]
    public bool HasImageInClipboard()
    {
        QueueHasImageInClipboard();
        return hasImageInClipboard;
    }

    [Evaluator.CanExecute("PixiEditor.Clipboard.CanCopyCels")]
    public bool CanCopyCels()
    {
        return Owner.DocumentIsNotNull(null) &&
               Owner.DocumentManagerSubViewModel.ActiveDocument.AnimationDataViewModel.AllCels.Any(
                   x => x.IsSelected);
    }

    [Evaluator.CanExecute("PixiEditor.Clipboard.CanCopyNodes",
        nameof(Owner.DocumentManagerSubViewModel),
        nameof(Owner.DocumentManagerSubViewModel.ActiveDocument),
        nameof(Owner.DocumentManagerSubViewModel.ActiveDocument.NodeGraph))]
    public bool CanCopyNodes()
    {
        return Owner.DocumentIsNotNull(null) &&
               Owner.DocumentManagerSubViewModel.ActiveDocument.NodeGraph.AllNodes.Any(x => x.IsNodeSelected);
    }


    [Evaluator.CanExecute("PixiEditor.Clipboard.CanPasteNodes",
        nameof(Owner.DocumentManagerSubViewModel),
        nameof(Owner.DocumentManagerSubViewModel.ActiveDocument),
        nameof(Owner.DocumentManagerSubViewModel.ActiveDocument.NodeGraph))]
    public bool CanPasteNodes()
    {
        if (!Owner.DocumentIsNotNull(null)) return false;

        QueueCheckNodesInClipboard();
        return areNodesInClipboard;
    }

    [Evaluator.CanExecute("PixiEditor.Clipboard.CanPasteCels",
        nameof(Owner.DocumentManagerSubViewModel),
        nameof(Owner.DocumentManagerSubViewModel.ActiveDocument),
        nameof(Owner.DocumentManagerSubViewModel.ActiveDocument.AnimationDataViewModel))]
    public bool CanPasteCels()
    {
        if (!Owner.DocumentIsNotNull(null)) return false;

        QueueCheckCelsInClipboard();
        return areCelsInClipboard;
    }

    [Evaluator.CanExecute("PixiEditor.Clipboard.CanPasteColor")]
    public bool CanPasteColor()
    {
        QueueFetchTextFromClipboard();
        return ColorHelper.ParseAnyFormat(lastTextInClipboard?.Trim() ?? string.Empty, out _);
    }

    [Evaluator.CanExecute("PixiEditor.Clipboard.CanCopy",
        nameof(Owner.DocumentManagerSubViewModel.ActiveDocument),
        nameof(Owner.DocumentManagerSubViewModel.ActiveDocument.TransformViewModel.TransformActive),
        nameof(Owner.DocumentManagerSubViewModel.ActiveDocument.SelectedStructureMember))]
    public bool CanCopy()
    {
        return Owner.DocumentManagerSubViewModel.ActiveDocument != null &&
               (Owner.SelectionSubViewModel.SelectionIsNotEmpty() ||
                Owner.DocumentManagerSubViewModel.ActiveDocument.TransformViewModel.TransformActive
                || Owner.DocumentManagerSubViewModel.ActiveDocument.SelectedStructureMember != null);
    }

    [Evaluator.Icon("PixiEditor.Clipboard.PasteColorIcon")]
    public IImage GetPasteColorIcon()
    {
        Color color;

        QueueFetchTextFromClipboard();
        color = ColorHelper.ParseAnyFormat(lastTextInClipboard?.Trim() ?? string.Empty,
            out var result)
            ? result.Value.ToOpaqueMediaColor()
            : Colors.Transparent;
        return ColorSearchResult.GetIcon(color.ToOpaqueColor());
    }

    [Evaluator.Icon("PixiEditor.Clipboard.CopyColorIcon")]
    public IImage GetCopyColorIcon(object data)
    {
        if (data is CopyColor color)
        {
        }
        else if (data is Models.Commands.Commands.Command.BasicCommand command)
        {
            color = (CopyColor)command.Parameter;
        }
        else if (data is Models.Commands.Search.CommandSearchResult result)
        {
            color = (CopyColor)((Models.Commands.Commands.Command.BasicCommand)result.Command).Parameter;
        }
        else
        {
            throw new ArgumentException("data must be of type CopyColor, BasicCommand or CommandSearchResult");
        }

        var targetColor = color switch
        {
            CopyColor.PrimaryHEX or CopyColor.PrimaryRGB => Owner.ColorsSubViewModel.PrimaryColor,
            _ => Owner.ColorsSubViewModel.SecondaryColor
        };

        return ColorSearchResult.GetIcon(targetColor.ToOpaqueMediaColor().ToOpaqueColor());
    }

    private void ConnectRelatedNodes(DocumentViewModel doc, Dictionary<Guid, Guid> nodeMapping)
    {
        foreach (var connection in doc.NodeGraph.Connections)
        {
            if (nodeMapping.TryGetValue(connection.InputNode.Id, out var inputNode) &&
                nodeMapping.TryGetValue(connection.OutputNode.Id, out var outputNode))
            {
                var inputNodeInstance = doc.NodeGraph.AllNodes.FirstOrDefault(x => x.Id == inputNode);
                var outputNodeInstance = doc.NodeGraph.AllNodes.FirstOrDefault(x => x.Id == outputNode);

                if (inputNodeInstance == null || outputNodeInstance == null)
                    continue;

                var inputProperty =
                    inputNodeInstance.Inputs.FirstOrDefault(
                        x => x.PropertyName == connection.InputProperty.PropertyName);
                var outputProperty =
                    outputNodeInstance.Outputs.FirstOrDefault(x =>
                        x.PropertyName == connection.OutputProperty.PropertyName);

                if (inputProperty == null || outputProperty == null)
                    continue;

                doc.NodeGraph.ConnectProperties(inputProperty, outputProperty);
            }
        }
    }

    private void QueueHasImageInClipboard()
    {
        QueueClipboardTask("HasImageInClipboard", ClipboardController.IsImageInClipboard, hasImageInClipboard,
            x => hasImageInClipboard = x);
    }

    private void QueueCheckCanPasteImage()
    {
        QueueClipboardTask("CheckCanPasteImage", ClipboardController.IsImageInClipboard, canPasteImage,
            x => canPasteImage = x);
    }

    private void QueueFetchTextFromClipboard()
    {
        QueueClipboardTask("FetchTextFromClipboard", ClipboardController.GetTextFromClipboard, lastTextInClipboard,
            x => lastTextInClipboard = x);
    }

    private void QueueCheckNodesInClipboard()
    {
        QueueClipboardTask("CheckNodesInClipboard", ClipboardController.AreNodesInClipboard, areNodesInClipboard,
            x => areNodesInClipboard = x);
    }

    private void QueueCheckCelsInClipboard()
    {
        QueueClipboardTask("CheckCelsInClipboard", ClipboardController.AreCelsInClipboard, areCelsInClipboard,
            x => areCelsInClipboard = x);
    }

    private void QueueClipboardTask<T>(string key, Func<Task<T>> task, T value, Action<T> updateAction)
    {
        if (clipboardTasks.TryGetValue(key, out var t))
        {
            if (t.IsFaulted)
            {
                clipboardTasks.Remove(key, out _);
            }

            return;
        }

        var newTask = Dispatcher.UIThread.InvokeAsync(
            async () =>
            {
                T result = await task();
                if (!EqualityComparer<T>.Default.Equals(result, value))
                {
                    updateAction(result);
                    CommandController.CanExecuteChanged("PixiEditor.Clipboard");
                }

                clipboardTasks.Remove(key, out _);
            });

        clipboardTasks.TryAdd(key, newTask);
    }

    public enum CopyColor
    {
        PrimaryHEX,
        PrimaryRGB,
        SecondaryHEX,
        SecondardRGB
    }
}
