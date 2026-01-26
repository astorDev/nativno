namespace Nativno.Bluetooth.Mac.Playground.Helpers;

public class Notifier<TEventArgs> : IListenable<TEventArgs>
{
    private readonly List<Action<TEventArgs>> listeners = new ();

    public IDisposable AddListener(Action<TEventArgs> listener)
    {
        listeners.Add(listener);
        return new Subscription(() => listeners.Remove(listener));
    }

    public void Notify(TEventArgs args)
    {
        foreach (var listener in listeners.ToList()) // materializes the list to avoid modification during iteration
        {
            listener(args);
        }
    }

    private class Subscription(Action unsubscribe) : IDisposable
    {
        public void Dispose() => unsubscribe();
    }
}

public class Notifier : IListenable
{
    private readonly List<Action> listeners = new ();

    public IDisposable AddListener(Action listener)
    {
        listeners.Add(listener);
        return new Subscription(() => listeners.Remove(listener));
    }

    public void Notify()
    {
        foreach (var listener in listeners.ToList()) // materializes the list to avoid modification during iteration
        {
            listener();
        }
    }

    private class Subscription(Action unsubscribe) : IDisposable
    {
        public void Dispose() => unsubscribe();
    }
}

public interface IListenable
{
    IDisposable AddListener(Action listener);
}

public interface IListenable<T>
{
    IDisposable AddListener(Action<T> listener);
}