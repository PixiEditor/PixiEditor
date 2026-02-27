using PixiEditor.UI.Common.Localization;

namespace PixiEditor.Models.AdvisorSystem;

public class Advisor : IAdvisor
{
    private Dictionary<string, Advice> adviceStorage = new Dictionary<string, Advice>();
    private Dictionary<string, IAdviceListener> listeners = new Dictionary<string, IAdviceListener>();

    public Advisor()
    {
    }

    public void RegisterAdvice(string adviceName, Advice advice)
    {
        adviceStorage[adviceName] = advice;
    }

    public Advice? RequestAdvice(string adviceName)
    {
        if (adviceStorage.TryGetValue(adviceName, out Advice advice))
        {
            advice.Choices?.Clear();
            return advice;
        }

        return null;
    }

    public void SendAdvice(string adviceName, Advice advice)
    {
        if (listeners.TryGetValue(adviceName, out IAdviceListener adviceListener))
        {
            adviceListener.OnAdviceReceived(advice);
        }
    }

    public void SubscribeToAdvisor(string adviceName, IAdviceListener listener)
    {
        listeners[adviceName] = listener;
    }

    public void Activate()
    {
        IAdvisor.SetCurrent(this);
    }
}

public interface IAdvisor
{
    public static IAdvisor Current { get; private set; }

    static void SetCurrent(IAdvisor advisor)
    {
        if (Current != null)
        {
            throw new InvalidOperationException("An instance of Advisor already exists. Only one instance is allowed.");
        }

        Current = advisor;
    }

    public void SubscribeToAdvisor(string adviceName, IAdviceListener listener);
    public void Activate();
    public void RegisterAdvice(string adviceName, Advice advice);
    public Advice? RequestAdvice(string adviceName);
    public void SendAdvice(string adviceName, Advice advice);
}

public interface IAdviceListener
{
    public void OnAdviceReceived(Advice advice);
}

public class Advice
{
    public string Name { get; }
    public LocalizedString Content { get; }
    public List<LocalizedString> Choices { get; private set; }
    public Action<int>? ChoiceSelected { get; private set; }
    public Advice? NextAdvice { get; private set; }
    public event Action? UserDismissed;

    public Advice(string name, LocalizedString content)
    {
        Name = name;
        Content = content;
    }

    public void Dismiss(int? choice = null)
    {
        if (choice.HasValue && ChoiceSelected != null)
        {
            ChoiceSelected.Invoke(choice.Value);
        }

        UserDismissed?.Invoke();
    }

    public Advice WithChoices(params LocalizedString[] choices)
    {
        Choices = choices.ToList();
        return this;
    }

    public Advice OnChoiceSelected(Action<int> callback)
    {
        ChoiceSelected = callback;
        return this;
    }

    public Advice WithFollowUpAdvice(Advice advice)
    {
        NextAdvice = advice;
        return this;
    }
}
