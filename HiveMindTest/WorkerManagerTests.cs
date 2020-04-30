using System.Threading.Tasks;
using FluentAssertions;
using HiveMind;
using Moq;
using NUnit.Framework;
using SC2APIProtocol;

namespace HiveMindTest
{
    [TestFixture]
    public class WorkerManagerTests
    {
        private Observation _currentObservation;
        private const int AbilityId = 118;
        private const int Tag = 500;

        [SetUp]
        public void Setup()
        {
            _currentObservation = new Observation
            {
                PlayerCommon = new PlayerCommon { FoodWorkers = 0 },
                RawData = new ObservationRaw { Units = { new Unit { UnitType = ConstantManager.CommandCenter, Alliance = Alliance.Self, BuildProgress = 1, Tag = Tag } } }
            };
        }

        [Test]
        public async Task CreateWorkerOnOneBaseTest()
        {
            Request actualRequest = null;
            var connectionMock = new Mock<IConnectionService>();
            connectionMock.Setup(m => m.SendRequestAsync(It.IsAny<Request>()))
                .Callback((Request r) => actualRequest = r);

            var gameDataServiceMock = new Mock<IGameDataService>();
            gameDataServiceMock.Setup(m => m.GetAbilityId(ConstantManager.Scv)).Returns(AbilityId);
            
            var sut = new WorkerManager(connectionMock.Object, new ConstantManager(Race.Terran), gameDataServiceMock.Object);

            await sut.Manage(_currentObservation);

            actualRequest.Action.Actions[0].ActionRaw.UnitCommand.AbilityId.Should().Be(AbilityId);
            actualRequest.Action.Actions[0].ActionRaw.UnitCommand.UnitTags[0].Should().Be(Tag);
        }
    }
}