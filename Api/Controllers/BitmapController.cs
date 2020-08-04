using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
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
        public async Task<FileContentResult> Get()
        {
            return File(await Sys.File.ReadAllBytesAsync("Monochrome100x100.bmp"), "image/bitmap");
        }
    }
}
