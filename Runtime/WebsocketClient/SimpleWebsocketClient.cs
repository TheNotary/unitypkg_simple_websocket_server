﻿using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace SimpleWebsocketServer
{
    public class SimpleWebsocketClient
    {
        private string localAddress;
        private int port;
        private string websocketToken;
        private LinkedList<Thread> threads = new LinkedList<Thread>();

        public INetworkStream? _stream;
        public WebsocketClient? websocketClient;

        public SimpleWebsocketClient(string localAddress, int port)
        {
            this.localAddress = localAddress;
            this.port = port;
            this.websocketToken = Convert.ToBase64String(Encoding.UTF8.GetBytes(GenerateRandomPassword()));
        }

        public void Connect()
        {
            TcpClient tcpClient = new TcpClient(this.localAddress, this.port);
            _stream = new NetworkStreamProxy(tcpClient.GetStream());
            websocketClient = new WebsocketClient(_stream);

            while (!tcpClient.Connected) ;   // Block until connected

            // perform handshake...
            string handshake = $"GET / HTTP/1.1\r\nHost: server.example.com\r\nUpgrade: websocket\r\nSec-WebSocket-Key: ${websocketToken}\r\n\r\n";
            byte[] handshakeBytes = Encoding.UTF8.GetBytes(handshake);
            _stream.Write(handshakeBytes, 0, handshakeBytes.Length);

            // Consume handshake response
            ConsumeHandshakeResponse();
        }

        public WebsocketFrame ReceiveMessageFromClient()
        {
            if (websocketClient == null)
                throw new InvalidOperationException("ReceiveMessageFromClient was called when websocketClient was null.");
            return websocketClient.ReceiveMessageFromClient();
        }

        public void SendMessage(string v)
        {
            websocketClient.SendMessage(v, true);
        }

        private void ConsumeHandshakeResponse()
        {
            websocketClient.ConsumeHandshakeResponse();
        }

        internal static string GenerateRandomPassword()
        {
            return WebsocketClient.GenerateRandomPassword();
        }
    }
}