using System.Collections.Generic;
using Newtonsoft.Json;

namespace Lib.Stock.Model
{
    public class StockRank
    {
        [JsonProperty("list")]
        public IEnumerable<StockInfo> List { get; set; }
    }

    public class StockInfo
    {
        [JsonProperty("rowId")]
        public string ROWID { get; set; }

        public string Code => ROWID.Replace(".TW", "");
        /// <summary>
        /// 漲跌幅
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }
        /// <summary>
        /// 排行榜
        /// </summary>
        [JsonProperty("rank")]
        public int Rank { get; set; } 
        /// <summary>
        /// 漲跌幅
        /// </summary>
        [JsonProperty("changePercent")]
        public string ChangePerson { get; set; }
        /// <summary>
        /// 成交價
        /// </summary>
        [JsonProperty("price")]
        public string Price { get; set; }

        private const string OutPutTemplate = "{0}.({1}){2} 漲幅:{3} 成交價:{4}";
        public override string ToString()
        {
            return string.Format(OutPutTemplate, $"{Rank}", Code, Name, ChangePerson, Price);
        }
    }
}