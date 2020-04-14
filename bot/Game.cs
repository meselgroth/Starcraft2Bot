using System.Threading.Tasks;
using SC2APIProtocol;

namespace bot
{
    public class Game
    {
        private readonly WebSocketWrapper webSocketWrapper;
        private bool surrender;

        public Game(WebSocketWrapper webSocketWrapper)
        {
            surrender = false;
            this.webSocketWrapper = webSocketWrapper;
        }

        public async Task Run()
        {
            var receiverTask = Receiver();
            while (surrender != true)
            {
                await webSocketWrapper.SendRequestAsync(new Request { Observation = new RequestObservation()});
                await Task.Delay(500);
            }

            await receiverTask;
        }

        private async Task Receiver()
        {
            Response response;
            do
            {
                response = await webSocketWrapper.ReceiveRequestAsync();
            } while (response.Status != Status.Ended || response.Status != Status.Quit);
        }
    }
}