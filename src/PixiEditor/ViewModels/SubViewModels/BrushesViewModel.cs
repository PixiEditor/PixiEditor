using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Brushes;
using PixiEditor.Helpers;
using PixiEditor.Helpers.Extensions;
using PixiEditor.Models.BrushEngine;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.Dialogs;
using PixiEditor.Models.IO;
using PixiEditor.UI.Common.Localization;
using PixiEditor.ViewModels.BrushSystem;
using PixiEditor.ViewModels.Document;
using PixiEditor.ViewModels.Document.Nodes.Brushes;

namespace PixiEditor.ViewModels.SubViewModels;

internal class BrushesViewModel : SubViewModel<ViewModelMain>
{
    public BrushLibrary BrushLibrary { get; private set; }

    public BrushesViewModel(ViewModelMain owner) : base(owner)
    {
        if (!Directory.Exists(Paths.PathToBrushesFolder))
        {
            Directory.CreateDirectory(Paths.PathToBrushesFolder);
        }

        BrushLibrary = new BrushLibrary(Paths.PathToBrushesFolder);
        Owner.BeforeDocumentClosed += OwnerOnBeforeDocumentClosed;
        Owner.DocumentManagerSubViewModel.DocumentAdded += AddDocumentBrushes;
        owner.AttachedToWindow += window =>
        {
            if (!window.IsLoaded)
            {
                window.Loaded += (_, _) => LoadBrushLibrary();
            }
            else
            {
                LoadBrushLibrary();
            }
        };
    }

    private void AddDocumentBrushes(DocumentViewModel obj)
    {
        var brushNodes = obj.NodeGraph.AllNodes.OfType<BrushOutputNodeViewModel>().ToList();
        if (brushNodes != null)
        {
            foreach (var node in brushNodes)
            {
                string name = node.Inputs.FirstOrDefault(x => x.PropertyName == BrushOutputNode.BrushNameProperty)
                    ?.Value?.ToString() ?? "Unnamed";

                BrushLibrary.Add(
                    new Brush(name, node.Document));
            }
        }
    }

    private void OwnerOnBeforeDocumentClosed(DocumentViewModel doc)
    {
        UnregisterBrushes(doc);
    }

    private void UnregisterBrushes(DocumentViewModel document)
    {
        var brushNodes = document.NodeGraph.AllNodes.OfType<BrushOutputNodeViewModel>().ToList();
        if (brushNodes != null)
        {
            foreach (var node in brushNodes)
            {
                BrushLibrary.RemoveById(node.Id);
            }
        }
    }

    private void LoadBrushLibrary()
    {
        BrushLibrary.LoadBrushes();
    }

    [Command.Internal("PixiEditor.Brushes.Delete", "DELETE_BRUSH")]
    public void DeleteBrush(BrushViewModel brushViewModel)
    {
        try
        {
            if(brushViewModel == null || brushViewModel.IsReadOnly || brushViewModel.Brush == null)
                return;

            var directory = Path.GetDirectoryName(brushViewModel.Brush.FilePath);
            if (directory == null || !Directory.Exists(directory) ||
                !brushViewModel.Brush.FilePath.StartsWith(Paths.PathToBrushesFolder))
            {
                return;
            }

            File.Delete(brushViewModel.Brush.FilePath);
        }
        catch (Exception e)
        {
            NoticeDialog.Show(
                new LocalizedString("ERROR_DELETING_BRUSH_MESSAGE", e.Message),
                "ERROR_DELETING_BRUSH_TITLE");
        }
    }

    [Command.Basic("PixiEditor.Brushes.Import", "IMPORT_BRUSH", "IMPORT_BRUSH_DESCRIPTIVE")]
    public void ImportBrush()
    {
        var options = new FileTypeDialogDataSet(FileTypeDialogDataSet.SetKind.Pixi).GetFormattedTypes(true);

        Application.Current.ForDesktopMainWindowAsync(async window =>
        {
            var dialog = await window.StorageProvider.OpenFilePickerAsync(
                new FilePickerOpenOptions { FileTypeFilter = options });

            if (dialog.Count == 0 || !Importer.IsSupportedFile(dialog[0].Path.LocalPath))
                return;

            try
            {
                if (dialog.Count > 0)
                {
                    string fullPath = Path.Combine(Paths.PathToBrushesFolder,
                        System.IO.Path.GetFileName(dialog[0].Path.LocalPath));

                    string uniqueName = FileHelper.GetUniqueFileName(fullPath);

                    File.Copy(dialog[0].Path.LocalPath, uniqueName);
                }
            }
            catch (Exception e)
            {
                NoticeDialog.Show(
                    "ERROR_IMPORTING_BRUSH_TITLE",
                    new LocalizedString("ERROR_IMPORTING_BRUSH_MESSAGE", e.Message));
            }
        });
    }
}
