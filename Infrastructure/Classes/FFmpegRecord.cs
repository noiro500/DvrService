using System;
using System.Diagnostics;
using DvrService.Infrastructure.Interfaces;

namespace DvrService.Infrastructure.Classes;

public class FFmpegRecord : IFFmpegRecord
{
    //private readonly Timer _timer;
    private readonly string PathFFmpeg;
    private readonly Camera _camera;
    private Process? FFmpegProcess { get; set; }

    public FFmpegRecord(string pathFFmpeg, string? cameraName, string cameraUrl, string pathRecord, int recordTimeMin, int restartRecordAfterHours)
    {
        PathFFmpeg = pathFFmpeg;
        Debug.WriteLine(cameraName);
        _camera = new Camera
        {
            CameraName = cameraName!,
            CameraUrl = cameraUrl,
            PathRecord = pathRecord,
            RecordTimeMin = int.Abs(recordTimeMin)
        };
        JobManager.AddJob(async () => await RestartFFmpegAsync(), (s) => s.ToRunEvery(restartRecordAfterHours).Hours());
    }

    public async Task<Process> StartFfmpegRecordAsync()
    {
        await Task.Run(() =>
            {
                try
                {
                    using Process process = new();
                    ProcessStartInfo startInfo = new()
                    {
                        FileName = PathFFmpeg + "ffmpeg.exe",
                        Arguments =
                            $" -hide_banner -y -loglevel error -rtsp_transport tcp -use_wallclock_as_timestamps 1 -i \"{_camera.CameraUrl}\" -vcodec copy  -f segment -reset_timestamps 1 -segment_time {_camera.RecordTimeMin * 60} -segment_format mkv -segment_atclocktime 1 -strftime 1  \"{_camera.PathRecord}\\%Y-%m-%d_%H-%M-%S.mkv\"",
                        UseShellExecute = false,
                        RedirectStandardInput = true
                    };
                    FFmpegProcess = Process.Start(startInfo);
                    Properties.FFmpegProcessBag.Add(FFmpegProcess!);
                    Debug.WriteLine(FFmpegProcess!.Id + " " + FFmpegProcess.ProcessName + $"Task ID= {Task.CurrentId}");
                }
                finally
                {
                    Debug.WriteLine("Не удалось запустить процесс");
                }
            });
        return FFmpegProcess!;
    }

    public async Task FFmpegRecordStopAsync()
    {
        Debug.WriteLine($"Завершение процесса {FFmpegProcess!.Id}");
        Process killProcess;
        FFmpegProcess.Kill();
        //SchedulesProperties.FFmpegProcessBag.TryTake(out killProcess);
        await FFmpegProcess.WaitForExitAsync();
        Debug.WriteLine($"FFmpegRecord остановлен");
    }

    private async Task RestartFFmpegAsync()
    {
        Console.WriteLine("Перезапуск процессов ffmpeg.exe");
        if (FFmpegProcess is null)
            return;
        await FFmpegRecordStopAsync();
        await Task.Delay(TimeSpan.FromSeconds(2));
        await StartFfmpegRecordAsync();

    }
}