using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Lib.Stock
{

    public class TaipeiStockCodeMapperProvider : IStockCodeMapperProvider
    {
        private readonly ILogger<TaipeiStockCodeMapperProvider> _logger;
        private readonly IMemoryCache _memoryCache;
        private MapperCacheKey _cacheKey;
        private readonly object _cacheKeyLockObject = new();

        public TaipeiStockCodeMapperProvider(ILogger<TaipeiStockCodeMapperProvider> logger, IMemoryCache memoryCache)
        {
            _cacheKey = MapperCacheKey.Create();
            _logger = logger;
            _memoryCache = memoryCache;
        }
        public IReadOnlyDictionary<string, string> Get()
        {

            if (!IsCacheExists())
            {
                CreateCache();
                return _memoryCache.Get<Dictionary<string, string>>(_cacheKey.Key);
            }

            var result = _memoryCache.Get<Dictionary<string, string>>(_cacheKey.Key);
            if (!IsCacheExpired())
                return result;
            if (!UpdateCacheData())
                return result;
            return _memoryCache.Get<Dictionary<string, string>>(_cacheKey.Key);

        }

        private bool UpdateCacheData()
        {
            try
            {
                lock (_cacheKeyLockObject)
                {
                    _cacheKey.UpdateTricks();
                }
                CreateCache();
                return true;
            }
            catch (Exception e)
            {
                _logger.LogCritical($"Update failed because:" + e);
                return false;
            }
        }

        private void CreateCache()
        {
            if (File.Exists(Const.StockCodeMappingInfoPath))
            {
                var dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(
                                     File.ReadAllText(Const.StockCodeMappingInfoPath));
                _memoryCache.Set(_cacheKey.Key, dictionary);
                return;
            }
            throw new Exception($"Stock mapper isn't exists , path : {Const.StockCodeMappingInfoPath}");
        }

        private bool IsCacheExpired()
        {
            return this._cacheKey.IsExpired;
        }

        private bool IsCacheExists()
        {
            return _memoryCache.TryGetValue(_cacheKey.Key, out var _);
        }
        private struct MapperCacheKey
        {
            private long _currentTricks;

            public bool IsExpired => DateTime.Today.Ticks != _currentTricks;
            public string Key => "StockCodeMapper";

            public static MapperCacheKey Create()
            {
                var r = new MapperCacheKey();
                r.UpdateTricks();
                return r;
            }
            public void UpdateTricks()
            {
                _currentTricks = DateTime.Today.Ticks;
            }
        }
    }
}