using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DemoProxy.Controllers
{
    [Route("/status")]
    [ApiController]
    public class StatusController : ControllerBase
    {
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly IConfiguration _configuration;
        private readonly ILogger<StatusController> _logger;

        public StatusController(IWebHostEnvironment hostEnvironment, IConfiguration configuration, ILogger<StatusController> logger)
        {
            _logger = logger;
            _configuration = configuration;
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