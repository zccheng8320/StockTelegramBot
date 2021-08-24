using System;
using System.Threading;
using System.Threading.Tasks;

namespace TelegramBotExtensions.Interfaces
{
    public interface IBackGroundService : IDisposable
    {
        Task Start(CancellationToken cancellationToken = default);
    }
}