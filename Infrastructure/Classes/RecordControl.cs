using DvrService.Infrastructure.Interfaces;
using System.Diagnostics;
using System.Globalization;

namespace DvrService.Infrastructure.Classes;

public class RecordControl : IRecordControl
{
    //private List<FileSystemWatcher>? WatchersList { get; set; }
    private Config? Config { get; init; }
    private List<IFFmpegRecord>? FFmpegRecordList { get; init; }
    private List<IFileWatcher>? FilesWatcherList { get; init; }
    private List<Process>? FfmpegProcess { get; init; }
    private readonly SemaphoreSlim _semaphore;

    public RecordControl(string configPath)
    {
        Config = new Config(configPath);
        FFmpegRecordList = [];
        FilesWatcherList = [];
        FfmpegProcess = [];
        _semaphore = new SemaphoreSlim(1, 1);
        InitializationRecordControl();
        if (Config.CheckOfRecordFilesTimeMin != 0)
            JobManager.AddJob(async () => await FFmpegProcessControlAsync(), (s) => s.WithName("FFmpegProcessControl").ToRunEvery(Config.CheckOfRecordFilesTimeMin).Minutes());

        if (Config.RestartRecordAfterHours != 0)
#if DEBUG
            JobManager.AddJob(async () => await RestartRecorsAsync(), (s) => s.WithName("RestartRecordsAsync").ToRunEvery(Config.RestartRecordAfterHours).Seconds());
#else
            JobManager.AddJob(async () => await RestartRecorsAsync(), (s) => s.WithName("RestartRecordsAsync").ToRunEvery(Config.RestartRecordAfterHours).Hours());  
#endif
    }

    private async Task FFmpegProcessControlAsync()
    {
        List<bool> flags = [];
        var format = "yyyy-MM-dd_HH-mm-ss";
        var schedule = JobManager.GetSchedule("FFmpegProcessControl");
        await _semaphore.WaitAsync();
#if DEBUG
        Console.WriteLine("Вход в метод FFmpegProcessControlAsync");
#endif
        try
        {
            foreach (var camera in Config!.Cameras)
            {
                var files = new DirectoryInfo(camera.PathRecord).GetFiles();
                var readFileDateTime =
                    DateTime.ParseExact(files.OrderBy(f => f.Name).TakeLast(1).ElementAt(0).Name.Substring(0, 19),
                        format, CultureInfo.InvariantCulture);
                if (DateTime.Now - readFileDateTime > TimeSpan.FromMinutes(camera.RecordTimeMin))
                    flags.Add(false);
            }

            var processFFmpeg = Process.GetProcessesByName("ffmpeg");
            if (processFFmpeg.Length < Config.Cameras.Count)
                flags.Add(false);

            if (flags.Any(x => x == false))
            {
                await RecordControlStopAsync();
                await Task.Delay(3000);
                await RecordControlStartAsync();
                flags.Clear();
#if DEBUG
                Console.WriteLine("Перезапуск ffmpeg произведен из FFmpegProcessControlAsync");
#endif
            }
            _semaphore.Release();
        }
        catch (Exception ex)
        {
#if DEBUG
            Console.WriteLine("Сработало исключение в FFmpegProcessControlAsync");
#endif
            await Properties.WriteErrorsAsync($"Error: {ex.Message}! ошибка в методе FFmpegProcessControlAsync");
            _semaphore.Release();
        }
    }

    private void InitializationRecordControl()
    {
        try
        {
            if (Config != null && Config.Cameras.Select(x => x.PathRecord).Distinct().Count() < Config.Cameras.Count)
                throw new Exception("PathRecord must not match");
            if (Config?.Cameras != null)
            {
                foreach (var cam in Config.Cameras)
                {
                    FFmpegRecordList!.Add(new FFmpegRecord(Config.FFmpegPath, cam.CameraName, cam.CameraUrl, cam.PathRecord, cam.EncodeRecord, cam.EncodeQuality, cam.RecordTimeMin, Config.RestartRecordAfterHours));
                    FilesWatcherList!.Add(new FileWatcher(cam.PathRecord, cam.NumberFilesInFolder, cam.RemoveOldFilesAfterMin));
                }
            }
            else
            {
                throw new Exception("Configuration file is empty");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Ошибка чтения файла конфигурации");
            Properties.WriteErrors($"Error: {ex.Message}! Service not start.");
            Environment.Exit(1);
        }
    }

    public async Task RecordControlStartAsync()
    {
        try
        {
            if (FFmpegRecordList != null)
            {
                foreach (var camRec in FFmpegRecordList)
                {
                    FfmpegProcess!.Add(await camRec.StartFfmpegRecordAsync());
                }
#if DEBUG
                FfmpegProcess?.ForEach(s => Debug.WriteLine(s.Id));
#endif
            }
            else
            {
                Debug.WriteLine("Список камер пуст. Запуск невозможен");
                throw new Exception("List of cameras is empty");
            }

            if (FilesWatcherList != null)
            {
                FilesWatcherList.ForEach(s => s.FileWatcherStartAsync());
            }
            else
            {
                Debug.WriteLine("Список камер пуст. Запуск невозможен");
                throw new Exception("List of cameras is empty");
            }
        }
        catch (ArgumentOutOfRangeException ex)
        {
            await Properties.WriteErrorsAsync($"Error: {ex.Message}! Service not start.");
            Environment.Exit(1);
        }
        catch (Exception ex)
        {
            await Properties.WriteErrorsAsync($"Error: {ex.Message}! Service not start.");
            Environment.Exit(1);
        }
    }

    public async Task RecordControlStopAsync()
    {

        if (FFmpegRecordList != null)
        {
            foreach (var process in FFmpegRecordList)
                await process.FFmpegRecordStopAsync();
        }

        if (FilesWatcherList != null)
        {
            foreach (var fileWatcher in FilesWatcherList)
                await fileWatcher.FileWatcherStopAsync();
        }
        Properties.errorFiles.Close();
    }

    private async Task RestartRecorsAsync()
    {
#if DEBUG
        Console.WriteLine("Перезапуск процессов ffmpeg.exe");
#endif
        await _semaphore.WaitAsync();
        try
        {
            await RecordControlStopAsync();
            await Task.Delay(2000);
            await RecordControlStartAsync();
            _semaphore.Release();
        }
        catch (Exception e)
        {
            await Properties.WriteErrorsAsync($"Error: {e.Message}!");
            _semaphore.Release();
        }

    }
}