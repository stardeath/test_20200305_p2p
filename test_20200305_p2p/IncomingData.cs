using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace test_20200305_p2p
{
    public enum DataType
    {
        None = 0,
        Message,
        MessageAck,
        PeerRequest,
        PeerResponse
    }

    public class IncomingData
    {
        public IPEndPoint EndPoint;
        public byte[] Data;
    }
}
