using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LBProxy.Controllers
{
    [Route("/status")]
    [ApiController]
    public class StatusController : ControllerBase
    {
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly ILogger<StatusController> _logger;

        public StatusController(IWebHostEnvironment hostEnvironment, ILogger<StatusController> logger)
        {
            _logger = logger;
            _hostEnvironment = hostEnvironment;
        }

        [HttpGet]
        public ActionResult GetStatus()
        {           
            return Ok(new
            {
                Environtment = _hostEnvironment.EnvironmentName,
                Platform = RuntimeInformation.FrameworkDescription
            });
        }
    }
}