using System;
using Google.Protobuf;
using System.IO;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using SC2APIProtocol;

namespace bot
{
    class Program
    {
        static ClientWebSocket clientSocket;
        // pushd "C:\Program Files (x86)\StarCraft II\Support64\"   
        // ."C:\Program Files (x86)\StarCraft II\Versions\Base78285\SC2_x64.exe" -listen 127.0.0.1 -port 5678 
        static async Task Main(string[] args)
        {
            clientSocket = new ClientWebSocket();
            Console.WriteLine("Connecting to port 5678");
            await clientSocket.ConnectAsync(new Uri("ws://127.0.0.1:5678/sc2api"), CancellationToken.None);
            Console.WriteLine("Connected");
            Console.ReadLine();
            await CreateGame();
            Console.ReadLine();
        }

        private static async Task WriteMessage(Request request)
        {
            var sendBuf = new byte[1024 * 1024];
            var outStream = new CodedOutputStream(sendBuf);
            request.WriteTo(outStream);
            using CancellationTokenSource cancellationSource = new CancellationTokenSource();
            cancellationSource.CancelAfter(5000);
            await clientSocket.SendAsync(new ArraySegment<byte>(sendBuf, 0, (int)outStream.Position),
                WebSocketMessageType.Binary, true, cancellationSource.Token);
        }

        async static Task CreateGame()
        {
            var createGame = new RequestCreateGame
            {
                Realtime = false
            };

            var mapPath = Path.Combine(@"C:\Program Files (x86)\StarCraft II\Maps", "AcropolisLE.SC2Map");

            if (!File.Exists(mapPath))
            {
                Console.WriteLine("Unable to locate map: " + mapPath);
                throw new Exception("Unable to locate map: " + mapPath);
            }

            createGame.LocalMap = new LocalMap();
            createGame.LocalMap.MapPath = mapPath;

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
            await WriteMessage(request);
            Console.WriteLine("request sent");
        }
    }
}
