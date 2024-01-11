using System.Diagnostics;
using DvrService.Infrastructure.Interfaces;

namespace DvrService.Infrastructure.Classes;

public class RecordControl
{
    private List<FileSystemWatcher>? WatchersList { get; set; }
    private Config? Config { get; init; }
    private List<IFFmpegRecord>? FFmpegRecordList { get; init; }
    private List<IFileWatcher>? FilesWatcherList { get; init; }
    private List<Process>? FfmpegProcess { get; init; }

    public RecordControl()
    {
        Config = new Config();
        FFmpegRecordList = new List<IFFmpegRecord>();
        FilesWatcherList = new List<IFileWatcher>();
        FfmpegProcess = new List<Process>();
        InitializationRecordControl();
    }

    private void InitializationRecordControl()
    {
        try
        {
            if (Config.Cameras != null)
            {
                foreach (var cam in Config.Cameras)
                {
                    //WatchersList!.Add(new FileSystemWatcher(cam.PathRecord)
                    //{
                    //    NotifyFilter = NotifyFilters.FileName,
                    //});
                    FFmpegRecordList!.Add(new FFmpegRecord(Config.FFmpegPath, cam.CameraName, cam.CameraUrl, cam.PathRecord, cam.RecordTimeMin, cam.RestartRecordAfterHours));
                    FilesWatcherList!.Add(new FileWatcher(cam.PathRecord, cam.NumberFilesInFolder, cam.RemoveOldFilesAfterMin));
                }
                //WatchersList!.ForEach(s => s.Created += OnChanged);
            }
            else
            {
                throw new Exception("Файл конфигурации пуст");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Ошибка чтения файла конфигурации");
            EventLog.WriteEntry("Ошибка чтения файла конфигурации", ex.Message,
                EventLogEntryType.Error);
            WatchersList = null;
        }
    }

    public async Task RecordControlStartAsync()
    {
        //if (WatchersList != null)
        //    WatchersList!.ForEach(s => s.EnableRaisingEvents = true);
        //await Task.Run(() => _timer.Change(TimeSpan.FromMinutes(_recordTimeMin + 0.1), TimeSpan.FromMinutes(_recordTimeMin)));
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
        }
        if (FilesWatcherList != null)
        {
            FilesWatcherList.ForEach(s=>s.FileWatcherStartAsync());
        }
        else
        {
            Debug.WriteLine("Список камер пуст. Запуск невозможен");
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
    }

    //public /*async*/ void /*Task*/ StopAsync()
    //{
    //    WatchersList.ForEach(s => s.EnableRaisingEvents = false);
    //    _timer.Change(Timeout.Infinite, Timeout.Infinite);
    //    _timer.Dispose();
    //    WatchersList.ForEach(s => s.Dispose());
    //}

    //private void OnChanged(object sender, FileSystemEventArgs e)
    //{
    //    Debug.WriteLine($"File: {e.FullPath}");
    //    _timer.Change(Timeout.Infinite, Timeout.Infinite);
    //    _timer.Change(TimeSpan.FromMinutes(_recordTimeMin + 0.1), TimeSpan.FromMinutes(_recordTimeMin));
    //}

    //private void OnTimedEvent(object? state)
    //{
    //    WatchersList.ForEach(s => s.EnableRaisingEvents = false);
    //    _timer.Change(Timeout.Infinite, Timeout.Infinite);
    //    Debug.WriteLine("Сработал OnTimedEvent в DirectoryWatcher классе");
    //}
}