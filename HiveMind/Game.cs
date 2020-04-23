using System.Collections.Generic;
using System.Threading.Tasks;
using SC2APIProtocol;

namespace HiveMind
{
    public class Game
    {
        private readonly IWebSocketWrapper _webSocketWrapper;
        private readonly IConnectionService _connectionService;
        private readonly IWorkerManager _workerManager;
        private bool _surrender;
        private ResponseObservation _responseObservation;
        private ResponseData _responseData;

        public Game(IWebSocketWrapper webSocketWrapper, IConnectionService connectionService,
            IWorkerManager workerManager)
        {
            _surrender = false;
            _webSocketWrapper = webSocketWrapper;
            _connectionService = connectionService;
            _workerManager = workerManager;
        }

        public async Task Run()
        {
            await RequestGameData();

            var receiverTask = Receiver();
            while (_surrender != true)
            {
                await _connectionService.SendRequestAsync(new Request { Observation = new RequestObservation() });
                await Task.Delay(500);
            }

            await receiverTask;
        }

        private async Task RequestGameData()
        {
            var requestData = new RequestData
            {
                UnitTypeId = true,
                AbilityId = true,
                BuffId = true,
                EffectId = true,
                UpgradeId = true
            };
            await _connectionService.SendRequestAsync(new Request {Data = requestData});
        }

        private async Task Receiver()
        {
            Response response;
            List<Task> tasks = new List<Task>();

            do
            {
                response = await _connectionService.ReceiveRequestAsync();
                if (response.HasObservation)
                {
                    _responseObservation = response.Observation;
                    await _workerManager.Manage(_responseObservation.Observation, _responseData);
                }
                if (response.HasData)
                {
                    _responseData = response.Data;
                }

            } while (response.Status != Status.Ended || response.Status != Status.Quit);

        }
    }
}