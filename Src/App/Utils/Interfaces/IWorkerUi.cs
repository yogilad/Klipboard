using Klipboard.Workers;

namespace Klipboard.Utils.Interfaces;

public interface IWorkerUi
{
    Task<object> ShowDialog(object arg);
}
