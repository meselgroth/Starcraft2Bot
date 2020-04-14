using System;
using System.IO;
using System.Threading.Tasks;
using SC2APIProtocol;

namespace bot
{
    public class GameStarter
    {
        private readonly IWebSocketWrapper webSocketWrapper;

        public GameStarter(IWebSocketWrapper webSocketWrapper)
        {
            this.webSocketWrapper = webSocketWrapper;
        }

        public async Task CreateGame()
        {
            var createGame = new RequestCreateGame
            {
                Realtime = true
            };

            var mapPath = Path.Combine(@"C:\Program Files (x86)\StarCraft II\Maps", "AcropolisLE.SC2Map");

            if (!File.Exists(mapPath))
            {
                Console.WriteLine("Unable to locate map: " + mapPath);
                throw new Exception("Unable to locate map: " + mapPath);
            }

            createGame.LocalMap = new LocalMap { MapPath = mapPath };

            var player1 = new PlayerSetup();
            createGame.PlayerSetup.Add(player1);
            player1.Type = PlayerType.Participant;

            var player2 = new PlayerSetup();
            createGame.PlayerSetup.Add(player2);
            player2.Race = Race.Terran;
            player2.Type = PlayerType.Computer;
            player2.Difficulty = Difficulty.VeryEasy;

            var request = new Request();
            request.CreateGame = createGame;
            await webSocketWrapper.SendRequestAsync(request);
            Console.WriteLine("request sent");
        }

        public async Task JoinGame(Race race)
        {
            var joinGame = new RequestJoinGame
            {
                Race = race,
                Options = new InterfaceOptions { Raw = true, Score = true }
            };
            
            var request = new Request {JoinGame = joinGame};
            await webSocketWrapper.SendRequestAsync(request);
        }
    }
}