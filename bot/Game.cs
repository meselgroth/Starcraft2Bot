using System.Threading.Tasks;
using SC2APIProtocol;

namespace bot
{
    public class Game
    {
        private readonly IWebSocketWrapper _webSocketWrapper;
        private readonly IConnectionService _connectionService;
        private bool _surrender;

        public Game(IWebSocketWrapper webSocketWrapper, IConnectionService connectionService)
        {
            _surrender = false;
            _webSocketWrapper = webSocketWrapper;
            _connectionService = connectionService;
        }

        public async Task Run()
        {
            var receiverTask = Receiver();
            while (_surrender != true)
            {
                await _connectionService.SendRequestAsync(new Request { Observation = new RequestObservation()});
                await Task.Delay(500);
            }

            await receiverTask;
        }

        private async Task Receiver()
        {
            Response response;
            do
            {
                response = await _connectionService.ReceiveRequestAsync();
                if (response.HasObservation)
                {
                    //response.Observation.Observation.PlayerCommon.FoodUsed;
                }
            } while (response.Status != Status.Ended || response.Status != Status.Quit);
        }
    }
}