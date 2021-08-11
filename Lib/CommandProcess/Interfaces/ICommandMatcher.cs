using Telegram.Bot.Types;

namespace Lib.CommandProcess.Interfaces
{
    public interface ICommandMatcher
    {
        bool IsMatch(Update update);
    }
}