
using HiveMind;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RawDataController : ControllerBase
    {
        // Snapshot of current game state represented as structured data.
        [Route("[action]")]
        public JsonResult Observation()
        {
            return new JsonResult(Game.ResponseObservation);
        }
            
        // Static information about gameplay elements
        [Route("[action]")]
        public JsonResult Gameplay()
        {
            return new JsonResult(Game.ResponseData);
        }

        // Static information about the map.
        [Route("[action]")]
        public JsonResult GameInfo()
        {
            return new JsonResult(Game.ResponseGameInfo);
        }
    }
}