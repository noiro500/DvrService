using DvrService;
using DvrService.Infrastructure.Classes;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Logging.EventLog;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Services.AddWindowsService(options =>
{
    options.ServiceName = "FFmpeg dvr and control Service";

});

    builder.Services.AddHostedService(sp => new RecordControlWindowsService( args.ToList()));

IHost host = builder.Build();
host.Run();
