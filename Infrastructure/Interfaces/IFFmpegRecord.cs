using System.Diagnostics;

namespace DvrService.Infrastructure.Interfaces
{
    internal interface IFFmpegRecord
    {
        Task<Process> StartAsync();
        Task FFmpegRecordStopAsync();
    }
}
