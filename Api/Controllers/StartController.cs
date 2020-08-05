
using HiveMind;
using Microsoft.AspNetCore.Mvc;
using SC2APIProtocol;
using System.Threading.Tasks;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StartController : ControllerBase
    {
        private readonly IWebSocketWrapper _webSocketWrapper;
        private readonly GameStarter gameStarter;
        private readonly IGame game;

        public StartController(IWebSocketWrapper webSocketWrapper, GameStarter gameStarter, IGame game)
        {
            _webSocketWrapper = webSocketWrapper;
            this.gameStarter = gameStarter;
            this.game = game;
        }
        public async Task<OkResult> Index()
        {
            await _webSocketWrapper.ConnectWebSocket();

            await gameStarter.CreateGame();
            await gameStarter.JoinGame(Race.Terran);

            _ = game.Run();

            return Ok();
        }
    }
}