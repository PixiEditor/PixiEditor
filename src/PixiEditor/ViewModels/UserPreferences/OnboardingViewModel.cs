namespace PixiEditor.ViewModels.UserPreferences;

public class OnboardingViewModel : PixiObservableObject
{
    private int page;
    public int Page
    {
        get => page;
        set
        {
            SetProperty(ref page, value);
        }
    }
}
