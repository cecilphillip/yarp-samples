using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ReverseProxy.Service;

namespace CCProxy.Controllers
{
    [Route("/yarp")]
    [ApiController]
    public class YarpController : Controller
    {
        private readonly IProxyConfigProvider _proxyConfigProvider;

        public YarpController(IProxyConfigProvider proxyConfigProvider)
        {
            _proxyConfigProvider = proxyConfigProvider;
        }
        
        [HttpGet("routes")]
        public ActionResult DumpRoutes()
        {
            var proxyConfig = _proxyConfigProvider.GetConfig();
            return Ok(proxyConfig.Routes);
        }

        [HttpGet("clusters")]
        public ActionResult DumpClusters()
        {
            var proxyConfig = _proxyConfigProvider.GetConfig();
            return Ok(proxyConfig.Clusters);
        }

        [HttpGet("incoming")]
        public IActionResult Dump()
        {
            var result = new
            {
                Request.Method,
                Request.Protocol,
                Request.Scheme,
                Host = Request.Host.Value,
                PathBase = Request.PathBase.Value,
                Path = Request.Path.Value,
                Query = Request.QueryString.Value,
                Headers = Request.Headers.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToArray()),
                Time = DateTimeOffset.UtcNow
            };

            return Ok(result);
        }
    }
}