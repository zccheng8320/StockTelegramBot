using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Lib.CommandProcess.Interfaces
{
    public interface ICommandProcessor
    {
        Task Process(Update update);
    }
}