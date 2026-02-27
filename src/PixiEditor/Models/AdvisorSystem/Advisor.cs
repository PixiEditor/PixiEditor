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

    public void RequestAdvice(string adviceName)
    {
        if (adviceStorage.TryGetValue(adviceName, out Advice advice))
        {
            SendAdvice(adviceName, advice);
        }
    }

    private void SendAdvice(string adviceName, Advice advice)
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
    public void RequestAdvice(string adviceName);
}

public interface IAdviceListener
{
    public void OnAdviceReceived(Advice advice);
}

public class Advice
{
    public string Name { get; }
    public LocalizedString Content { get; }
    public event Action? UserDismissed;

    public Advice(string name, LocalizedString content)
    {
        Name = name;
        Content = content;
    }

    public void Dismiss()
    {
        UserDismissed?.Invoke();
    }
}
