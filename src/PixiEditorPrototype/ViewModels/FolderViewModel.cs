using System;
using System.Collections.ObjectModel;
using PixiEditorPrototype.Models;

namespace PixiEditorPrototype.ViewModels;

internal class FolderViewModel : StructureMemberViewModel
{
    public ObservableCollection<StructureMemberViewModel> Children { get; } = new();
    public FolderViewModel(DocumentViewModel doc, DocumentHelpers helpers, Guid guidValue) : base(doc, helpers, guidValue) { }
}
