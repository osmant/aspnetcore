﻿using Microsoft.Net.WebSockets.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Net.WebSockets.Test
{
    public class WebSocketClientTests
    {
        private static string ClientAddress = "ws://localhost:8080/";
        private static string ServerAddress = "http://localhost:8080/";

        [Fact]
        public async Task Connect_Success()
        {
            using (HttpListener listener = new HttpListener())
            {
                listener.Prefixes.Add(ServerAddress);
                listener.Start();
                Task<HttpListenerContext> serverAccept = listener.GetContextAsync();

                WebSocketClient client = new WebSocketClient();
                Task<WebSocket> clientConnect = client.ConnectAsync(new Uri(ClientAddress), CancellationToken.None);

                HttpListenerContext serverContext = await serverAccept;
                Assert.True(serverContext.Request.IsWebSocketRequest);
                HttpListenerWebSocketContext serverWebSocketContext = await serverContext.AcceptWebSocketAsync(null);

                WebSocket clientSocket = await clientConnect;
                clientSocket.Dispose();
            }
        }

        [Fact]
        public async Task SendShortData_Success()
        {
            using (HttpListener listener = new HttpListener())
            {
                listener.Prefixes.Add(ServerAddress);
                listener.Start();
                Task<HttpListenerContext> serverAccept = listener.GetContextAsync();

                WebSocketClient client = new WebSocketClient();
                Task<WebSocket> clientConnect = client.ConnectAsync(new Uri(ClientAddress), CancellationToken.None);

                HttpListenerContext serverContext = await serverAccept;
                Assert.True(serverContext.Request.IsWebSocketRequest);
                HttpListenerWebSocketContext serverWebSocketContext = await serverContext.AcceptWebSocketAsync(null);

                WebSocket clientSocket = await clientConnect;

                byte[] orriginalData = Encoding.UTF8.GetBytes("Hello World");
                await clientSocket.SendAsync(new ArraySegment<byte>(orriginalData), WebSocketMessageType.Binary, true, CancellationToken.None);

                byte[] serverBuffer = new byte[orriginalData.Length];
                WebSocketReceiveResult result = await serverWebSocketContext.WebSocket.ReceiveAsync(new ArraySegment<byte>(serverBuffer), CancellationToken.None);
                Assert.True(result.EndOfMessage);
                Assert.Equal(orriginalData.Length, result.Count);
                Assert.Equal(WebSocketMessageType.Binary, result.MessageType);
                Assert.Equal(orriginalData, serverBuffer);

                clientSocket.Dispose();
            }
        }

        [Fact]
        public async Task SendMediumData_Success()
        {
            using (HttpListener listener = new HttpListener())
            {
                listener.Prefixes.Add(ServerAddress);
                listener.Start();
                Task<HttpListenerContext> serverAccept = listener.GetContextAsync();

                WebSocketClient client = new WebSocketClient();
                Task<WebSocket> clientConnect = client.ConnectAsync(new Uri(ClientAddress), CancellationToken.None);

                HttpListenerContext serverContext = await serverAccept;
                Assert.True(serverContext.Request.IsWebSocketRequest);
                HttpListenerWebSocketContext serverWebSocketContext = await serverContext.AcceptWebSocketAsync(null);

                WebSocket clientSocket = await clientConnect;

                byte[] orriginalData = Encoding.UTF8.GetBytes(new string('a', 130));
                await clientSocket.SendAsync(new ArraySegment<byte>(orriginalData), WebSocketMessageType.Text, true, CancellationToken.None);

                byte[] serverBuffer = new byte[orriginalData.Length];
                WebSocketReceiveResult result = await serverWebSocketContext.WebSocket.ReceiveAsync(new ArraySegment<byte>(serverBuffer), CancellationToken.None);
                Assert.True(result.EndOfMessage);
                Assert.Equal(orriginalData.Length, result.Count);
                Assert.Equal(WebSocketMessageType.Text, result.MessageType);
                Assert.Equal(orriginalData, serverBuffer);

                clientSocket.Dispose();
            }
        }

        [Fact]
        public async Task SendLongData_Success()
        {
            using (HttpListener listener = new HttpListener())
            {
                listener.Prefixes.Add(ServerAddress);
                listener.Start();
                Task<HttpListenerContext> serverAccept = listener.GetContextAsync();

                WebSocketClient client = new WebSocketClient();
                Task<WebSocket> clientConnect = client.ConnectAsync(new Uri(ClientAddress), CancellationToken.None);

                HttpListenerContext serverContext = await serverAccept;
                Assert.True(serverContext.Request.IsWebSocketRequest);
                HttpListenerWebSocketContext serverWebSocketContext = await serverContext.AcceptWebSocketAsync(null, 0xFFFF, TimeSpan.FromMinutes(100));

                WebSocket clientSocket = await clientConnect;

                byte[] orriginalData = Encoding.UTF8.GetBytes(new string('a', 0x1FFFF));
                await clientSocket.SendAsync(new ArraySegment<byte>(orriginalData), WebSocketMessageType.Text, true, CancellationToken.None);

                byte[] serverBuffer = new byte[orriginalData.Length];
                WebSocketReceiveResult result = await serverWebSocketContext.WebSocket.ReceiveAsync(new ArraySegment<byte>(serverBuffer), CancellationToken.None);
                int intermediateCount = result.Count;
                Assert.False(result.EndOfMessage);
                Assert.Equal(WebSocketMessageType.Text, result.MessageType);

                result = await serverWebSocketContext.WebSocket.ReceiveAsync(new ArraySegment<byte>(serverBuffer, intermediateCount, orriginalData.Length - intermediateCount), CancellationToken.None);
                intermediateCount += result.Count;
                Assert.False(result.EndOfMessage);
                Assert.Equal(WebSocketMessageType.Text, result.MessageType);

                result = await serverWebSocketContext.WebSocket.ReceiveAsync(new ArraySegment<byte>(serverBuffer, intermediateCount, orriginalData.Length - intermediateCount), CancellationToken.None);
                intermediateCount += result.Count;
                Assert.True(result.EndOfMessage);
                Assert.Equal(orriginalData.Length, intermediateCount);
                Assert.Equal(WebSocketMessageType.Text, result.MessageType);

                Assert.Equal(orriginalData, serverBuffer);

                clientSocket.Dispose();
            }
        }

        [Fact]
        public async Task ReceiveShortData_Success()
        {
            using (HttpListener listener = new HttpListener())
            {
                listener.Prefixes.Add(ServerAddress);
                listener.Start();
                Task<HttpListenerContext> serverAccept = listener.GetContextAsync();

                WebSocketClient client = new WebSocketClient();
                Task<WebSocket> clientConnect = client.ConnectAsync(new Uri(ClientAddress), CancellationToken.None);

                HttpListenerContext serverContext = await serverAccept;
                Assert.True(serverContext.Request.IsWebSocketRequest);
                HttpListenerWebSocketContext serverWebSocketContext = await serverContext.AcceptWebSocketAsync(null);

                WebSocket clientSocket = await clientConnect;

                byte[] orriginalData = Encoding.UTF8.GetBytes("Hello World");
                await serverWebSocketContext.WebSocket.SendAsync(new ArraySegment<byte>(orriginalData), WebSocketMessageType.Binary, true, CancellationToken.None);

                byte[] clientBuffer = new byte[orriginalData.Length];
                WebSocketReceiveResult result = await clientSocket.ReceiveAsync(new ArraySegment<byte>(clientBuffer), CancellationToken.None);
                Assert.True(result.EndOfMessage);
                Assert.Equal(orriginalData.Length, result.Count);
                Assert.Equal(WebSocketMessageType.Binary, result.MessageType);
                Assert.Equal(orriginalData, clientBuffer);

                clientSocket.Dispose();
            }
        }

        [Fact]
        public async Task ReceiveMediumData_Success()
        {
            using (HttpListener listener = new HttpListener())
            {
                listener.Prefixes.Add(ServerAddress);
                listener.Start();
                Task<HttpListenerContext> serverAccept = listener.GetContextAsync();

                WebSocketClient client = new WebSocketClient();
                Task<WebSocket> clientConnect = client.ConnectAsync(new Uri(ClientAddress), CancellationToken.None);

                HttpListenerContext serverContext = await serverAccept;
                Assert.True(serverContext.Request.IsWebSocketRequest);
                HttpListenerWebSocketContext serverWebSocketContext = await serverContext.AcceptWebSocketAsync(null);

                WebSocket clientSocket = await clientConnect;

                byte[] orriginalData = Encoding.UTF8.GetBytes(new string('a', 130));
                await serverWebSocketContext.WebSocket.SendAsync(new ArraySegment<byte>(orriginalData), WebSocketMessageType.Binary, true, CancellationToken.None);

                byte[] clientBuffer = new byte[orriginalData.Length];
                WebSocketReceiveResult result = await clientSocket.ReceiveAsync(new ArraySegment<byte>(clientBuffer), CancellationToken.None);
                Assert.True(result.EndOfMessage);
                Assert.Equal(orriginalData.Length, result.Count);
                Assert.Equal(WebSocketMessageType.Binary, result.MessageType);
                Assert.Equal(orriginalData, clientBuffer);

                clientSocket.Dispose();
            }
        }

        [Fact]
        public async Task ReceiveLongDataInSmallBuffer_Success()
        {
            using (HttpListener listener = new HttpListener())
            {
                listener.Prefixes.Add(ServerAddress);
                listener.Start();
                Task<HttpListenerContext> serverAccept = listener.GetContextAsync();

                WebSocketClient client = new WebSocketClient();
                Task<WebSocket> clientConnect = client.ConnectAsync(new Uri(ClientAddress), CancellationToken.None);

                HttpListenerContext serverContext = await serverAccept;
                Assert.True(serverContext.Request.IsWebSocketRequest);
                HttpListenerWebSocketContext serverWebSocketContext = await serverContext.AcceptWebSocketAsync(null);

                WebSocket clientSocket = await clientConnect;

                byte[] orriginalData = Encoding.UTF8.GetBytes(new string('a', 0x1FFFF));
                await serverWebSocketContext.WebSocket.SendAsync(new ArraySegment<byte>(orriginalData), WebSocketMessageType.Binary, true, CancellationToken.None);

                byte[] clientBuffer = new byte[orriginalData.Length];
                WebSocketReceiveResult result;
                int receivedCount = 0;
                do
                {
                    result = await clientSocket.ReceiveAsync(new ArraySegment<byte>(clientBuffer, receivedCount, clientBuffer.Length - receivedCount), CancellationToken.None);
                    receivedCount += result.Count;
                    Assert.Equal(WebSocketMessageType.Binary, result.MessageType);
                }
                while (!result.EndOfMessage);

                Assert.Equal(orriginalData.Length, receivedCount);
                Assert.Equal(WebSocketMessageType.Binary, result.MessageType);
                Assert.Equal(orriginalData, clientBuffer);

                clientSocket.Dispose();
            }
        }

        [Fact]
        public async Task ReceiveLongDataInLargeBuffer_Success()
        {
            using (HttpListener listener = new HttpListener())
            {
                listener.Prefixes.Add(ServerAddress);
                listener.Start();
                Task<HttpListenerContext> serverAccept = listener.GetContextAsync();

                WebSocketClient client = new WebSocketClient() { ReceiveBufferSize = 0xFFFFFF };
                Task<WebSocket> clientConnect = client.ConnectAsync(new Uri(ClientAddress), CancellationToken.None);

                HttpListenerContext serverContext = await serverAccept;
                Assert.True(serverContext.Request.IsWebSocketRequest);
                HttpListenerWebSocketContext serverWebSocketContext = await serverContext.AcceptWebSocketAsync(null);

                WebSocket clientSocket = await clientConnect;

                byte[] orriginalData = Encoding.UTF8.GetBytes(new string('a', 0x1FFFF));
                await serverWebSocketContext.WebSocket.SendAsync(new ArraySegment<byte>(orriginalData), WebSocketMessageType.Binary, true, CancellationToken.None);

                byte[] clientBuffer = new byte[orriginalData.Length];
                WebSocketReceiveResult result = await clientSocket.ReceiveAsync(new ArraySegment<byte>(clientBuffer), CancellationToken.None);
                Assert.True(result.EndOfMessage);
                Assert.Equal(orriginalData.Length, result.Count);
                Assert.Equal(WebSocketMessageType.Binary, result.MessageType);
                Assert.Equal(orriginalData, clientBuffer);

                clientSocket.Dispose();
            }
        }
    }
}
