namespace DvrService.Infrastructure.Interfaces;

internal interface IFileWatcher
{
    Task FileWatcherStartAsync();
    Task FileWatcherStopAsync();
}