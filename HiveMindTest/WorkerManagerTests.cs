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
        private ResponseData _responseData;
        private Observation _currentObservation;
        private Mock<IConstantManager> _constantMock;
        private const int AbilityId = 118;
        private const int Tag = 500;

        [SetUp]
        public void Setup()
        {
            _constantMock = new Mock<IConstantManager>();
            // Mock index of worker to 0
            _constantMock.SetupGet(m => m.WorkerUnitIndex).Returns(0);
            _constantMock.SetupGet(m => m.GetBaseTypeIds).Returns(new[] { ConstantManager.CommandCenter });

            _responseData = new ResponseData();
            // Index of worker matches above mocked constant 
            _responseData.Units.Add(new UnitTypeData { AbilityId = AbilityId, UnitId = ConstantManager.Scv });
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
            var sut = new WorkerManager(connectionMock.Object, _constantMock.Object);

            await sut.Manage(_currentObservation, _responseData);

            actualRequest.Action.Actions[0].ActionRaw.UnitCommand.AbilityId.Should().Be(AbilityId);
            actualRequest.Action.Actions[0].ActionRaw.UnitCommand.UnitTags[0].Should().Be(Tag);
        }
    }
}