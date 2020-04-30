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
        private Mock<IConstantManager> _constantMock;
        private const int AbilityId = 118;
        private const int Tag = 500;
        private const int UnitTypeIndex = 1;

        [SetUp]
        public void Setup()
        {
            _constantMock = new Mock<IConstantManager>();
            _constantMock.SetupGet(m => m.WorkerUnitIndex).Returns(ConstantManager.Scv);
            _constantMock.SetupGet(m => m.BaseTypeIds).Returns(new[] { ConstantManager.CommandCenter });
            // Mock index of SupplyUnit to 0
            _constantMock.SetupGet(m => m.SupplyUnit).Returns(UnitTypeIndex);

            _responseData = new ResponseData();
            // Index of worker matches above mocked constant 
            _responseData.Units.Add(new UnitTypeData { AbilityId = 999, UnitId = ConstantManager.Scv });
            _responseData.Units.Add(new UnitTypeData { AbilityId = AbilityId, UnitId = ConstantManager.SupplyDepot });
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
            var sut = new BuildingManager(connectionMock.Object, _constantMock.Object);

            await sut.Build(_currentObservation, _responseData, UnitTypeIndex, new ResponseGameInfo());

            actualRequest.Action.Actions[0].ActionRaw.UnitCommand.AbilityId.Should().Be(AbilityId);
            actualRequest.Action.Actions[0].ActionRaw.UnitCommand.UnitTags[0].Should().Be(Tag);
            actualRequest.Action.Actions[0].ActionRaw.UnitCommand.TargetWorldSpacePos.X.Should().BeInRange(0, 100);
            actualRequest.Action.Actions[0].ActionRaw.UnitCommand.TargetWorldSpacePos.Y.Should().BeInRange(0, 100);
        }
    }
}