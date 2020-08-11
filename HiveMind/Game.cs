using System.Collections.Generic;
using System.Threading.Tasks;
using SC2APIProtocol;

namespace HiveMind
{
    public class Game
    {
        private readonly IConnectionService _connectionService;
        private readonly IWorkerManager _workerManager;
        private readonly IBuildQueue _buildQueue;
        private bool _surrender;
        private ResponseObservation _responseObservation;
        public static ResponseData ResponseData;
        public static ResponseGameInfo ResponseGameInfo;

        public static Task Task { get; set; }

        public Game(IConnectionService connectionService,
            IWorkerManager workerManager, IBuildQueue buildQueue)
        {
            _surrender = false;
            _connectionService = connectionService;
            _workerManager = workerManager;
            _buildQueue = buildQueue;
        }

        public async Task Run()
        {
            await RequestGameData();

            var receiverTask = Receiver();
            while (!_surrender)
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
            await _connectionService.SendRequestAsync(new Request { Data = requestData });

            var requestGameInfo = new RequestGameInfo();
            await _connectionService.SendRequestAsync(new Request { GameInfo = requestGameInfo }); // Todo: Join requests?
        }

        private async Task Receiver()
        {
            Response response;
            var tasks = new List<Task>();

            do
            {
                response = await _connectionService.ReceiveRequestAsync();
                if (response.HasObservation)
                {
                    _responseObservation = response.Observation;
                    await _workerManager.Manage(_responseObservation.Observation);
                    await _buildQueue.Act(_responseObservation.Observation);
                }
                if (response.HasData)
                {
                    ResponseData = response.Data;
                }
                if (response.HasGameInfo)
                {
                    ResponseGameInfo = response.GameInfo;
                }

            } while (response.Status != Status.Ended || response.Status != Status.Quit);

        }
    }
}