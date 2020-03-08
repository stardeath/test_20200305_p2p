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
		private int m_MessageCounter = 0;

		public Peer()
		{
			Messages.Add( new Message() { Status = test_20200305_p2p.Message.StatusDesc.Technical, Data = "window init done" } );

			SendCommand = new RelayCommand( o => { OnSendCommand(); } );
			SendIdentCommand = new RelayCommand( o => { OnSendIdentCommand(); } );
		}

		private void OnSendIdentCommand()
		{
			MainModel.Instance.EnqueueOutboundIdent( this );
		}

		private void OnSendCommand()
		{
			Messages.Add( new Message() { Status = test_20200305_p2p.Message.StatusDesc.Sending, Data = Message, Id = m_MessageCounter } );
			MainModel.Instance.EnqueueOutboundMessage( this, m_MessageCounter, Message );
			++m_MessageCounter;
		}

		public void MarkAsReceived( int id )
		{
			Message message = Messages.FirstOrDefault( m => m.Id == id );
			if( message != null )
			{
				message.Status = test_20200305_p2p.Message.StatusDesc.Sent;
			}
		}

		public bool IsValid
		{
			get
			{
				return !string.IsNullOrEmpty( Name ) && Address != null && Port != 0;
			}
		}

		public RelayCommand SendCommand
		{
			get; private set;
		}
		public RelayCommand SendIdentCommand
		{
			get; private set;
		}

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

		private ObservableCollection<Message> m_Messages = new ObservableCollection<Message>();
		public ObservableCollection<Message> Messages
		{
			get
			{
				return m_Messages;
			}
			set
			{
				SetProperty( ref m_Messages, value );
			}
		}

		private string m_Message = "";
		public string Message
		{
			get
			{
				return m_Message;
			}
			set
			{
				SetProperty( ref m_Message, value );
			}
		}

		public void AddMessage( Message message )
		{
			Messages.Add( message );
		}
	}
}
