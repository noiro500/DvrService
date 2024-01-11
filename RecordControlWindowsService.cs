using System.Diagnostics;
using DvrService.Infrastructure.Classes;

namespace DvrService
{
    public sealed class RecordControlWindowsService : BackgroundService
    {
        private readonly RecordControl _recordControl = new();

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Delay(1000, stoppingToken);
            await _recordControl.RecordControlStartAsync();
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            Debug.WriteLine("Сработал метод StopAsync из класса RecordControlWindowsService");
            await _recordControl.RecordControlStopAsync();
            await base.StopAsync(cancellationToken);
        }
    }
}
