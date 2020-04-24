using System;
using System.Threading.Tasks;
using SC2APIProtocol;

namespace HiveMind
{
    public class Program
    {
        public Program(IConstantManager constantManager)
        {
            _constantManager = constantManager;
        }

        private static WebSocketWrapper _webSocketWrapper;
        private static IConstantManager _constantManager;

        static async Task Main(string[] args)
        {
            _webSocketWrapper = new WebSocketWrapper();
            await _webSocketWrapper.ConnectWebSocket();
            var connectionService = new ConnectionService(_webSocketWrapper);

            var gameStarter = new GameStarter(connectionService);
            await gameStarter.CreateGame();
            await gameStarter.JoinGame(Race.Terran);

            _constantManager = new ConstantManager(Race.Terran);
            var game = new Game(_webSocketWrapper, connectionService, new WorkerManager(connectionService, _constantManager));
            await game.Run();
            Console.ReadLine();
        }
    }
}
