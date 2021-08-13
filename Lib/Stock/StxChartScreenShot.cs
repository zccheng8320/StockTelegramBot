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
        public async Task<byte[]> GetChartImageAsync(string code)
        {
            try
            {
                var bytes = await Task.Run(() =>
                {
                    var converter = new HtmlConverter();
                    var s = converter.FromUrl(
                        code is Const.櫃買指數代號 or Const.加權指數代號 ? string.Format(Const.指數網址, code) : string.Format(Const.個股網址, code), width:512);
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