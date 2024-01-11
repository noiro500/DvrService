using System.Diagnostics;
using DvrService.Infrastructure.Interfaces;

namespace DvrService.Infrastructure.Classes;


public class FileWatcher : IFileWatcher
{

    private readonly Timer _timer;

    private readonly Camera _camera;
    private FileInfo[]? Files { get; set; }

    public FileWatcher(string pathRecord, int numberFilesInFolder, double removeOldFilesAfterMin)
    {
        Debug.WriteLine("Конструктор удаления файлов");
        _camera = new Camera
        {
            PathRecord = pathRecord,
            NumberFilesInFolder = numberFilesInFolder,
            RemoveOldFilesAfterMin = removeOldFilesAfterMin
        };
        _timer = new Timer(OnTimedEvent);
    }

    public Task FileWatcherStartAsync()
    {
        return Task.Run(() =>
        {
            Debug.WriteLine($"FileWatcher TaskID={Task.CurrentId}");
            _timer.Change(TimeSpan.FromSeconds(10.0), TimeSpan.FromMinutes(_camera.RemoveOldFilesAfterMin));
        });
    }

    public async Task FileWatcherStopAsync()
    {
        _timer.Change(Timeout.Infinite, Timeout.Infinite);
        await _timer.DisposeAsync();
        Debug.WriteLine($"FileWatcher остановлен");

    }

    private void OnTimedEvent(object? sender)
    {
        Debug.WriteLine("Сработал таймер, вызван OnTimedEvent");
        Files = new DirectoryInfo(_camera.PathRecord).GetFiles();
        if (Files.Length >= _camera.NumberFilesInFolder)
        {
            Debug.WriteLine("Запуск удаления файлов");
            foreach (var file in Files.OrderBy(f => f.CreationTime).Take(Files.Length - _camera.NumberFilesInFolder))
            {
                file.Delete();
            }
            Debug.WriteLine("Файлы удалены");
        }
    }

}