using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using bot;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace botTest
{
    public class ConnectionServiceTests
    {
        private byte[] _gRpcReceivedBytes;

        [OneTimeSetUp]
        public void Setup()
        {
            _gRpcReceivedBytes = File.ReadAllBytes("./byteDumpObservationMsg");
        }

        [Test]
        public async Task ReceiveRequestAsync()
        {
            var webSocketMock = new Mock<IWebSocketWrapper>();
            // Setup ReceiveAsync to fill passed in ArraySegment with _gRpcReceivedBytes
            webSocketMock.Setup(m => m.ReceiveAsync(It.IsAny<ArraySegment<byte>>(), It.IsAny<CancellationToken>()))
                .Callback<ArraySegment<byte>, CancellationToken>((arraySegment, token) =>
                {
                    // Write each byte to ArraySegment parameter
                    for (var i = 0; i < _gRpcReceivedBytes.Length; i++)
                    {
                        arraySegment[i] = _gRpcReceivedBytes[i];
                    }
                })
                .Returns(Task.FromResult(new WebSocketReceiveResult(_gRpcReceivedBytes.Length,
                        WebSocketMessageType.Binary, true)));

            var sut = new ConnectionService(webSocketMock.Object);

            var actualBytes = await sut.ReceiveMessageAsync(CancellationToken.None);

            actualBytes.Should().BeEquivalentTo(_gRpcReceivedBytes);
        }
    }
}