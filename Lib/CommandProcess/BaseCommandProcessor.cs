using System.Threading.Tasks;
using Lib.CommandProcess.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBotExtensions.Interfaces;

namespace Lib.CommandProcess
{
    public abstract class BaseCommandProcessor : ICommandMatcher,ICommandProcessor
    {
        protected readonly ITelegramBotClient _client;

        protected BaseCommandProcessor(ITelegramBotClient client)
        {
            _client = client;
        }
        public abstract bool IsMatch(Update update);
        public abstract Task Process(Update update);
    }
}