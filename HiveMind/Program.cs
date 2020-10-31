using System;
using System.Threading.Tasks;
using SC2APIProtocol;

namespace HiveMind
{
    public static class Program
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
            var workerManager = new UnitBuilder(connectionService, constantManager, new GameDataService());
            var game = new Game(connectionService, workerManager,
                new BuildQueue(new BuildingManager(connectionService, constantManager, new GameDataService()), 
                workerManager, new ArmyManager(connectionService, constantManager, new GameDataService())));
            await game.Run();
            Console.ReadLine();
        }
    }
}
