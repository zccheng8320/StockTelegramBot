using Telegram.Bot.Types;

namespace Lib.CommandProcess.Interfaces
{
    public interface ICommandProcessorFactory
    {
        public ICommandProcessor Create(Update update);
    }
}