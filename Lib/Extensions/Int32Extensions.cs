using System;

namespace Lib.Extensions
{
    public static class Int32Extensions
    {
        public static string FormatNumber(this int num)
        {

            return Math.Abs(num) switch
            {
                >= 100000000 => FormatNumber(num / 1000000) + "M",
                >= 100000 => FormatNumber(num / 1000) + "K",
                >= 10000 => (num / 1000D).ToString("0.#") + "K",
                _ => num.ToString("#,0")
            };
        }
    }
}