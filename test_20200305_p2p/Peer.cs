using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace test_20200305_p2p
{
    public class Peer : BindableBase
    {
        public Peer()
        {
            Messages.Add(new Message() { IsReceivedMessage = true, Data = "window init done" });

            SendCommand = new RelayCommand(o => { OnSendCommand(); });

            Socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        }

        private Socket Socket;

        private void OnSendCommand()
        {
            IPAddress ip = IPAddress.Parse(Ip);
            int port = int.Parse(Port);
            EndPoint = new IPEndPoint(ip, port);

            List<byte> buffer = new List<byte>(sizeof(int) * 2 + Message.Length * sizeof(char));

            buffer.AddRange(BitConverter.GetBytes((int)DataType.Message));
            buffer.AddRange(BitConverter.GetBytes((int)Message.Length));
            buffer.AddRange(Encoding.ASCII.GetBytes(Message));

            Messages.Add(new Message() { IsReceivedMessage = false, Data = Message });
            Socket.SendTo(buffer.ToArray(), EndPoint);
        }

        public RelayCommand SendCommand { get; private set; }

        private string m_Name;
        public string Name
        {
            get
            {
                return m_Name;
            }
            set
            {
                SetProperty(ref m_Name, value);
            }
        }

        private IPEndPoint m_EndPoint;
        public IPEndPoint EndPoint
        {
            get
            {
                return m_EndPoint;
            }
            set
            {
                SetProperty(ref m_EndPoint, value);
            }
        }

        private ObservableCollection<Message> m_Messages = new ObservableCollection<Message>();
        public ObservableCollection<Message> Messages
        {
            get
            {
                return m_Messages;
            }
            set
            {
                SetProperty(ref m_Messages, value);
            }
        }

        private string m_Ip;
        public string Ip
        {
            get
            {
                return m_Ip;
            }
            set
            {
                SetProperty(ref m_Ip, value);
            }
        }

        private string m_Port;
        public string Port
        {
            get
            {
                return m_Port;
            }
            set
            {
                SetProperty(ref m_Port, value);
            }
        }

        private string m_Message;
        public string Message
        {
            get
            {
                return m_Message;
            }
            set
            {
                SetProperty(ref m_Message, value);
            }
        }

        public void AddMessage(Message message)
        {
            Messages.Add(message);
        }

        List<byte> RecvBuffer = new List<byte>();
        public void HandleData(byte[] data)
        {
            RecvBuffer.AddRange(data);

            bool must_continue = true;
            while(must_continue)
            {
                byte[] recv_bytes = RecvBuffer.ToArray();

                if (recv_bytes.Length >= sizeof(int))
                {
                    int type = BitConverter.ToInt32(recv_bytes, 0);

                    switch ((DataType)type)
                    {
                        case DataType.Message:
                            if (recv_bytes.Length >= 2 * sizeof(int))
                            {
                                int size = BitConverter.ToInt32(recv_bytes, sizeof(int));
                                if (recv_bytes.Length >= 2 * sizeof(int) + size)
                                {
                                    string msg = Encoding.ASCII.GetString(recv_bytes, 2 * sizeof(int), size);
                                    Messages.Add(new Message() { IsReceivedMessage = true, Data = msg });

                                    RecvBuffer.RemoveRange(0, 2 * sizeof(int) + size);
                                }
                                else
                                {
                                    must_continue = false;
                                }
                            }
                            else
                            {
                                must_continue = false;
                            }
                            break;
                    }
                }
                else
                {
                    must_continue = false;
                }
            }

            //int type = BitConverter.ToInt32(recv_bytes, 0);
            //int size = BitConverter.ToInt32(recv_bytes, sizeof(int));
            //string msg = Encoding.ASCII.GetString(recv_bytes, 2 * sizeof(int), size);
        }
    }
}
