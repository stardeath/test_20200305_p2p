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
		private string m_AddressAsString;

		private Dictionary<string, (IPAddress, int)> m_Directory = new Dictionary<string, (IPAddress, int)>();

		private string m_Name;

		private int m_Port;

		private string m_Status = "not started";

		private ObservableCollection<Peer> m_Threads = new ObservableCollection<Peer>();

		private List<(string, int, string)> m_WaitingMessages = new List<(string, int, string)>();

		private Socket Socket = new Socket( AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp );

		public MainModel()
		{
			Instance = this;

			StartCommand = new RelayCommand( o => { OnStartCommand(); } );
			CreateThreadCommand = new RelayCommand( o => { OnCreateThreadCommand(); } );
		}

		public static MainModel Instance
		{
			get;
			private set;
		}
		public IPAddress Address
		{
			get
			{
				if( IPAddress.TryParse( AddressAsString, out var address ) )
				{
					return address;
				}
				else
				{
					return null;
				}
			}
			set
			{
				AddressAsString = value.ToString();
			}
		}

		public string AddressAsString
		{
			get
			{
				return m_AddressAsString;
			}
			set
			{
				SetProperty( ref m_AddressAsString, value );
			}
		}

		public RelayCommand CreateThreadCommand
		{
			get; private set;
		}

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

		public RelayCommand StartCommand
		{
			get; private set;
		}

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

		private BackgroundWorker Worker { get; set; } = new BackgroundWorker();

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
		private Peer GetOrCreate( string name )
		{
			Peer peer = Threads.FirstOrDefault( el => { return el.Name == name; } );
			if( peer == null )
			{
				peer = new Peer() { Name = name };
				Threads.Add( peer );
			}
			return peer;
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

					Peer peer = GetOrCreate( message_data.Item1 );
					peer.Address = message_data.Item2;
					peer.Port = message_data.Item3;

					{
						var waiting_messages = m_WaitingMessages.Where( wm => wm.Item1 == message_data.Item1 );
						var endpoint = m_Directory[message_data.Item1];
						foreach( var wm in waiting_messages )
						{
							if( wm.Item2 == 0 && peer.IsValid )
							{
								var ident_as_bytes = Codec.EncodeIdent( Name, Address, Port );
								Socket.SendTo( ident_as_bytes.ToArray(), new IPEndPoint( peer.Address, peer.Port ) );
							}

							var data = Codec.EncodeMessage( Name, wm.Item2, wm.Item3 );
							Socket.SendTo( data.ToArray(), new IPEndPoint( peer.Address, peer.Port ) );
						}
						m_WaitingMessages.RemoveAll( wm => wm.Item1 == message_data.Item1 );
					}
				}
				break;

				case DataType.Message:
				{
					var message_data = Codec.DecodeMessage( recv_bytes );

					Peer peer = GetOrCreate( message_data.Item1 );
					peer.AddMessage( new Message() { Status = Message.StatusDesc.Received, Data = message_data.Item3 } );

					// send ack
					if( peer.IsValid )
					{
						var data = Codec.EncodeMessageAck( Name, message_data.Item2 );
						Socket.SendTo( data.ToArray(), new IPEndPoint( peer.Address, peer.Port ) );
					}
					else
					{
						peer.AddMessage( new Message() { Status = Message.StatusDesc.Technical, Data = "no connection infos for " + peer.Name } );
					}
				}
				break;

				case DataType.MessageAck:
				{
					var message_data = Codec.DecodeMessageAck( recv_bytes );

					Peer peer = GetOrCreate( message_data.Item1 );
					peer.MarkAsReceived( message_data.Item2 );
				}
				break;

				case DataType.PeerRequest:
				{
					var message_data = Codec.DecodePeerRequest( recv_bytes );

					Peer peer = GetOrCreate( message_data.Item1 );
					peer.AddMessage( new Message() { Status = Message.StatusDesc.Technical, Data = message_data.Item1 + " requesting connection to " + message_data.Item2 } );

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
				Worker.ProgressChanged += new ProgressChangedEventHandler( Worker_ProgressChanged );

				Worker.RunWorkerAsync();
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

			Status = "stopped";
		}

		private void Worker_ProgressChanged( object sender, ProgressChangedEventArgs e )
		{
			if( e.UserState != null )
			{
				IncomingData id = e.UserState as IncomingData;

				HandleData( id.EndPoint, id.Data );
			}
		}
	}
}
