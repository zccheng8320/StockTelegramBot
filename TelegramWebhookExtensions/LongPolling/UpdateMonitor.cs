using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBotExtensions.Interfaces;

namespace TelegramBotExtensions.LongPolling
{
    public class UpdateMonitor : IBackGroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly UpdateQueue _updateQueue;
        private readonly ILogger<UpdateMonitor> _logger;
        private readonly int _getUpdatesLimit;
        private Timer _updateMonitorTimer;
        private Task _mainTask;
        private int _currentOffset;

        public UpdateMonitor(UpdateQueue updateQueue, ILogger<UpdateMonitor> logger,IServiceProvider serviceProvider)
        {
            _updateQueue = updateQueue;
            _logger = logger;
            _serviceProvider = serviceProvider;
            _getUpdatesLimit = DependencyInjection.GetUpdatesLimit;
        }
        public Task Start(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Start UpdateInfoMonitor");
            _updateMonitorTimer = new Timer(async o => { await SendRequestAndEnqueueTask(cancellationToken); }, null, 0, 10);
            return Task.CompletedTask;
        }
        private async Task SendRequestAndEnqueueTask(CancellationToken cancellationToken)
        {
            if (_mainTask is { Status: TaskStatus.Running })
                return;
            _mainTask = Task.Run(() =>
            {
                try
                {
                    var task = SendGetUpdateRequestAsync(_currentOffset, cancellationToken);
                    task.Wait(cancellationToken);
                    var updates = task.Result;
                    foreach (var update in updates)
                    {
                        _updateQueue.Enqueue(update);
                        _currentOffset = update.Id;
                    }

                    if (updates.Any())
                        _currentOffset++;
                }
                catch (Exception e)
                {
                    _logger.LogCritical(e.ToString());
                }
            }, cancellationToken);
            await _mainTask;
        }
        private async Task<Update[]> SendGetUpdateRequestAsync(int offset, CancellationToken token)
        {
            try
            {
                var telegramBotClient =
                    _serviceProvider.CreateAsyncScope().ServiceProvider.GetService<ITelegramBotClient>();
                //_logger.LogInformation($"Client is sending GetUpdates(offset:{offset}) request to Telegram Api...");
                var updates = await telegramBotClient.GetUpdatesAsync(offset,
                    _getUpdatesLimit, cancellationToken: token);
                //_logger.LogInformation($"GetUpdates(offset:{offset}) request is success!, updates count is {updates.Length}");
                return updates;
            }
            catch (Exception e)
            {
                _logger.LogCritical(e.ToString());
                return new Update[0];
            }
        }

        public void Dispose()
        {
            _updateMonitorTimer?.Dispose();
            _mainTask?.Dispose();
        }
    }
}