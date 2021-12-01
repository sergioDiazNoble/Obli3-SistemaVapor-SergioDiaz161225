using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SimpleProtocolShared
{
    public interface INetworkStreamHandler
    {
        Task<byte[]> ReadAsync(int length);

        Task SendAsync(byte[] data);

    }
}
