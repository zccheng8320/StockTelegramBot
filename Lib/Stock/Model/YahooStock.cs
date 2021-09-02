using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;

namespace Lib.Stock.Model
{
    public class YahooStock
    {
        private const string IncreaseIcon = "△ ";
        private const string DecreaseIcon = "▼";
        [JsonProperty("tick")]
        public IEnumerable<Tick> Tick { get; set; }
        [JsonProperty("mem")]
        public Mem Mem { get; set; }
        public string Code { get; set; }
        public string Icon
        {
            get
            {
                if (Mem == null)
                    return "";
                return Mem.漲跌 == 0 ? "" : Mem.漲跌 > 0 ? IncreaseIcon : DecreaseIcon;
            }
        }

        public string ToSimpleString()
        {
            var tick = Tick.LastOrDefault();
            if (tick == null || Mem == null)
                return $"{Code}:尚無成交資訊。";
            return $"{Mem.SimpleDisplay}：{tick.現價} {Icon} {Mem.漲跌} ({Mem.漲跌幅:F}%)";
        }
        public override string ToString()
        {
            var tick = Tick.LastOrDefault();
            if (tick == null || Mem == null)
                return "尚無成交資訊。";
            return $"{tick.Time} {Mem.Display} \r\n成交價：{tick.現價} {Icon} {Mem.漲跌} ({Mem.漲跌幅:F}%) 總量:{Mem.總量}";
        }
    }

    public class Tick
    {
        public string t { get; set; }

        public string Time => DateTime.ParseExact(t, "yyyyMMddHHmm", DateTimeFormatInfo.CurrentInfo)
            .ToString("yyyy/MM/dd HH:mm");
        [JsonProperty("p")]
        public string 現價 { get; set; }
        [JsonProperty("v")]
        public string 成交量 { get; set; }
    }
    public class Mem
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("name")]
        private string Name { get; set; }
        /// <summary>
        /// 顯示名稱
        /// </summary>
        public string Display => Id is "#001" or "#026" ? Id is "#001" ? "加權股價指數" : "櫃買指數" : $"({Id}){Name}";
        public string SimpleDisplay => Id is "#001" or "#026" ? Id is "#001" ? "加權股價指數" : "櫃買指數" : $"{Name}";
        [JsonProperty("404")]
        private int 總量_個股 { get; set; }
        [JsonProperty("501")]
        private int 總量_指數 { get; set; }

        public int 總量 => Id is "#001" or "#026" ? 總量_指數 : 總量_個股;
        [JsonProperty("184")]
        public decimal 漲跌 { get; set; }
        [JsonProperty("185")]
        public decimal 漲跌幅 { get; set; }

        public StockType StockType => Id is "#001" or "#026" ? StockType.指數 : StockType.個股;
    }
}