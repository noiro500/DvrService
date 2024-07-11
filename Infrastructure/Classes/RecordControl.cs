using System.Diagnostics;
using System.Globalization;
using DvrService.Infrastructure.Interfaces;

namespace DvrService.Infrastructure.Classes;

public class RecordControl : IRecordControl
{
    //private List<FileSystemWatcher>? WatchersList { get; set; }
    private Config? Config { get; init; }
    private List<IFFmpegRecord>? FFmpegRecordList { get; init; }
    private List<IFileWatcher>? FilesWatcherList { get; init; }
    private List<Process>? FfmpegProcess { get; init; }
    private readonly StreamWriter _errorFile;
    private readonly Timer _timer;

    public RecordControl(string configPath)
    {
        _errorFile = new StreamWriter($@"{AppDomain.CurrentDomain.BaseDirectory}\Error.txt", false);
        Config = new Config(configPath, _errorFile);
        FFmpegRecordList = new();
        FilesWatcherList = new();
        FfmpegProcess = new();
        /// <summary>
        /// Таймер проверки работоспособности программы.
        /// </summary>
        /// <remarks>
        /// Если время, прошедшее между созданием файла и текущим временем больше <c>RecordTimeMin</c>, 
        /// то вызывается метод <see cref="RecordControlStopAsync"/> и затем перезапускается <see cref="RecordControlStartAsync"/>.
        /// </remarks>
        _timer = new Timer(OnTimedEvent);
        InitializationRecordControl();
    }

    private async void OnTimedEvent(object? state)
    {
        _timer.Change(Timeout.Infinite, Timeout.Infinite);
        List<bool> flags = new();
        var format = "yyyy-MM-dd_HH-mm-ss";
        foreach (var camera in Config!.Cameras)
        {
            var files = new DirectoryInfo(camera.PathRecord).GetFiles();
            var readFileDateTime = DateTime.ParseExact(files.OrderBy(f => f.Name).TakeLast(1).ElementAt(0).Name.Substring(0, 19), format, CultureInfo.InvariantCulture);
            if (DateTime.Now - readFileDateTime > TimeSpan.FromMinutes(camera.RecordTimeMin))
                flags.Add(false);
        }
        if (flags.Any(x => x == false))
        {
            await RecordControlStopAsync();
            await Task.Delay(5000);
            await RecordControlStartAsync();
        }
        _timer.Change(TimeSpan.FromSeconds(10.0), TimeSpan.FromMinutes(Config.CheckOfRecordFilesTimeMin));

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
                    FFmpegRecordList!.Add(new FFmpegRecord(Config.FFmpegPath, cam.CameraName, cam.CameraUrl, cam.PathRecord, cam.RecordTimeMin, cam.RestartRecordAfterHours));
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
            _errorFile.WriteLine($"Error: {ex.Message}! Service not start.");
            _errorFile.Close();
            Environment.Exit(1);
        }
        _timer.Change(TimeSpan.FromSeconds(10.0), TimeSpan.FromMinutes(Config.CheckOfRecordFilesTimeMin));
    }

    public async Task RecordControlStartAsync()
    {
        try
        {
            if (FFmpegRecordList != null)
            {
                foreach (var camRec in FFmpegRecordList)
                {
                    FfmpegProcess!.Add(await camRec.StartAsync());
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
            await _errorFile.WriteLineAsync($"Error: {ex.Message}! Service not start.");
            _errorFile.Close();
            Environment.Exit(1);
        }
        catch (Exception ex)
        {
            await _errorFile.WriteLineAsync($"Error: {ex.Message}! Service not start.");
            _errorFile.Close();
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
        _errorFile.Close();
    }
}