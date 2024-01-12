namespace DvrService.Infrastructure.Classes;

public interface IRecordControl
{
    Task RecordControlStartAsync();
    Task RecordControlStopAsync();
}