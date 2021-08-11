using System.Threading;
using System.Threading.Tasks;

namespace Lib
{
    public interface IStxChartScreenShot
    {
        Task<byte[]> GetChartImageAsync(string code);
    }
}