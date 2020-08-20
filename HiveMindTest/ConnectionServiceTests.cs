using System;
using System.ComponentModel;
using System.IO;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using HiveMind;
using Moq;
using NUnit.Framework;

namespace HiveMindTest
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
            // Double the size of the payload
            var gRpcBigPayload = new byte[_gRpcReceivedBytes.Length * 2];
            Array.Copy(_gRpcReceivedBytes, 0, gRpcBigPayload, _gRpcReceivedBytes.Length, _gRpcReceivedBytes.Length);

            var webSocketMock = new Mock<IWebSocketWrapper>();
            webSocketMock.Setup(m => m.ReceiveAsync(It.IsAny<ArraySegment<byte>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((ArraySegment<byte> arraySegment, CancellationToken token) =>
                {
                    // This method will be called at least twice
                    int i;
                    var overallIndex = arraySegment.Offset;

                    // Write to the arraySegment starting from given offset
                    for (i = 0; i < arraySegment.Count && overallIndex < gRpcBigPayload.Length; i++)
                    {
                        arraySegment[i] = gRpcBigPayload[overallIndex];
                        overallIndex++;
                    }

                    // If all of gRpcBigPayload is sent, endOfMessage=true
                    var retObj = new WebSocketReceiveResult(i, WebSocketMessageType.Binary, overallIndex >= gRpcBigPayload.Length);
                    return retObj;
                });

            var sut = new ConnectionService(webSocketMock.Object);

            var actualBytes = await sut.ReceiveMessageAsync(CancellationToken.None);

            actualBytes.Should().BeEquivalentTo(gRpcBigPayload);
            webSocketMock.Verify(m => m.ReceiveAsync(It.IsAny<ArraySegment<byte>>(), It.IsAny<CancellationToken>()), Times.AtLeast(2),
                "ReceiveAsync should be called at least twice, to satisfy test. Ensure test payload _gRpcReceivedBytes is bigger than buffer used in ReceiveAsync");
        }
    }
}