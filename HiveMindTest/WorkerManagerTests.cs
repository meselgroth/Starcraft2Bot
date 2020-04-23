using System.Threading.Tasks;
using HiveMind;
using Moq;
using NUnit.Framework;
using SC2APIProtocol;

namespace HiveMindTest
{
    [TestFixture]
    public class WorkerManagerTests
    {
        [Test]
        public async Task ManageTest()
        {
            var sut = new WorkerManager(new ConnectionService(new Mock<IWebSocketWrapper>().Object));
            await sut.Manage(new Observation(), new ResponseData());
        }
    }
}