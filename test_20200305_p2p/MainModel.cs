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
		public static MainModel Instance
		{
			get;
			private set;
		}

		private Dictionary<string, (IPAddress, int)> m_Directory = new Dictionary<string, (IPAddress, int)>();

		internal void Parse( string[] args )
		{
			if( args.Length > 0 )
			{
				char[] delimiters = { '@', ':' };

				for( int i = 0; i < args.Length; ++i )
				{
					string[] tokens = args[i].Split( delimiters, StringSplitOptions.None );

					if( i == 0 )
					{
						Name = tokens[0];
						if( tokens.Length > 1 )
							AddressAsString = tokens[1];
						if( tokens.Length > 2 )
							Port = int.Parse( tokens[2] );
					}

					if( i > 0 )
					{
						Peer peer = new Peer() { Name = tokens[0] };
						Threads.Add( peer );
						if( tokens.Length > 1 )
							peer.AddressAsString = tokens[1];
						if( tokens.Length > 2 )
							peer.Port = int.Parse( tokens[2] );
					}
				}
			}
		}

		private Socket Socket = new Socket( AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp );

		public MainModel()
		{
			Instance = this;

			//Threads.Add( new Peer() );

			StartCommand = new RelayCommand( o => { OnStartCommand(); } );
			CreateThreadCommand = new RelayCommand( o => { OnCreateThreadCommand(); } );
		}

		private void OnCreateThreadCommand()
		{
			Threads.Add( new Peer() { Name = "placeholder name" } );
		}

		private void OnStartCommand()
		{
			if( !Worker.IsBusy )
			{
				Worker.WorkerReportsProgress = true;
				Worker.WorkerSupportsCancellation = true;
				Worker.DoWork += new DoWorkEventHandler( Worker_DoWork );
				//Worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(Worker_RunWorkerCompleted);
				Worker.ProgressChanged += new ProgressChangedEventHandler( Worker_ProgressChanged );

				Worker.RunWorkerAsync();
			}
		}

		private List<(string, int, string)> m_WaitingMessages = new List<(string, int, string)>();

		public void EnqueueOutboundMessage( Peer receiver, int message_counter, string message )
		{
			if( m_Directory.ContainsKey( receiver.Name ) || receiver.IsValid )
			{
				if( message_counter == 0 && receiver.IsValid )
				{
					var ident_as_bytes = Codec.EncodeIdent( Name, Address, Port );
					Socket.SendTo( ident_as_bytes.ToArray(), new IPEndPoint( receiver.Address, receiver.Port ) );
				}

				if( !m_Directory.ContainsKey( receiver.Name ) )
				{
					m_Directory.Add( receiver.Name, (receiver.Address, receiver.Port) );
				}

				var endpoint = m_Directory[receiver.Name];
				var data = Codec.EncodeMessage( Name, message_counter, message );
				Socket.SendTo( data.ToArray(), new IPEndPoint( endpoint.Item1, endpoint.Item2 ) );
			}
			else
			{
				m_WaitingMessages.Add( (receiver.Name, message_counter, message) );
				foreach( var peer in Threads.Where( p => p.IsValid ) )
				{
					var data = Codec.EncodePeerRequest( Name, receiver.Name );
					Socket.SendTo( data.ToArray(), new IPEndPoint( peer.Address, peer.Port ) );
				}
			}
		}

		public void EnqueueOutboundIdent( Peer receiver )
		{
			if( m_Directory.ContainsKey( receiver.Name ) )
			{
				var endpoint = m_Directory[receiver.Name];

				var ident_as_bytes = Codec.EncodeIdent( Name, Address, Port );
				Socket.SendTo( ident_as_bytes.ToArray(), new IPEndPoint( endpoint.Item1, endpoint.Item2 ) );
			}
			else if( receiver.IsValid )
			{
				m_Directory.Add( receiver.Name, (receiver.Address, receiver.Port) );

				var ident_as_bytes = Codec.EncodeIdent( Name, Address, Port );
				Socket.SendTo( ident_as_bytes.ToArray(), new IPEndPoint( receiver.Address, receiver.Port ) );
			}
		}

		private void Worker_DoWork( object sender, DoWorkEventArgs e )
		{
			Status = "started";

			BackgroundWorker worker = sender as BackgroundWorker;

			UdpClient listener = new UdpClient( Port );
			IPEndPoint end_point = new IPEndPoint( IPAddress.Any, Port );

			try
			{
				while( !worker.CancellationPending )
				{
					byte[] bytes = listener.Receive( ref end_point );

					worker.ReportProgress( 0, new IncomingData { EndPoint = end_point, Data = bytes } );
				}
			}
			catch( SocketException ex )
			{
				Console.WriteLine( ex );
			}
			finally
			{
				listener.Close();
			}
		}

		private void Worker_ProgressChanged( object sender, ProgressChangedEventArgs e )
		{
			if( e.UserState != null )
			{
				IncomingData id = e.UserState as IncomingData;

				HandleData( id.EndPoint, id.Data );
			}
		}

		private void HandleData( IPEndPoint end_point, byte[] recv_bytes )
		{
			DataType type = Codec.DecodeType( recv_bytes );

			switch( type )
			{
				case DataType.Ident:
				{
					var message_data = Codec.DecodeIdent( recv_bytes );
					if( !m_Directory.ContainsKey( message_data.Item1 ) )
					{
						m_Directory.Add( message_data.Item1, (message_data.Item2, message_data.Item3) );
					}
					else
					{
						m_Directory[message_data.Item1] = (message_data.Item2, message_data.Item3);
					}
					Peer peer = Threads.FirstOrDefault( el => { return el.Name == message_data.Item1; } );
					if( peer != null )
					{
						peer.Address = message_data.Item2;
						peer.Port = message_data.Item3;
					}
				}
				break;

				case DataType.Message:
				{
					var message_data = Codec.DecodeMessage( recv_bytes );
					Peer peer = Threads.FirstOrDefault( el => { return el.Name == message_data.Item1; } );
					if( peer == null )
					{
						peer = new Peer() { Name = message_data.Item1 };
						Threads.Add( peer );

						if( m_Directory.ContainsKey( message_data.Item1 ) )
						{
							var endpoint = m_Directory[message_data.Item1];
							peer.Address = endpoint.Item1;
							peer.Port = endpoint.Item2;
						}

					}
					peer.AddMessage( new Message() { Status = Message.StatusDesc.Received, Data = message_data.Item3 } );

					// send ack
					var data = Codec.EncodeMessageAck( Name, message_data.Item2 );
					Socket.SendTo( data.ToArray(), new IPEndPoint( peer.Address, peer.Port ) );
				}
				break;

				case DataType.MessageAck:
				{
					var message_data = Codec.DecodeMessageAck( recv_bytes );

					Peer peer = Threads.FirstOrDefault( el => { return el.Name == message_data.Item1; } );
					if( peer != null )
					{
						peer.MarkAsReceived( message_data.Item2 );
					}
				}
				break;

				case DataType.PeerRequest:
				{
					var message_data = Codec.DecodePeerRequest( recv_bytes );

					if( m_Directory.ContainsKey( message_data.Item1 ) && m_Directory.ContainsKey( message_data.Item2 ) )
					{
						var requester = m_Directory[message_data.Item1];
						var endpoint = m_Directory[message_data.Item2];
						var data = Codec.EncodeIdent( message_data.Item2, endpoint.Item1, endpoint.Item2 );
						Socket.SendTo( data.ToArray(), new IPEndPoint( requester.Item1, requester.Item2 ) );
					}
				}
				break;

				case DataType.None:
					break;
			}
		}

		public RelayCommand StartCommand
		{
			get; private set;
		}

		public RelayCommand CreateThreadCommand
		{
			get; private set;
		}

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
				SetProperty( ref m_Name, value );
			}
		}

		private IPAddress m_Address;
		public IPAddress Address
		{
			get
			{
				return m_Address;
			}
			set
			{
				if( SetProperty( ref m_Address, value ) )
				{
					AddressAsString = value.ToString();
				}
			}
		}

		private string m_AddressAsString;
		public string AddressAsString
		{
			get
			{
				return m_AddressAsString;
			}
			set
			{
				if( SetProperty( ref m_AddressAsString, value ) )
				{
					if( IPAddress.TryParse( value, out var address ) )
					{
						Address = address;
					}
				}
			}
		}

		private int m_Port;
		public int Port
		{
			get
			{
				return m_Port;
			}
			set
			{
				SetProperty( ref m_Port, value );
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
				SetProperty( ref m_Status, value );
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
				SetProperty( ref m_Threads, value );
			}
		}
	}
}
