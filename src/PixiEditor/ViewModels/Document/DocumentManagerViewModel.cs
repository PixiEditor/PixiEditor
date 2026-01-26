using System.Collections;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Input;
using Avalonia.Threading;
using PixiEditor.ChangeableDocument.Enums;
using PixiEditor.Helpers;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.Models.Commands.Attributes.Evaluators;
using PixiEditor.Models.Dialogs;
using PixiEditor.Models.Handlers;
using PixiEditor.Models.IO;
using PixiEditor.UI.Common.Fonts;
using PixiEditor.ViewModels.SubViewModels;
using PixiEditor.ViewModels.Tools.Tools;
using PixiEditor.Views;
using PixiEditor.Views.Overlays.SymmetryOverlay;

namespace PixiEditor.ViewModels.Document;
#nullable enable
[Command.Group("PixiEditor.Document", "IMAGE")]
internal class DocumentManagerViewModel : SubViewModel<ViewModelMain>, IDocumentManagerHandler
{
    public ObservableCollection<DocumentViewModel> Documents { get; } = new ObservableCollection<DocumentViewModel>();

    public ObservableCollection<LazyDocumentViewModel> LazyDocuments { get; } =
        new ObservableCollection<LazyDocumentViewModel>();

    public event EventHandler<DocumentChangedEventArgs>? ActiveDocumentChanged;

    private DocumentViewModel? activeDocument;

    public DocumentViewModel? ActiveDocument
    {
        get => activeDocument;
        // Use WindowSubViewModel.MakeDocumentViewportActive(document);
        private set
        {
            if (activeDocument == value)
                return;
            DocumentViewModel? prevDoc = activeDocument;
            activeDocument = value;
            OnPropertyChanged(nameof(ActiveDocument));
            ActiveDocumentChanged?.Invoke(this, new(value, prevDoc));

            if (ViewModelMain.Current.ToolsSubViewModel.ActiveTool == null)
            {
                var firstTool =
                    ViewModelMain.Current.ToolsSubViewModel.ActiveToolSet?.Tools.FirstOrDefault(x =>
                        x.CanBeUsedOnActiveLayer);
                if (firstTool != null)
                {
                    ViewModelMain.Current.ToolsSubViewModel.SetActiveTool(firstTool.GetType(), false);
                }
            }
        }
    }

    IDocument? IDocumentManagerHandler.ActiveDocument
    {
        get => ActiveDocument;
        set => ActiveDocument = (DocumentViewModel)value;
    }

    public bool HasActiveDocument => ActiveDocument != null;
    public ImmutableHashSet<DocumentReferenceData> DocumentReferences => documentReferences.ToImmutableHashSet();

    public event Action<DocumentViewModel> DocumentAdded;

    private HashSet<DocumentReferenceData> documentReferences = new();

    public DocumentManagerViewModel(ViewModelMain owner) : base(owner)
    {
        owner.WindowSubViewModel.ActiveViewportChanged += (_, args) =>
        {
            ActiveDocument = args.Document;
        };
    }

    public void MakeActiveDocumentNull() => ActiveDocument = null;

    [Evaluator.CanExecute("PixiEditor.HasDocument", nameof(ActiveDocument))]
    public bool DocumentNotNull() => ActiveDocument != null;

    [Command.Basic("PixiEditor.Document.Open", "OPEN_DOCUMENT", "OPEN_DOCUMENT_DESC",
        Icon = PixiPerfectIcons.File, AnalyticsTrack = true)]
    public void OpenDocument(string path)
    {
        if (Guid.TryParse(path, out Guid referenceId))
        {
            Owner.FileSubViewModel.OpenDocumentReference(referenceId);
        }
        else if (Path.Exists(path))
        {
            Owner.FileSubViewModel.OpenFromPath(path);
        }
    }

    [Command.Basic("PixiEditor.Document.ClipCanvas", "CLIP_CANVAS", "CLIP_CANVAS",
        CanExecute = "PixiEditor.HasDocument",
        Icon = PixiPerfectIcons.Crop, MenuItemPath = "IMAGE/CLIP_CANVAS", MenuItemOrder = 2, AnalyticsTrack = true)]
    public void ClipCanvas() => ActiveDocument?.Operations.ClipCanvas();

    [Command.Basic("PixiEditor.Document.FlipImageHorizontal", FlipType.Horizontal, "FLIP_IMG_HORIZONTALLY",
        "FLIP_IMG_HORIZONTALLY", CanExecute = "PixiEditor.HasDocument",
        MenuItemPath = "IMAGE/FLIP/FLIP_IMG_HORIZONTALLY", MenuItemOrder = 14, Icon = PixiPerfectIcons.XFlip,
        AnalyticsTrack = true)]
    [Command.Basic("PixiEditor.Document.FlipImageVertical", FlipType.Vertical, "FLIP_IMG_VERTICALLY",
        "FLIP_IMG_VERTICALLY", CanExecute = "PixiEditor.HasDocument",
        MenuItemPath = "IMAGE/FLIP/FLIP_IMG_VERTICALLY", MenuItemOrder = 15, Icon = PixiPerfectIcons.Image180,
        AnalyticsTrack = true)]
    public void FlipImage(FlipType type) =>
        ActiveDocument?.Operations.FlipImage(type, activeDocument.AnimationDataViewModel.ActiveFrameBindable);

    [Command.Basic("PixiEditor.Document.FlipLayersHorizontal", FlipType.Horizontal, "FLIP_LAYERS_HORIZONTALLY",
        "FLIP_LAYERS_HORIZONTALLY", CanExecute = "PixiEditor.HasDocument",
        MenuItemPath = "LAYER/FLIP/FLIP_LAYERS_HORIZONTALLY", MenuItemOrder = 16,
        Icon = PixiPerfectIcons.MirrorHorizontal,
        AnalyticsTrack = true)]
    [Command.Basic("PixiEditor.Document.FlipLayersVertical", FlipType.Vertical, "FLIP_LAYERS_VERTICALLY",
        "FLIP_LAYERS_VERTICALLY", CanExecute = "PixiEditor.HasDocument",
        MenuItemPath = "LAYER/FLIP/FLIP_LAYERS_VERTICALLY", MenuItemOrder = 17, Icon = PixiPerfectIcons.MirrorVertical,
        AnalyticsTrack = true)]
    public void FlipLayers(FlipType type)
    {
        if (ActiveDocument?.SelectedStructureMember == null)
            return;

        ActiveDocument?.Operations.FlipImage(type, ActiveDocument.GetSelectedMembers(),
            activeDocument.AnimationDataViewModel.ActiveFrameBindable);
    }

    [Command.Basic("PixiEditor.Document.Rotate90Deg", "ROT_IMG_90",
        "ROT_IMG_90", CanExecute = "PixiEditor.HasDocument", Parameter = RotationAngle.D90,
        MenuItemPath = "IMAGE/ROTATION/ROT_IMG_90_D", MenuItemOrder = 8, Icon = PixiPerfectIcons.Image90,
        AnalyticsTrack = true)]
    [Command.Basic("PixiEditor.Document.Rotate180Deg", "ROT_IMG_180",
        "ROT_IMG_180", CanExecute = "PixiEditor.HasDocument", Parameter = RotationAngle.D180,
        MenuItemPath = "IMAGE/ROTATION/ROT_IMG_180_D", MenuItemOrder = 9, Icon = PixiPerfectIcons.Image180,
        AnalyticsTrack = true)]
    [Command.Basic("PixiEditor.Document.Rotate270Deg", "ROT_IMG_-90",
        "ROT_IMG_-90", CanExecute = "PixiEditor.HasDocument", Parameter = RotationAngle.D270,
        MenuItemPath = "IMAGE/ROTATION/ROT_IMG_-90_D", MenuItemOrder = 10, Icon = PixiPerfectIcons.ImageMinus90,
        AnalyticsTrack = true)]
    public void RotateImage(RotationAngle angle) => ActiveDocument?.Operations.RotateImage(angle);

    [Command.Basic("PixiEditor.Document.Rotate90DegLayers", "ROT_LAYERS_90",
        "ROT_LAYERS_90", CanExecute = "PixiEditor.HasDocument", Parameter = RotationAngle.D90,
        MenuItemPath = "LAYER/ROTATION/ROT_LAYERS_90_D", MenuItemOrder = 11, Icon = PixiPerfectIcons.File90,
        AnalyticsTrack = true)]
    [Command.Basic("PixiEditor.Document.Rotate180DegLayers", "ROT_LAYERS_180",
        "ROT_LAYERS_180", CanExecute = "PixiEditor.HasDocument", Parameter = RotationAngle.D180,
        MenuItemPath = "LAYER/ROTATION/ROT_LAYERS_180_D", MenuItemOrder = 12, Icon = PixiPerfectIcons.File180,
        AnalyticsTrack = true)]
    [Command.Basic("PixiEditor.Document.Rotate270DegLayers", "ROT_LAYERS_-90",
        "ROT_LAYERS_-90", CanExecute = "PixiEditor.HasDocument", Parameter = RotationAngle.D270,
        MenuItemPath = "LAYER/ROTATION/ROT_LAYERS_-90_D", MenuItemOrder = 13, Icon = PixiPerfectIcons.FileMinus90,
        AnalyticsTrack = true)]
    public void RotateLayers(RotationAngle angle)
    {
        if (ActiveDocument?.SelectedStructureMember == null)
            return;

        ActiveDocument?.Operations.RotateImage(angle, ActiveDocument.GetSelectedMembers(),
            activeDocument.AnimationDataViewModel.ActiveFrameBindable);
    }

    [Command.Basic("PixiEditor.Document.ToggleVerticalSymmetryAxis", "TOGGLE_VERT_SYMMETRY_AXIS",
        "TOGGLE_VERT_SYMMETRY_AXIS", CanExecute = "PixiEditor.HasDocument",
        Icon = PixiPerfectIcons.YSymmetry, AnalyticsTrack = true)]
    public void ToggleVerticalSymmetryAxis()
    {
        if (ActiveDocument is null)
            return;
        ActiveDocument.VerticalSymmetryAxisEnabledBindable ^= true;
    }

    [Command.Basic("PixiEditor.Document.ToggleHorizontalSymmetryAxis", "TOGGLE_HOR_SYMMETRY_AXIS",
        "TOGGLE_HOR_SYMMETRY_AXIS", CanExecute = "PixiEditor.HasDocument",
        Icon = PixiPerfectIcons.XSymmetry, AnalyticsTrack = true)]
    public void ToggleHorizontalSymmetryAxis()
    {
        if (ActiveDocument is null)
            return;
        ActiveDocument.HorizontalSymmetryAxisEnabledBindable ^= true;
    }

    [Command.Internal("PixiEditor.Document.DragSymmetry", CanExecute = "PixiEditor.HasDocument")]
    public void DragSymmetry(SymmetryAxisDragInfo info)
    {
        if (ActiveDocument is null)
            return;
        ActiveDocument.EventInlet.OnSymmetryDragged(info);
    }

    [Command.Internal("PixiEditor.Document.StartDragSymmetry", CanExecute = "PixiEditor.HasDocument",
        AnalyticsTrack = true)]
    public void StartDragSymmetry(SymmetryAxisDirection dir)
    {
        if (ActiveDocument is null)
            return;
        ActiveDocument.EventInlet.OnSymmetryDragStarted(dir);
        ActiveDocument.Tools.UseSymmetry(dir);
    }

    [Command.Internal("PixiEditor.Document.EndDragSymmetry", CanExecute = "PixiEditor.HasDocument")]
    public void EndDragSymmetry(SymmetryAxisDirection dir)
    {
        if (ActiveDocument is null)
            return;
        ActiveDocument.EventInlet.OnSymmetryDragEnded(dir);
    }

    [Command.Basic("PixiEditor.Document.DeleteSelected", "DELETE_SELECTED", "DELETE_SELECTED_DESCRIPTIVE",
        Key = Key.Delete,
        ShortcutContexts = [typeof(ViewportWindowViewModel)],
        Icon = PixiPerfectIcons.Eraser,
        MenuItemPath = "EDIT/DELETE_SELECTED", MenuItemOrder = 6, AnalyticsTrack = true)]
    public void DeleteSelected()
    {
        if (ActiveDocument is null)
            return;

        if (ActiveDocument.SelectionPathBindable != null && ActiveDocument.SelectionPathBindable is { IsEmpty: false })
        {
            ActiveDocument.Operations.DeleteSelectedPixels(ActiveDocument.AnimationDataViewModel.ActiveFrameBindable);
        }
        else
        {
            var selectedMembers = ActiveDocument?.GetSelectedMembers();
            if (selectedMembers == null || selectedMembers.Count == 0)
                return;

            ActiveDocument?.Operations.DeleteStructureMembers(selectedMembers);
        }
    }

    [Command.Basic("PixiEditor.Document.ResizeDocument", false, "RESIZE_DOCUMENT", "RESIZE_DOCUMENT",
        CanExecute = "PixiEditor.HasDocument", Key = Key.I, Modifiers = KeyModifiers.Control | KeyModifiers.Shift,
        Icon = PixiPerfectIcons.Resize, MenuItemPath = "IMAGE/RESIZE_IMAGE", MenuItemOrder = 0, AnalyticsTrack = true)]
    [Command.Basic("PixiEditor.Document.ResizeCanvas", true, "RESIZE_CANVAS", "RESIZE_CANVAS",
        CanExecute = "PixiEditor.HasDocument", Key = Key.C, Modifiers = KeyModifiers.Control | KeyModifiers.Shift,
        Icon = PixiPerfectIcons.CanvasResize, MenuItemPath = "IMAGE/RESIZE_CANVAS", MenuItemOrder = 1,
        AnalyticsTrack = true)]
    public async Task OpenResizePopup(bool canvas)
    {
        DocumentViewModel? doc = Owner.DocumentManagerSubViewModel.ActiveDocument;
        if (doc is null)
            return;

        ResizeDocumentDialog dialog = new ResizeDocumentDialog(
            doc.Width,
            doc.Height,
            MainWindow.Current!,
            canvas);
        if (await dialog.ShowDialog())
        {
            if (canvas)
            {
                doc.Operations.ResizeCanvas(new(dialog.Width, dialog.Height), dialog.ResizeAnchor);
            }
            else
            {
                doc.Operations.ResizeImage(new(dialog.Width, dialog.Height), ResamplingMethod.NearestNeighbor);
            }
        }
    }

    [Command.Basic("PixiEditor.Document.CenterContent", "CENTER_CONTENT", "CENTER_CONTENT",
        CanExecute = "PixiEditor.HasDocument",
        Icon = PixiPerfectIcons.Center, MenuItemPath = "IMAGE/CENTER_CONTENT", MenuItemOrder = 3,
        AnalyticsTrack = true)]
    public void CenterContent()
    {
        if (ActiveDocument?.SelectedStructureMember == null)
            return;

        ActiveDocument.Operations.CenterContent(ActiveDocument.GetSelectedMembers(),
            activeDocument.AnimationDataViewModel.ActiveFrameBindable);
    }

    [Command.Basic("PixiEditor.Document.UseLinearSrgbProcessing", "USE_LINEAR_SRGB_PROCESSING",
        "USE_LINEAR_SRGB_PROCESSING_DESC", CanExecute = "PixiEditor.DocumentUsesSrgbBlending",
        AnalyticsTrack = true)]
    public void UseLinearSrgbProcessing()
    {
        if (ActiveDocument is null)
            return;

        ActiveDocument.Operations.UseLinearSrgbProcessing();
    }

    [Command.Basic("PixiEditor.Document.UseSrgbProcessing", "USE_SRGB_PROCESSING",
        "USE_SRGB_PROCESSING_DESC", CanExecute = "PixiEditor.DocumentUsesLinearBlending",
        AnalyticsTrack = true)]
    public void UseSrgbProcessing()
    {
        if (ActiveDocument is null)
            return;

        ActiveDocument.Operations.UseSrgbProcessing();
    }

    [Command.Internal("PixiEditor.Document.LoadLazyDocument")]
    public void LoadLazyDocument(LazyDocumentViewModel lazyDocument)
    {
        if (lazyDocument == null)
            return;

        Owner.FileSubViewModel.LoadLazyDocument(lazyDocument);
        LazyDocuments.Remove(lazyDocument);
        Owner.WindowSubViewModel.CloseViewportForLazyDocument(lazyDocument);
    }

    [Evaluator.CanExecute("PixiEditor.DocumentUsesSrgbBlending", nameof(ActiveDocument),
        nameof(ActiveDocument.UsesSrgbBlending))]
    public bool DocumentUsesSrgbBlending() => ActiveDocument?.UsesSrgbBlending ?? false;

    [Evaluator.CanExecute("PixiEditor.DocumentUsesLinearBlending", nameof(ActiveDocument),
        nameof(ActiveDocument.UsesSrgbBlending))]
    public bool DocumentUsesLinearBlending() => !ActiveDocument?.UsesSrgbBlending ?? true;

    public void Add(DocumentViewModel doc)
    {
        Documents.Add(doc);
        DocumentAdded?.Invoke(doc);
    }

    public void ReloadDocumentReference(Guid referenceId, string fullPath)
    {
        if (string.IsNullOrEmpty(fullPath) || !Path.Exists(fullPath))
            return;

        Dispatcher.UIThread.Post(() =>
        {
            var loaded = Documents.FirstOrDefault(x => x.FullFilePath == fullPath) ??
                         FileViewModel.ImportFromPath(fullPath);
            foreach (var doc in Documents)
            {
                if (doc.FullFilePath == fullPath)
                    continue;

                doc.UpdateDocumentReferences(referenceId, loaded);
            }
        });
    }

    private void OnDocumentReferenceDeleted(Guid referenceId)
    {
        Dispatcher.UIThread.Post(() =>
        {
            foreach (var doc in Documents)
            {
                doc.UpdateNestedLinkedStatus(referenceId);
            }
        });
    }

    public void AddDocumentReference(Guid documentId, Guid nodeId, string? originalPath, Guid docReferenceId)
    {
        var existingReference = documentReferences.FirstOrDefault(x =>
             (!string.IsNullOrEmpty(originalPath) && x.OriginalFilePath == originalPath) || x.ReferenceId == docReferenceId);
        if (existingReference != null)
        {
            existingReference.AddReferencingNode(documentId, nodeId);
            return;
        }

        if (existingReference == null)
        {
            var newReference = new DocumentReferenceData(originalPath, docReferenceId);
            newReference.ReferencingNodes[documentId] = new HashSet<Guid> { nodeId };
            documentReferences.Add(newReference);
            newReference.DocumentChanged += ReloadDocumentReference;
            newReference.LinkedDocumentDeleted += OnDocumentReferenceDeleted;
        }
    }

    public void RemoveDocumentReferenceByNodeId(Guid documentId, Guid infoNodeId)
    {
        var reference = documentReferences.FirstOrDefault(x => x.ReferencingNodes.ContainsKey(documentId) &&
                                                               x.ReferencingNodes[documentId].Contains(infoNodeId));
        if (reference != null)
        {
            reference.ReferencingNodes[documentId].Remove(infoNodeId);
            if (reference.ReferencingNodes[documentId].Count == 0)
            {
                reference.ReferencingNodes.Remove(documentId);
                reference.Dispose();
                documentReferences.Remove(reference);
            }
        }
    }

    public void RemoveDocumentReferences(Guid documentId, IEnumerable<Guid> ids)
    {
        foreach (var id in ids)
        {
            RemoveDocumentReferenceByNodeId(documentId, id);
        }
    }

    public void ReloadReference(DocumentViewModel document)
    {
        var references = documentReferences.ToList();
        foreach (var reference in references)
        {
            if (reference.ReferenceId == document.ReferenceId)
            {
                var docs = reference.ReferencingNodes.Keys;
                foreach (var doc in docs)
                {
                    var documentVm = Documents.FirstOrDefault(x => x.Id == doc);
                    documentVm?.UpdateDocumentReferences(reference.ReferenceId, document);
                }
            }
        }
    }
}

public class DocumentReferenceData : IDisposable, IDocumentReferenceData
{
    public Dictionary<Guid, HashSet<Guid>> ReferencingNodes { get; } = new();
    public string? OriginalFilePath { get; set; }
    public Guid ReferenceId { get; set; }

    public FileSystemWatcher? Watcher { get; set; }

    public event Action<Guid, string>? DocumentChanged;
    public event Action<Guid>? LinkedDocumentDeleted;


    public DocumentReferenceData(string? originalFilePath, Guid referenceId)
    {
        OriginalFilePath = originalFilePath;
        ReferenceId = referenceId;
        TryCreateFileWatcher();
    }

    public void AddReferencingNode(Guid document, Guid nodeId)
    {
        if (!ReferencingNodes.ContainsKey(document))
        {
            ReferencingNodes[document] = new HashSet<Guid>();
        }

        ReferencingNodes[document].Add(nodeId);
    }

    public void RemoveReferencingNode(Guid document, Guid nodeId)
    {
        if (ReferencingNodes.ContainsKey(document))
        {
            ReferencingNodes[document].Remove(nodeId);
            if (ReferencingNodes[document].Count == 0)
            {
                ReferencingNodes.Remove(document);
            }
        }
    }

    public void TryCreateFileWatcher()
    {
        if (!string.IsNullOrEmpty(OriginalFilePath))
        {
            try
            {
                var dirPath = System.IO.Path.GetDirectoryName(OriginalFilePath);
                Watcher = new FileSystemWatcher(dirPath);
                Watcher.Filter = System.IO.Path.GetFileName(OriginalFilePath);

                Watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName;
                Watcher.IncludeSubdirectories = false;

                /*Watcher.Changed += (s, e) =>
                {
                    DocumentChanged?.Invoke(ReferenceId, e.FullPath);
                };*/

                Watcher.Renamed += (s, e) =>
                {
                    DocumentChanged?.Invoke(ReferenceId, e.FullPath);
                    Watcher.EnableRaisingEvents = false;

                    Watcher.Filter = System.IO.Path.GetFileName(e.FullPath);
                    Watcher.EnableRaisingEvents = true;
                };

                Watcher.Deleted += (sender, args) =>
                {
                    LinkedDocumentDeleted?.Invoke(ReferenceId);
                };

                Watcher.EnableRaisingEvents = true;
            }
            catch (Exception)
            {
                // Ignore errors with file watchers
            }
        }
        else
        {
            // TODO: Nested document references without paths
        }
    }

    public void Dispose()
    {
        Watcher?.Dispose();
    }
}
