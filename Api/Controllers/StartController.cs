
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
        private readonly Game _game;
        private readonly GameStarter _gameStarter;

        public StartController(IWebSocketWrapper webSocketWrapper, GameStarter gameStarter, Game game)
        {
            _webSocketWrapper = webSocketWrapper;
            _game = game;
            _gameStarter = gameStarter;
        }
        public async Task<OkResult> Index()
        {
            await _webSocketWrapper.ConnectWebSocket();

            await _gameStarter.CreateGame();
            await _gameStarter.JoinGame(Race.Terran);

            _ = _game.Run();

            return Ok();
        }
    }
}