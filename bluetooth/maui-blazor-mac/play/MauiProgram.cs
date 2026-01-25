using System.Reflection;
using CommunityToolkit.Maui;
using CoreBluetooth;
using CoreFoundation;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Nativno.Bluetooth.Mac.Playground.Components.Pages;
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
		builder.Services.AddSingleton<TrackedPeripheralCollection>();

#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
#endif
		
#if DEBUG
		return builder.BuildAndValidateBlazor();
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

public static class MauiAppBuilderExtension
{
	extension(MauiAppBuilder builder)
	{
		public MauiApp BuildWithRegisteringAndValidating<TComponent>() where TComponent : ComponentBase
		{
			builder.Services.AddTransient<TComponent>();

			var app = builder.Build();

			var home = app.Services.GetRequiredService<TComponent>();
		
			return app;
		}

		public MauiApp BuildAndValidateBlazor()
		{
			var app = builder.Build();
			
			app.Services.ResolveAllInjectedInExecutingAssemblyComponents();

			return app;
		}
	}

	extension(IServiceProvider provider)
	{
		public void ResolveAllInjectedInExecutingAssemblyComponents()
		{
			var components = Assembly.GetExecutingAssembly()
				.GetTypes()
				.Where(t => t.IsSubclassOf(typeof(ComponentBase)))
				.ToList();

			var injectedTypes = components
				.SelectMany(c => c.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
					.Where(p => p.GetCustomAttributes(typeof(InjectAttribute), true).Any())
					.Select(p => p.PropertyType))
				.Distinct()
				.ToList();

			foreach (var type in injectedTypes)
			{
				var resolved = provider.GetRequiredService(type);
			}
		}
	}
}