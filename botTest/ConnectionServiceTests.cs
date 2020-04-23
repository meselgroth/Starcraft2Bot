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
            if (_gRpcReceivedBytes.Length < 4000)
            {
                throw new Exception("Byte dump file is not big enough, run CallWebSocketAndSaveByteResponse test again");
            }
        }

        [Test]
        public async Task ReceiveMessageAsync()
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
                .ReturnsAsync(() => new WebSocketReceiveResult(_gRpcReceivedBytes.Length,
                    WebSocketMessageType.Binary, true));

            var sut = new ConnectionService(webSocketMock.Object);

            var actualBytes = await sut.ReceiveMessageAsync(CancellationToken.None);

            actualBytes.Should().BeEquivalentTo(_gRpcReceivedBytes);
        }
        [Test]
        public async Task ReceiveMessageAsync2SocketResponses()
        {
            var firstCall = true;
            var webSocketMock = new Mock<IWebSocketWrapper>();
            webSocketMock.Setup(m => m.ReceiveAsync(It.IsAny<ArraySegment<byte>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((ArraySegment<byte> arraySegment, CancellationToken token) =>
                {
                    // Write a portion of each byte to ArraySegment parameter
                    var length = firstCall ? 2000 : arraySegment.Count;
                    for (var i = arraySegment.Offset; i < length; i++)
                    {
                        arraySegment[i-arraySegment.Offset] = _gRpcReceivedBytes[i];
                    }
                    var retObj = new WebSocketReceiveResult(length, WebSocketMessageType.Binary, !firstCall);
                    firstCall = false;
                    return retObj;
                });

            var sut = new ConnectionService(webSocketMock.Object);

            var actualBytes = await sut.ReceiveMessageAsync(CancellationToken.None);

            actualBytes.Should().BeEquivalentTo(_gRpcReceivedBytes);
        }
    }
}