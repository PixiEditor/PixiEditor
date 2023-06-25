using System.Windows;
using System.Windows.Controls;

namespace PixiEditor.Views.UserControls;

public class AlwaysEnabled : ContentControl
{
    static AlwaysEnabled()
    {
        IsEnabledProperty.OverrideMetadata(
            typeof(AlwaysEnabled),
            new UIPropertyMetadata(
                defaultValue: true,
                propertyChangedCallback: (_, __) => { },
                coerceValueCallback: (_, x) => x));
    }
}
