namespace PixiEditor.Models.Handlers;

public interface IGuidesHandler
{
    public IEnumerable<IGuideHandler> GuidesHandler { get; }

    public void AddGuide(bool vertical, double offset);
    public void RemoveGuide(IGuideHandler guide);
    void AddGuideInternal(IGuideHandler guideViewModel);
    void RemoveGuideInternal(IGuideHandler guide);
}
