using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CoreHtmlToImage;
using Lib;

namespace WebCrawler
{
    public class StxChartScreenShot : IStxChartScreenShot
    {
        private const string 櫃買指數網址= "https://s.yimg.com/nb/tw_stock_frontend/scripts/TseChart/TseChart.eb1b267900.html?sid=OTC";
        private const string 加權指數網址= "https://s.yimg.com/nb/tw_stock_frontend/scripts/TseChart/TseChart.eb1b267900.html?sid=TSE";
        public async Task<byte[]> GetChartImageAsync(string code)
        {
            try
            {
                var bytes = await Task.Run(() =>
                {
                    var converter = new HtmlConverter();
                    var s = converter.FromUrl(
                        $"https://s.yimg.com/nb/tw_stock_frontend/scripts/StxChart/StxChart.9d11dfe155.html?sid={code}",width:512);
                    return s;
                });
                return bytes;
            }
            catch (Exception e)
            {
                return await Task.FromException<byte[]>(e);
            }
        }
    }
}