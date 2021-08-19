using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBotExtensions.Interfaces;
using TelegramBotExtensions.LongPolling;
using TelegramWebhookExtensions.Middleware;

namespace TelegramBotExtensions
{
    public static class DependencyInjection
    {
        public static void AddTelegramBotClient(this IServiceCollection services)
        {
            services.AddTransient<ITelegramBotClient, TelegramBotClient>(m =>
            {
                var configuration = m.GetService<IConfiguration>();
                var telegramApiToken = configuration["TelegramSetting:TelegramApiToken"];
                return new TelegramBotClient(telegramApiToken);
            });
        }
        public static void AddTelegramBotClient(this IServiceCollection services,string telegramApiToken)
        {
            services.AddTransient<ITelegramBotClient, TelegramBotClient>(m =>
            {
                var configuration = m.GetService<IConfiguration>();
                return new TelegramBotClient(telegramApiToken);
            });
        }
        public static void AddLongPolling<TUpdateHandler>(this IServiceCollection services) 
            where TUpdateHandler : class,IUpdateHandler 
        {
            services.AddHostedService<LongPollingHostService>();
            services.AddSingleton<IQueue<Update>, UpdateQueue>();
            services.AddScoped<IUpdateHandler, TUpdateHandler>();
        }
        
        public static void AddTelegramWebhook<TUpdateHandler>(this IServiceCollection services)
            where TUpdateHandler : class, IUpdateHandler
        {
            services.AddScoped<IUpdateHandler, TUpdateHandler>();
        }

        private static bool isSetWebhookInfo = false;
        public static IApplicationBuilder UseTelegramApiWebhookEndPoint(this IApplicationBuilder app)
        {
            app.Use(async (context, next) =>
            {
                if(isSetWebhookInfo)
                    await next.Invoke();
                await SetWebhookInfo(context.RequestServices);
                await next.Invoke();
            });

            app.UseTelegramWebhookMiddleware();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapPost("/", async context =>
                {
                    context.Response.StatusCode = 200;
                    await context.Response.WriteAsync("ok");
                    var updateHandler = context.RequestServices.GetService<IUpdateHandler>();
                    if (context.Features[typeof(Update)] is Update update)
                        await updateHandler.Process(update);
                });
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Hi");
                });
            });
            return app;
        }
        static async Task SetWebhookInfo(IServiceProvider serviceProvider)
        {
            var configure = serviceProvider.CreateScope().ServiceProvider.GetService<IConfiguration>();
            var telegramBot = serviceProvider.CreateScope().ServiceProvider.GetService<ITelegramBotClient>();
            await telegramBot.TestApiAsync();
            var telegramWebhookUrl = configure["TelegramSetting:TelegramApiWebhookUrl"];
            await telegramBot.SetWebhookAsync(telegramWebhookUrl);
        }
    }
}