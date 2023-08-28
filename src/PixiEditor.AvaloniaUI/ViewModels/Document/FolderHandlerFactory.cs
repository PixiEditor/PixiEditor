﻿using PixiEditor.AvaloniaUI.Helpers;
using PixiEditor.AvaloniaUI.Models.DocumentModels;
using PixiEditor.AvaloniaUI.Models.Handlers;

namespace PixiEditor.AvaloniaUI.ViewModels.Document;

internal class FolderHandlerFactory : IFolderHandlerFactory
{
    public DocumentViewModel Document { get; }
    IDocument IFolderHandlerFactory.Document => Document;

    public FolderHandlerFactory(DocumentViewModel document)
    {
        Document = document;
    }

    public IFolderHandler CreateFolderHandler(DocumentInternalParts helper, Guid infoGuidValue)
    {
        return new FolderViewModel(Document, helper, infoGuidValue);
    }
}