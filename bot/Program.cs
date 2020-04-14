using System;
using System.Threading.Tasks;
using SC2APIProtocol;

namespace bot
{
    public class Program
    {
        private static WebSocketWrapper _webSocketWrapper;

        // pushd 'C:\Program Files (x86)\StarCraft II\Support64\'   
        // & 'C:\Program Files (x86)\StarCraft II\Versions\Base78285\SC2_x64.exe' -listen 127.0.0.1 -port 5678 
        static async Task Main(string[] args)
        {
            _webSocketWrapper = new WebSocketWrapper();
            await _webSocketWrapper.ConnectWebSocket();
            
            var gameStarter = new GameStarter(_webSocketWrapper);
            await gameStarter.CreateGame();
            await gameStarter.JoinGame(Race.Terran);

            var game = new Game(_webSocketWrapper);
            await game.Run();
            Console.ReadLine();
        }
    }
}
