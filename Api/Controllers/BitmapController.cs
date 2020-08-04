using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Sys = System.IO;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BitmapController : ControllerBase
    {
        private readonly ILogger<BitmapController> _logger;

        public BitmapController(ILogger<BitmapController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public async Task<string> Get()
        {
            var bitmapBytes = await Sys.File.ReadAllBytesAsync("Monochrome100x100.bmp");
            return Encoding.UTF8.GetString(bitmapBytes);
        }
    }
}
