using System;
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
            services.AddSingleton<ILongPollingExecutor, LongPollingExecutor>();
            services.AddSingleton<IQueue<Update>, UpdateQueue>();
            services.AddScoped<IUpdateHandler, TUpdateHandler>();

        }
        public static  void Run(this IHost host)
        {
            var executor = host.Services.GetService<ILongPollingExecutor>();
            executor.StartUp();
        }
        public static Task RunAsync(this IHost host)
        {
            var executor = host.Services.GetService<ILongPollingExecutor>();
            executor.StartUp();
            return Task.CompletedTask;
        }
        public static void AddTelegramWebhook<TUpdateHandler>(this IServiceCollection services)
            where TUpdateHandler : class, IUpdateHandler
        {
            services.AddScoped<IUpdateHandler, TUpdateHandler>();
        }

        public static IApplicationBuilder UseTelegramApiWebhookEndPoint(this IApplicationBuilder app)
        {
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
    }
}