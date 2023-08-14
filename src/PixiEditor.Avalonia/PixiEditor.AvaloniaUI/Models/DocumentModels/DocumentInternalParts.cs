using System.Collections.Generic;
using PixiEditor.AvaloniaUI.Models.Handlers;
using PixiEditor.ChangeableDocument;

namespace PixiEditor.AvaloniaUI.Models.DocumentModels;
#nullable enable
internal class DocumentInternalParts
{
    public DocumentInternalParts(IDocument doc, List<IHandler> handlers)
    {
        Tracker = new DocumentChangeTracker();
        StructureHelper = new DocumentStructureHelper(doc, this);
        Updater = new DocumentUpdater(doc, this);
        ActionAccumulator = new ActionAccumulator(doc, this);
        State = new DocumentState();
        ChangeController = new ChangeExecutionController(doc, this, handlers);
    }
    public ActionAccumulator ActionAccumulator { get; }
    public DocumentChangeTracker Tracker { get; }
    public DocumentStructureHelper StructureHelper { get; }
    public DocumentUpdater Updater { get; }
    public DocumentState State { get; }
    public ChangeExecutionController ChangeController { get; }
}
