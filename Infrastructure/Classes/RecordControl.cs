using System.Diagnostics;
using DvrService.Infrastructure.Interfaces;

namespace DvrService.Infrastructure.Classes;

public class RecordControl : IRecordControl
{
    private List<FileSystemWatcher>? WatchersList { get; set; }
    private Config? Config { get; init; }
    private List<IFFmpegRecord>? FFmpegRecordList { get; init; }
    private List<IFileWatcher>? FilesWatcherList { get; init; }
    private List<Process>? FfmpegProcess { get; init; }
    private readonly StreamWriter _errorFile;

    public RecordControl(string configPath)
    {
        _errorFile = new StreamWriter($@"{AppDomain.CurrentDomain.BaseDirectory}\Error.txt", false);
        Config = new Config(configPath, _errorFile);
        FFmpegRecordList = new List<IFFmpegRecord>();
        FilesWatcherList = new List<IFileWatcher>();
        FfmpegProcess = new List<Process>();
        InitializationRecordControl();
    }

    private void InitializationRecordControl()
    {
        try
        {
            var s = Config.Cameras.Select(x => x.PathRecord).Distinct().Count();
            if (Config.Cameras.Select(x => x.PathRecord).Distinct().Count() < Config.Cameras.Count)
                throw new Exception("PathRecord must not match");
            if (Config.Cameras != null)
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