using System.Diagnostics;

namespace DvrService.Infrastructure.Interfaces
{
    internal interface IFFmpegRecord
    {
        Task<Process> StartFfmpegRecordAsync();
        Task FFmpegRecordStopAsync();
    }
}
