using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Google.Protobuf;
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
            Game.ResponseGameInfo = new ResponseGameInfo { StartRaw = new StartRaw() };
            Game.ResponseGameInfo.StartRaw.PlacementGrid = new ImageData
            {
                Data = ByteString.CopyFrom(new byte[] { 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255 }), // Each bit is 1
                Size = new Size2DI { X = 10, Y = 10 }
            };
            Game.ResponseGameInfo.StartRaw.PathingGrid = new ImageData
            {
                Data = ByteString.CopyFrom(new byte[] { 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255 }), // Each bit is 1
                Size = new Size2DI { X = 10, Y = 10 }
            };
            Game.ResponseGameInfo.StartRaw.PlayableArea = new RectangleI
            {
                P0 = new PointI { X = 1, Y = 1 },
                P1 = new PointI { X = 9, Y = 9 }
            };

            Game.MapManager = new MapManager(GridConverter.ToGroundTypeMatrix(Game.ResponseGameInfo.StartRaw.PlacementGrid, Game.ResponseGameInfo.StartRaw.PathingGrid, Game.ResponseGameInfo.StartRaw.PlayableArea)
                , new Point { X = 3, Y = 7, Z = 0 }); // Main Base is top left
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

            await sut.Build(_currentObservation, ConstantManager.SupplyDepot, 2, 2);

            actualRequest.Action.Actions[1].ActionRaw.UnitCommand.AbilityId.Should().Be(AbilityId);
            actualRequest.Action.Actions[1].ActionRaw.UnitCommand.UnitTags[0].Should().Be(Tag);
            actualRequest.Action.Actions[1].ActionRaw.UnitCommand.TargetWorldSpacePos.X.Should().Be(5.5F);
            actualRequest.Action.Actions[1].ActionRaw.UnitCommand.TargetWorldSpacePos.Y.Should().Be(7.5F);
        }
    }
}