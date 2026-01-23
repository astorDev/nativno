using CommunityToolkit.Maui;
using CoreBluetooth;
using CoreFoundation;
using Microsoft.Extensions.Logging;
using Nativno.Bluetooth.Mac.Playground.Helpers;

namespace Nativno.Bluetooth.Mac.Playground;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseMauiCommunityToolkit()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
			});

		builder.Services.AddMauiBlazorWebView();
		builder.Services.AddSingleton<PeripheralsCollection>();
		builder.Services.AddSingleton<CBCentralManager>(_ => new (dispatchQueue: DispatchQueue.MainQueue));
		builder.Services.AddSingleton<SelectedService>();

#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}

public class PeripheralsCollection(ILogger<PeripheralsCollection> logger)
{
	public CBPeripheral? Connected { get; set; }
	public string? ConnectedId => Connected?.Identifier.ToString();
	public Dictionary<string,  CBDiscoveredPeripheralEventArgs> Discoveries { get; } = new ();

	public void PutIfPeripheralIsNamed(CBDiscoveredPeripheralEventArgs e)
	{
		var id = e.Peripheral.Identifier.ToString();
		var name = e.Peripheral.Name;
            
		if (String.IsNullOrWhiteSpace(name))
		{
			logger.LogDebug("Skipping peripheral without name {Peripheral}", e.Peripheral);
			return;
		}
            
		Discoveries[id] = e;
	}

	public void SubscribeOnConnectionEvent(CBCentralManager central)
	{
		central.ConnectedPeripheral += (_, e) => Connected = e.Peripheral;
	}
}