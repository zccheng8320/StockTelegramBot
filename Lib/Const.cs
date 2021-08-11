using System;
using System.IO;

namespace Lib
{
    public static class Const
    {
        /// <summary>
        /// 股票代號對應資訊Path
        /// </summary>
        public static string StockCodeMappingInfoPath => Path.Combine("","stockCodeMappingInfo.json");
        public const string 指數網址 = "https://s.yimg.com/nb/tw_stock_frontend/scripts/TseChart/TseChart.eb1b267900.html?sid={0}";
        public const string 個股網址 = "https://s.yimg.com/nb/tw_stock_frontend/scripts/StxChart/StxChart.9d11dfe155.html?sid={0}";
        public const string 證交所網址 = "https://mis.twse.com.tw/stock/fibest.jsp?stock={0}";
        public const string 櫃買指數代號 = "OTC";
        public const string 加權指數代號 = "TSE";
    }
}