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
        Ident,
        Message,
        MessageAck,
        PeerRequest,
        PeerResponse
    }

    /*
    ident :
    - int type = DataType.Ident
    - int name_size
    - char[] name
    - ? ip
    - int port

    message :
    - int type = DataType.Message
    - int msg_number
    - int msg_size
    - char[] msg

    message ack :
    - int type = DataType.MessageAck
    - int msg_number

    peer request :
    - int type = DataType.PeerRequest
    - int peer_name_size
    - char[] peer_name

    peer response :
    - int type = DataType.PeerResponse
    - int peer_name_size
    - char[] peer_name
    - ? ip
    - int port
    */

    public class IncomingData
    {
        public IPEndPoint EndPoint;
        public byte[] Data;
    }
}
