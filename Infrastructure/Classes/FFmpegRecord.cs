using System;
using System.Diagnostics;
using DvrService.Infrastructure.Interfaces;

namespace DvrService.Infrastructure.Classes;

public class FFmpegRecord : IFFmpegRecord
{
    private readonly Timer _timer;
    private readonly string PathFFmpeg;
    private readonly Camera _camera;
    private Process? FFmpegProcess { get; set; }
    private double RestartRecordAfterHours { get; set; }

    public FFmpegRecord(string pathFFmpeg, string? cameraName, string cameraUrl, string pathRecord, double recordTimeMin, double restartRecordAfterHours)
    {
        PathFFmpeg = pathFFmpeg;
        Debug.WriteLine(cameraName);
        _camera = new Camera
        {
            CameraName = cameraName!,
            CameraUrl = cameraUrl,
            PathRecord = pathRecord,
            RecordTimeMin = double.Abs(recordTimeMin)
        };
        RestartRecordAfterHours = double.Abs(restartRecordAfterHours);
        _timer = new Timer(OnTimedEvent);
    }

    private async void OnTimedEvent(object? sender)
    {
        Debug.WriteLine("Перезапуск процессов ffmpeg.exe");
        _timer.Change(Timeout.Infinite, Timeout.Infinite);
        if(FFmpegProcess is null)
            return;
        await FFmpegRecordStopAsync();
        await Task.Delay(TimeSpan.FromSeconds(2));
        await StartAsync();
    }

    public async Task<Process> StartAsync()
    {
        try
        {
            _timer.Change(TimeSpan.FromHours(RestartRecordAfterHours), TimeSpan.FromHours(RestartRecordAfterHours));
        }
        catch
        {
            throw new ArgumentOutOfRangeException("The \"RestartRecordAfterHours\" parameter value must be greater than zero");
        }
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
        FFmpegProcess.Kill();
        await FFmpegProcess.WaitForExitAsync();
        Debug.WriteLine($"FFmpegRecord остановлен");
    }
}