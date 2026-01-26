namespace Nativno.Bluetooth.Mac.Playground.Helpers;

public static class PeripheralHelper
{
    public static void SetupToReadValuesOnCharacteristicDiscovery(this CBPeripheral peripheral)
    {
        peripheral.DiscoveredCharacteristics += (_, e) =>
        {
            foreach (var ch in e.Service.Characteristics!)
            {
                peripheral.ReadValue(ch);
                peripheral.SetNotifyValue(true, ch);
            }
        };
    }
}

public class SelectedService
{
    public CBService? Instance { get; set; }
}

public class TrackedPeripheral(CBDiscoveredPeripheralEventArgs discovery, DateTime discoveredAt)
{
    public CBDiscoveredPeripheralEventArgs Discovery { get; private set; } = discovery;
    public DateTime DiscoveredAt { get; private set; } = discoveredAt;
    public PeripheralConnectionState ConnectionState { get; set; } = PeripheralConnectionState.Disconnected;

    public void RegisterNewDiscovery(CBDiscoveredPeripheralEventArgs discovery, DateTime discoveredAt)
    {
        Discovery = discovery;
        DiscoveredAt = discoveredAt;
    }
}

public class TrackedPeripheralCollection
{
    public Dictionary<string, TrackedPeripheral> Items { get; } = new ();
    public Notifier<TrackedPeripheral> ConnectionStatusUpdatedNotifier { get; } = new ();
    public Notifier ListUpdatedNotifier { get; } = new ();

    public void SubscribeToPeripheralsDiscoveries(CBCentralManager manager)
    {
        manager.DiscoveredPeripheral += (_, e) =>
        {
            RegisterDiscoveryOnNamedPeripheral(e);
        };
    }

    public void RegisterDiscoveryOnNamedPeripheral(CBDiscoveredPeripheralEventArgs e)
    {
        var id = e.Peripheral.Identifier.ToString();
        var name = e.Peripheral.Name;

        if (String.IsNullOrWhiteSpace(name))
        {
            return;
        }

        if (Items.TryGetValue(id, out var existing))
        {
            existing.RegisterNewDiscovery(e, DateTime.UtcNow);
        }
        else
        {
            Items[id] = new TrackedPeripheral(e, DateTime.UtcNow);
        }

        ListUpdatedNotifier.Notify();
    }

    public void RemoveOldDiscoveries(TimeSpan maxAge)
    {
        var threshold = DateTime.UtcNow - maxAge;
        var toRemove = Items
            .Where(kv => kv.Value.DiscoveredAt < threshold)
            .Select(kv => kv.Key);

        foreach (var id in toRemove)
        {
            Items.Remove(id);
        }
    }

    public TrackedPeripheral Tracked(CBPeripheral peripheral)
    {
        var id = peripheral.Identifier.ToString();
        return Items[id];
    }

    public void SubscribeToConnectionStatuses(CBCentralManager manager)
    {
        manager.ConnectedPeripheral += (_, e) => UpdateConnectionStatus(e.Peripheral, PeripheralConnectionState.Connected);
        manager.DisconnectedPeripheral += (_, e) =>  UpdateConnectionStatus(e.Peripheral, PeripheralConnectionState.Disconnected);
        manager.DidDisconnectPeripheral += (_, e) => UpdateConnectionStatus(e.Peripheral, PeripheralConnectionState.Disconnected);
        manager.FailedToConnectPeripheral += (_, e) => UpdateConnectionStatus(e.Peripheral, PeripheralConnectionState.Disconnected);
    }

    public void UpdateConnectionStatus(CBPeripheral peripheral, PeripheralConnectionState state)
    {
        var tracked = Tracked(peripheral);
        tracked.ConnectionState = state;

        ConnectionStatusUpdatedNotifier.Notify(tracked);
    }
}

public enum PeripheralConnectionState
{
    Disconnected,
    Connecting,
    Connected
}