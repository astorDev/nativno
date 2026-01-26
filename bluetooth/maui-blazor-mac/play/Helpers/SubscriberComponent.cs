namespace Nativno.Bluetooth.Mac.Playground.Helpers;

public static class SubscriptionCollectionExtensions
{
    public static void DisposeAll(this IEnumerable<IDisposable> subscriptions)
    {
        foreach (var subscription in subscriptions)
        {
            subscription.Dispose();
        }
    }

    public static void AddTo(this IDisposable subscription, ICollection<IDisposable> collection)
    {
        collection.Add(subscription);
    }
}

public class SubscriberComponent : ComponentBase, IDisposable
{
    protected List<IDisposable> subscriptions = new ();

    public void Dispose()
    {
        DisposeAdditionally();
        subscriptions.DisposeAll();
        GC.SuppressFinalize(this);
    }

    public virtual void DisposeAdditionally()
    {}

    public void SubscribeTo(IListenable listenable, Action onEvent)
    {
        listenable.AddListener(onEvent).AddTo(subscriptions);
    }

    public void SubscribeTo<T>(IListenable<T> listenable, Action<T> onEvent)
    {
        var subscription = listenable.AddListener(onEvent);
        subscriptions.Add(subscription);
    }

    public void SubscribeStateHasChangedTo(IListenable listenable)
    {
        listenable.AddListener(() => InvokeAsync(StateHasChanged)).AddTo(subscriptions);
    }

    public void SubscribeStateHasChangedTo<T>(IListenable<T> listenable)
    {
        listenable.AddListener((_) => InvokeAsync(StateHasChanged)).AddTo(subscriptions);
    }
}