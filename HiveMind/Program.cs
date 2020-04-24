using System;
using System.Threading.Tasks;
using SC2APIProtocol;

namespace HiveMind
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            var webSocketWrapper = new WebSocketWrapper();
            await webSocketWrapper.ConnectWebSocket();
            var connectionService = new ConnectionService(webSocketWrapper);

            var gameStarter = new GameStarter(connectionService);
            await gameStarter.CreateGame();
            await gameStarter.JoinGame(Race.Terran);

            IConstantManager constantManager = new ConstantManager(Race.Terran);
            var game = new Game(webSocketWrapper, connectionService, new WorkerManager(connectionService, constantManager),
                new BuildQueue(new BuildingManager(connectionService, constantManager)));
            await game.Run();
            Console.ReadLine();
        }
    }
}
