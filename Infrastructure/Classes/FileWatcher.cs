using System.Diagnostics;
using DvrService.Infrastructure.Interfaces;

namespace DvrService.Infrastructure.Classes;


public class FileWatcher : IFileWatcher
{
    private readonly Camera _camera;
    private FileInfo[]? Files { get; set; }

    public FileWatcher(string pathRecord, int numberFilesInFolder, int removeOldFilesAfterMin)
    {
        Debug.WriteLine("Конструктор удаления файлов");
        _camera = new Camera
        {
            PathRecord = pathRecord,
            NumberFilesInFolder =int.Abs(numberFilesInFolder),
            RemoveOldFilesAfterMin = int.Abs(removeOldFilesAfterMin)
        };
        JobManager.AddJob(DeleteOldFiles, (s) => s.WithName("FileWatcherControl").ToRunNow().AndEvery(_camera.RemoveOldFilesAfterMin).Minutes());

    }

    public Task FileWatcherStartAsync()
    {
        return Task.Run(() =>
        {
            Debug.WriteLine($"FileWatcher TaskID={Task.CurrentId}");
        });
    }

    public Task FileWatcherStopAsync()
    {
        JobManager.RemoveJob("FileWatcherControl");
        Debug.WriteLine($"FileWatcher остановлен");
return Task.CompletedTask;
    }

    private void DeleteOldFiles()
    {
        Console.WriteLine("Сработал таймер, вызван DeleteOldFiles");
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