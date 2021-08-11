using System.Collections;
using System.Collections.Generic;

namespace Lib
{
    public interface IStockCodeMapperProvider
    {
        IReadOnlyDictionary<string, string> Get();
    }
}