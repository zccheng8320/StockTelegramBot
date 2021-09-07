﻿using System;
using System.Text;
using System.Threading.Tasks;
using DataAccess;
using Lib.CommandProcess;
using Lib.CommandProcess.Interfaces;
using Lib.SeleniumExtensions;
using Lib.Stock;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenQA.Selenium.Chrome;
using Telegram.Bot;
using TelegramBotExtensions.Interfaces;
using WebCrawler;
using WebCrawler.InformationParser;

namespace Lib
{
    public static class DependencyInjection
    {
        /// <summary>
        /// use this method with <see cref="StockBotHostExtensions.RunStockTelegramBotAsync"/> necessarily
        /// </summary>
        /// <param name="services"></param>
        public static void AddStockTelegramBot(this IServiceCollection services, IConfiguration configuration = null)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            services.AddMemoryCache();
            services.AddSingleton<IStockCodeMapperProvider,TaipeiStockCodeMapperProvider>();
            services.AddSingleton<IStockCodeMapperCreator,TaipeiStockCodeMapperCreator>();
            services.AddSingleton<IHtmlDataDownload,HtmlDataDownload>();       
            //services.AddStxChartSelenium();
            services.AddSingleton<IStxChartScreenShot, StxChartScreenShot>();
            services.AddScoped<IStxInfoTextCrawler,StxInfoTextCrawler>();           
            services.AddScoped<BaseCommandProcessor, StxChartSearch>();
            services.AddScoped<BaseCommandProcessor, StxTextSearch>();
            services.AddScoped<BaseCommandProcessor, GetPortfolio>();
            services.AddScoped<BaseCommandProcessor, SetPortfolio>();
            services.AddScoped<ICommandProcessorFactory,CommandProcessorFactory>();
            services.AddDataAccess(configuration);
        }

        private static void AddStxChartSelenium(this IServiceCollection services)
        {
            services.AddSingleton<ChromeDriverConcurrentQueue>();
            services.AddSingleton<IStxChartScreenShot, StxChartSelenium>();
            
        }
    }
}