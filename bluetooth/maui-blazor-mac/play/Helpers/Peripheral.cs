using CoreBluetooth;

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