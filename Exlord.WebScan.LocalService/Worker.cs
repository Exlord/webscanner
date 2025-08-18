using EmbedIO;
using EmbedIO.Actions;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Win32;
using NAPS2.Escl;
using NAPS2.Escl.Server;
using NAPS2.Images;
using NAPS2.Images.ImageSharp;
using NAPS2.Remoting.Server;
using NAPS2.Scan;
using NAPS2.Threading;
using NAPS2.Wia;
using NTwain.Data;
using Swan.Logging;
using System.Runtime.InteropServices;

namespace Exlord.WebScan.LocalService;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    // Our web server is disposable.
    private ScanServer _scanServer;
    CancellationToken _stoppingToken;


    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _stoppingToken = stoppingToken;
        var url = "http://localhost:9881/";
        using (var server = CreateDeviceServer(url))
        {
            Console.WriteLine(" ============ device server created");
            server.Start(stoppingToken);
            Console.WriteLine(" ============ device server started");
            //CreateScanServer();
            await stoppingToken.WaitHandle.WaitOneAsync();
            if (_scanServer != null)
            {
                await _scanServer.Stop();
                _scanServer = null;
            }
        }
    }

    // Create and configure our web server.
    private WebServer CreateDeviceServer(string url)
    {
        var server = new WebServer(ws => ws
            .WithUrlPrefix(url)
            .WithMode(HttpListenerMode.EmbedIO))
            .WithCors("*")
            // action to get a list of devices
            .WithModule(new ActionModule("/devices", HttpVerbs.Get, async ctx =>
            {
                Console.WriteLine(" ======= gettting device list");
                using var scanningContext = new ScanningContext(new ImageSharpImageContext());
                scanningContext.Logger = _logger;
                scanningContext.SetUpWin32Worker();
                var controller = new ScanController(scanningContext);

                var wiaDevices = await controller.GetDeviceList(Driver.Wia);
                var twainDevices = await controller.GetDeviceList(Driver.Twain);

                await ctx.SendDataAsync(wiaDevices.Concat(twainDevices).ToArray());
            }))
            // action to save selected device 
            .WithModule(new ActionModule("/devices", HttpVerbs.Put, async ctx =>
            {
                Console.WriteLine(" ======= got new device");
                Globals.SelectedDevice = null;
                var options = await ctx.GetRequestDataAsync<SelectDeviceOptions>();

                Console.WriteLine($" ============= saving device {options.device.Name}");

                RegistryKey key = Registry.LocalMachine.OpenSubKey("Software", true);
                RegistryKey subKey = key.CreateSubKey(Consts.RegKey, true);
                subKey.SetValue(Consts.RegKeyDeviceDriver, options.device.Driver);
                subKey.SetValue(Consts.RegKeyDeviceId, options.device.ID);
                subKey.SetValue(Consts.RegKeyPaperSource, options.paperSource);
                subKey.SetValue(Consts.RegKeyBitDepth, options.bitDepth);
                subKey.SetValue(Consts.RegKeyDPI, options.dpi);
                subKey.Close();
                key.Close();

                // stop the previes device sharing server
                if (_scanServer != null) await _scanServer.Stop();

                // strat the sharing server with the new selected device
                CreateScanServer();

                await ctx.SendStringAsync("Done", "Text", System.Text.Encoding.UTF8);
            }))
            ;

        // Listen for state changes.
        server.StateChanged += (s, e) => $"WebServer New State - {e.NewState}".Info();

        return server;
    }

    private async void CreateScanServer()
    {
        Console.WriteLine(" ======== creating scan server");
        var deviceOptions = Globals.GetSelectedScanner();
        if (deviceOptions.device == null)
        {
            Console.WriteLine(" ============= no device is selected");
            return;
        }

        var scanningContext = new ScanningContext(new ImageSharpImageContext());
        scanningContext.Logger = _logger;
        var driver = deviceOptions.device.Driver.ConvertToEnum<Driver>();
        if (driver == Driver.Twain) scanningContext.SetUpWin32Worker();
        var controller = new ScanController(scanningContext);
        var devices = await controller.GetDeviceList(new ScanOptions
        {
            Driver = driver,
            PaperSource = deviceOptions.paperSource,
            BitDepth = deviceOptions.bitDepth,
            //PageSize = PageSize.A4,
            //Brightness
            //Contrast = 
            Dpi = deviceOptions.dpi,
            //MaxQuality
            //Quality            
        });
        var firstDevice = devices.Find(d => d.ID == deviceOptions.device.ID);
        if (firstDevice == null)
        {
            Console.WriteLine(" ========== selected device not found");
            return;
        }

        var scanServer = new ScanServer(scanningContext, new EsclServer
        {
            // This line is required for scanning from a browser to work
            SecurityPolicy = EsclSecurityPolicy.ServerAllowAnyOrigin,
        });
        Console.WriteLine($" ========== registered '${firstDevice.Name}' on port 9880");
        scanServer.RegisterDevice(firstDevice, port: 9880);

        // Share the device(s) until the service is stopped
        Console.WriteLine(" ========== starting the server");
        await scanServer.Start();
        Console.WriteLine(" ========== server started");

        _scanServer = scanServer;
    }
}