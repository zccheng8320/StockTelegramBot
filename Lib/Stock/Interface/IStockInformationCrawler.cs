using System.Threading;
using System.Threading.Tasks;

namespace Lib
{
    public interface IStockCodeMapperCreator
    {
        Task CreateAsync(CancellationToken token = default);
    }

}