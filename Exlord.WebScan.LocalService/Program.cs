using Exlord.WebScan.LocalService;

var builder = Globals.Builder;
builder.Services.AddWindowsService(options =>
{
    options.ServiceName = "Exlord WebScan Service";
});
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();