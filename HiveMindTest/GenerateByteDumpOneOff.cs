using System.IO;
using System.Threading;
using System.Threading.Tasks;
using HiveMind;
using NUnit.Framework;
using SC2APIProtocol;

namespace HiveMindTest
{
    public class GenerateByteDumpOneOff
    {
        //Requires Sc2 to be running
        [Test]
        [Ignore("One off to save Sc2 websocket byte response to file")]
        public async Task CallWebSocketAndSaveByteResponse()
        {
            var webSocketWrapper = new WebSocketWrapper();
            await webSocketWrapper.ConnectWebSocket();
            var connectionService = new ConnectionService(webSocketWrapper);

            var gameStarter = new GameStarter(connectionService);
            await gameStarter.CreateGame();
            await gameStarter.JoinGame(Race.Terran);

            // Ensure game is underway so observation has juicy stuff
            await Task.Delay(5000);

            await connectionService.SendRequestAsync(new Request { Observation = new RequestObservation() });
            var bytes = await connectionService.ReceiveMessageAsync(CancellationToken.None);

            File.WriteAllBytes("../../../byteDumpObservationMsg", bytes);
        }
    }
}
