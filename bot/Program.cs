using System;
using System.Threading.Tasks;
using SC2APIProtocol;

namespace bot
{
    public class Program
    {
        private static WebSocketWrapper _webSocketWrapper;

        static async Task Main(string[] args)
        {
            _webSocketWrapper = new WebSocketWrapper();
            await _webSocketWrapper.ConnectWebSocket();
            var connectionService = new ConnectionService(_webSocketWrapper);

            var gameStarter = new GameStarter(connectionService);
            await gameStarter.CreateGame();
            await gameStarter.JoinGame(Race.Terran);

            var game = new Game(_webSocketWrapper, connectionService);
            await game.Run();
            Console.ReadLine();
        }
    }
}
