using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Lib.CommandProcess.Interfaces;
using Telegram.Bot.Types;
using TelegramBotExtensions.Interfaces;

namespace Lib
{
    public class StxUpdateHandler : IUpdateHandler
    {
        private readonly ILogger<StxUpdateHandler> _logger;
        private readonly ICommandProcessorFactory _factory;
        
        public StxUpdateHandler(ILogger<StxUpdateHandler> logger, ICommandProcessorFactory factory)
        {
            _logger = logger;
            _factory = factory;
        }

        public async Task Process(Update update)
        {
            try
            {
                if (update is null)
                    return;

                var nowUtc = DateTime.UtcNow;

                if (nowUtc > update.Message.Date.AddMinutes(1))
                    return;

                //_logger.LogInformation($"Start Process UpdateId: {id}");

                void CallBack(Task task)
                {
                    //_logger.LogInformation($" UpdateId: {id} is {task.Status}");
                    if (task.IsFaulted) _logger.LogCritical($"Fail Reason :{task.Exception?.ToString()}");
                }

                await ProcessCore(CallBack,update);

            }
            catch (Exception e)
            {
                _logger.LogCritical(e.ToString());
                await Task.FromException(e);
            }
        }
        private async Task ProcessCore(Action<Task> callBack, Update update)
        {
            try
            {
                var processor = _factory.Create(update);
                if (processor != null)
                    await processor.Process(update);
                callBack?.Invoke(Task.CompletedTask);
            }
            catch (Exception e)
            {
                callBack?.Invoke(Task.FromException(e));
            }
        }


    }
}