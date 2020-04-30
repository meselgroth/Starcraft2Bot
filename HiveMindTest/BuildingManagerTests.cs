using System.Threading.Tasks;
using FluentAssertions;
using HiveMind;
using Moq;
using NUnit.Framework;
using SC2APIProtocol;

namespace HiveMindTest
{
    [TestFixture]
    public class BuildingManagerTests
    {
        private ResponseData _responseData;
        private Observation _currentObservation;
        private const int AbilityId = 118;
        private const int Tag = 500;

        [SetUp]
        public void Setup()
        {
            _currentObservation = new Observation
            {
                PlayerCommon = new PlayerCommon { FoodWorkers = 0 },
                RawData = new ObservationRaw { Units = { new Unit { UnitType = ConstantManager.Scv, Alliance = Alliance.Self, BuildProgress = 1, Tag = Tag, Pos = new Point { X = 20, Y = 25 } } } }
            };
        }

        [Test]
        public async Task BuildSupplyDepotTest()
        {
            Request actualRequest = null;
            var connectionMock = new Mock<IConnectionService>();
            connectionMock.Setup(m => m.SendRequestAsync(It.IsAny<Request>()))
                .Callback((Request r) => actualRequest = r);

            var gameDataServiceMock = new Mock<IGameDataService>();
            gameDataServiceMock.Setup(m => m.GetAbilityId(ConstantManager.SupplyDepot)).Returns(AbilityId);
            
            var sut = new BuildingManager(connectionMock.Object, new ConstantManager(Race.Terran), gameDataServiceMock.Object);

            await sut.Build(_currentObservation, ConstantManager.SupplyDepot, new ResponseGameInfo());

            actualRequest.Action.Actions[0].ActionRaw.UnitCommand.AbilityId.Should().Be(AbilityId);
            actualRequest.Action.Actions[0].ActionRaw.UnitCommand.UnitTags[0].Should().Be(Tag);
            actualRequest.Action.Actions[0].ActionRaw.UnitCommand.TargetWorldSpacePos.X.Should().BeInRange(0, 100);
            actualRequest.Action.Actions[0].ActionRaw.UnitCommand.TargetWorldSpacePos.Y.Should().BeInRange(0, 100);
        }
    }
}