using PixiEditor.UI.Common.Localization;
using PixiEditor.ViewModels.Document.Properties;

namespace PixiEditor.ViewModels.Document;

internal interface IPropertiesObject
{
    LocalizedString LabelKey { get; }
    public IReadOnlyCollection<PropertyViewModel> Properties { get; }
    event Action<IReadOnlyCollection<PropertyViewModel>> PropertiesUpdated;
    void StartUpdates();
    void StopUpdates();
}
