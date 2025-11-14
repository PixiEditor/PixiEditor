using PixiEditor.ChangeableDocument;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.Models.Handlers;
using PixiEditor.ViewModels.Document;

namespace PixiEditor.Models.DocumentModels;
#nullable enable
internal class DocumentInternalParts
{
    public DocumentInternalParts(IDocument doc, IServiceProvider services)
    {
        Tracker = new DocumentChangeTracker();
        StructureHelper = new DocumentStructureHelper(doc, this);
        Updater = new DocumentUpdater(doc, this);
        ActionAccumulator = new ActionAccumulator(doc, this);
        State = new DocumentState();
        ChangeController = new ChangeExecutionController(doc, this, services);
    }

    public DocumentInternalParts(IDocument doc, IServiceProvider currentServices, IReadOnlyDocument readOnlyDocument)
    {
        Tracker = new DocumentChangeTracker(readOnlyDocument);
        StructureHelper = new DocumentStructureHelper(doc, this);
        Updater = new DocumentUpdater(doc, this);
        ActionAccumulator = new ActionAccumulator(doc, this);
        State = new DocumentState();
        ChangeController = new ChangeExecutionController(doc, this, currentServices);
    }

    public ActionAccumulator ActionAccumulator { get; }
    public DocumentChangeTracker Tracker { get; }
    public DocumentStructureHelper StructureHelper { get; }
    public DocumentUpdater Updater { get; }
    public DocumentState State { get; }
    public ChangeExecutionController ChangeController { get; }
}
