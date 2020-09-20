using System;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.ReverseProxy.Middleware;

namespace CCProxy.Middleware
{
    public class HttpInspectorMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;

        public HttpInspectorMiddleware(RequestDelegate next,
            ILogger<HttpInspectorMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            LogRequest(context);
            LogDestinations(context);
            await _next(context);
        }

        private void LogDestinations(HttpContext context)
        {
            _logger.LogDebug("Available Destinations: ");
            var proxyFeature = context.Features.Get<IReverseProxyFeature>();

            proxyFeature.AvailableDestinations.ForEach(dest =>
            {
                _logger.LogDebug("Destination {ID} {Address}", dest.DestinationId, dest.Config.Address);
            });
        }

        private void LogRequest(HttpContext context)
        {
            var buffer = new StringBuilder();

            context.Request.Headers
                .ForEach(kvp => buffer.Append($"{kvp.Key}: {kvp.Value}{Environment.NewLine}"));

            _logger.LogDebug($"Http Request Information:{Environment.NewLine}" +
                             $"Schema: {context.Request.Scheme}{Environment.NewLine}" +
                             $"Host: {context.Request.Host}{Environment.NewLine}" +
                             $"Path: {context.Request.Path}{Environment.NewLine}" +
                             $"QueryString: {context.Request.QueryString}{Environment.NewLine}" +
                             $"Headers: {Environment.NewLine}{buffer}{Environment.NewLine}");
        }
    }
}