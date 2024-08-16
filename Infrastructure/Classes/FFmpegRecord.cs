﻿using DvrService.Infrastructure.Interfaces;
using System.Diagnostics;

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
                            $" -hide_banner -y -loglevel fatal -rtsp_transport tcp -use_wallclock_as_timestamps 1 -i \"{_camera.CameraUrl}\" -c copy  -f segment -reset_timestamps 1 -segment_time {_camera.RecordTimeMin * 60} -segment_format mkv -segment_atclocktime 1 -strftime 1  \"{_camera.PathRecord}\\%Y-%m-%d_%H-%M-%S.mkv\"",
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
#if DEBUG
        Console.WriteLine($"Завершение процесса {FFmpegProcess!.Id}");
#endif
        if (FFmpegProcess != null && !FFmpegProcess.HasExited)
        {
            using (var streamWriter = FFmpegProcess.StandardInput)
            {
                if (streamWriter.BaseStream.CanWrite)
                    streamWriter.WriteLine("q");
            }
        }
        await FFmpegProcess.WaitForExitAsync();
#if DEBUG
        Console.WriteLine($"FFmpegRecord остановлен");
#endif
    }
}