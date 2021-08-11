using System.Collections.Generic;
using System.Linq;
using Lib.CommandProcess.Interfaces;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBotExtensions.Interfaces;

namespace Lib.CommandProcess
{
    public class CommandProcessorFactory : ICommandProcessorFactory
    {
        private readonly IEnumerable<BaseCommandProcessor> _baseCommandProcessors;

        public CommandProcessorFactory(IEnumerable<BaseCommandProcessor> baseCommandProcessors)
        {
            _baseCommandProcessors = baseCommandProcessors;
        }

        public ICommandProcessor Create(Update update)
        {
            var result = _baseCommandProcessors.FirstOrDefault(m => m.IsMatch(update));
            return result;
        }
    }
}