using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Telegram.Bot.Types;

namespace Webhook.Middleware
{
    public class JsonContentTypeFilterMiddleware
    {
        private readonly ILogger<JsonContentTypeFilterMiddleware> _logger;
        private readonly RequestDelegate _next;

        public JsonContentTypeFilterMiddleware(ILogger<JsonContentTypeFilterMiddleware> logger, RequestDelegate next)
        {
            _logger = logger;
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {

            if (!context.Request.HasJsonContentType())
            {
                _logger.LogInformation($"This {context.Request} isn't JsonContentType");
                context.Response.StatusCode = 403;
                return; 
            }
            await _next.Invoke(context);
        }
    }
}