using System.Diagnostics;
using DvrService.Infrastructure.Classes;

namespace DvrService
{
    public sealed class RecordControlWindowsService : BackgroundService
    {
        private readonly List<string> _args;
        private readonly IRecordControl _recordControl;
        
        public RecordControlWindowsService(List<string> args)
        {
            _args = args;
            if(_args.Any())
                _recordControl = new RecordControl(_args[0]);
            else
                _recordControl = new RecordControl(String.Empty);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Delay(1000, stoppingToken);
await _recordControl.RecordControlStartAsync();
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            Debug.WriteLine("Сработал метод StopAsync из класса RecordControlWindowsService");
            await _recordControl.RecordControlStopAsync();
            await Task.Delay(3000, cancellationToken);
            await base.StopAsync(cancellationToken);
        }
    }
}
