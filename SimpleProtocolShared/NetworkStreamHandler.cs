﻿using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace SimpleProtocolShared
{
    public class NetworkStreamHandler : INetworkStreamHandler
    {
        private readonly NetworkStream _networkStream;

        public NetworkStreamHandler(NetworkStream networkStream)
        {
            _networkStream = networkStream;        
        }
        public async Task SendAsync(byte[] data)
        {
            await _networkStream.WriteAsync(data, 0, data.Length);
        }

        public async Task<byte[]> ReadAsync(int length)
        {
            int offset = 0;
            byte[] response = new byte[length];
            while (offset < length)
            {
                    int received = await _networkStream.ReadAsync(
                    response,
                    offset,
                    length - offset);
                if (received == 0)
                {
                    throw new SocketException();
                }

                offset += received;
            }
            return response;
        }

        public void Shutdown()
        {
            Console.WriteLine("Shutting down connection...");
            try
            {
                _networkStream.Close(1);
            }
            catch (SocketException)
            {
                Console.WriteLine("Connection already closed");
            }
        }
    }
}
