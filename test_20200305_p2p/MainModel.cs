using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace test_20200305_p2p
{
    public class MainModel : BindableBase
    {
        public MainModel()
        {
            Threads.Add(new Peer());

            StartCommand = new RelayCommand(o => { OnStartCommand(); });
        }

        private void OnStartCommand()
        {
            if(!Worker.IsBusy)
            {
                Worker.WorkerReportsProgress = true;
                Worker.WorkerSupportsCancellation = true;
                Worker.DoWork += new DoWorkEventHandler(Worker_DoWork);
                //Worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(Worker_RunWorkerCompleted);
                Worker.ProgressChanged += new ProgressChangedEventHandler(Worker_ProgressChanged);

                Worker.RunWorkerAsync();
            }
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            int port = int.Parse(Port);

            UdpClient listener = new UdpClient(port);
            IPEndPoint end_point = new IPEndPoint(IPAddress.Any, port);

            try
            {
                while (!worker.CancellationPending)
                {
                    byte[] bytes = listener.Receive(ref end_point);

                    worker.ReportProgress(0, new IncomingData { EndPoint = end_point, Data = bytes });
                }
            }
            catch (SocketException ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                listener.Close();
            }
        }

        private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if(e.UserState != null)
            {
                IncomingData id = e.UserState as IncomingData;

                HandleData(id.EndPoint, id.Data);
            }
        }

        private List<byte> RecvBuffer = new List<byte>();
        private void HandleData(IPEndPoint end_point, byte[] data)
        {
            Peer peer = Threads.FirstOrDefault(el => { return el.EndPoint != null && el.EndPoint.Equals(end_point); });
            if (peer == null)
            {
                peer = new Peer() { EndPoint = end_point };
                Threads.Add(peer);
            }

            RecvBuffer.AddRange(data);

            bool must_continue = true;
            while (must_continue)
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
                                    peer.AddMessage(new Message() { IsReceivedMessage = true, Data = msg });

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

                        case DataType.MessageAck:
                            break;
                    }
                }
                else
                {
                    must_continue = false;
                }
            }
        }

        public RelayCommand StartCommand { get; private set; }

        private BackgroundWorker Worker { get; set; } = new BackgroundWorker();

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

        private string m_Status;
        public string Status
        {
            get
            {
                return m_Status;
            }
            set
            {
                SetProperty(ref m_Status, value);
            }
        }

        private ObservableCollection<Peer> m_Threads = new ObservableCollection<Peer>();
        public ObservableCollection<Peer> Threads
        {
            get
            {
                return m_Threads;
            }
            set
            {
                SetProperty(ref m_Threads, value);
            }
        }
    }
}
