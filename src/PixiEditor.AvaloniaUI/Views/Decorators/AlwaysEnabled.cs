using Avalonia;
using Avalonia.Controls;

namespace PixiEditor.AvaloniaUI.Views.Decorators;

public class AlwaysEnabled : ContentControl
{
    static AlwaysEnabled()
    {
        IsEnabledProperty.OverrideMetadata(
            typeof(AlwaysEnabled), /*TODO: Validate if coerce is needed*/
            new StyledPropertyMetadata<bool>(true, coerce: (_, x) => x));
    }
}
