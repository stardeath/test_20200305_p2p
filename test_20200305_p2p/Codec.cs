using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace test_20200305_p2p
{
	public static class Codec
	{
		public static (string, IPAddress, int) DecodeIdent( byte[] data )
		{
			int offset = sizeof( int );

			int name_size = BitConverter.ToInt32( data, offset );
			offset += sizeof( int );
			string name = Encoding.UTF8.GetString( data, offset, name_size );
			offset += name_size;

			int address_size = BitConverter.ToInt32( data, offset );
			offset += sizeof( int );
			string address = Encoding.UTF8.GetString( data, offset, address_size );
			offset += address_size;

			int port = BitConverter.ToInt32( data, offset );
			offset += sizeof( int );

			return (name, IPAddress.Parse( address ), port);
		}

		public static (string, int, string) DecodeMessage( byte[] data )
		{
			int offset = sizeof( int );

			int sender_name_size = BitConverter.ToInt32( data, offset );
			offset += sizeof( int );
			string sender_name = Encoding.UTF8.GetString( data, offset, sender_name_size );
			offset += sender_name_size;

			int msg_number = BitConverter.ToInt32( data, offset );
			offset += sizeof( int );

			int msg_size = BitConverter.ToInt32( data, offset );
			offset += sizeof( int );
			string msg = Encoding.UTF8.GetString( data, offset, msg_size );
			offset += msg_size;

			return (sender_name, msg_number, msg);
		}

		public static (string, int) DecodeMessageAck( byte[] data )
		{
			int offset = sizeof( int );

			int sender_name_size = BitConverter.ToInt32( data, offset );
			offset += sizeof( int );
			string sender_name = Encoding.UTF8.GetString( data, offset, sender_name_size );
			offset += sender_name_size;

			int msg_number = BitConverter.ToInt32( data, offset );
			offset += sizeof( int );

			return (sender_name, msg_number);
		}

		public static (string, string) DecodePeerRequest( byte[] data )
		{
			int offset = sizeof( int );

			int requester_name_size = BitConverter.ToInt32( data, offset );
			offset += sizeof( int );
			string requester_name = Encoding.UTF8.GetString( data, offset, requester_name_size );
			offset += requester_name_size;

			int peer_name_size = BitConverter.ToInt32( data, offset );
			offset += sizeof( int );
			string peer_name = Encoding.UTF8.GetString( data, offset, peer_name_size );
			offset += peer_name_size;

			return (requester_name, peer_name);
		}

		public static (string, IPAddress, int) DecodePeerResponse( byte[] data )
		{
			int offset = sizeof( int );

			int name_size = BitConverter.ToInt32( data, offset );
			offset += sizeof( int );
			string name = Encoding.UTF8.GetString( data, offset, name_size );
			offset += name_size;

			int address_size = BitConverter.ToInt32( data, offset );
			offset += sizeof( int );
			string address = Encoding.UTF8.GetString( data, offset, address_size );
			offset += address_size;

			int port = BitConverter.ToInt32( data, offset );
			offset += sizeof( int );

			return (name, IPAddress.Parse( address ), port);
		}

		public static DataType DecodeType( byte[] data )
		{
			if( data.Length < sizeof( int ) )
			{
				return DataType.None;
			}

			int type = BitConverter.ToInt32( data, 0 );

			return ( DataType ) type;
		}

		public static byte[] EncodeIdent( string name, IPAddress address, int port )
		{
			List<byte> buffer = new List<byte>();

			byte[] name_as_bytes = Encoding.UTF8.GetBytes( name );
			byte[] address_as_bytes = Encoding.UTF8.GetBytes( address.ToString() );

			buffer.AddRange( BitConverter.GetBytes( ( int ) DataType.Ident ) );
			buffer.AddRange( BitConverter.GetBytes( ( int ) name_as_bytes.Length ) );
			buffer.AddRange( name_as_bytes );
			buffer.AddRange( BitConverter.GetBytes( ( int ) address_as_bytes.Length ) );
			buffer.AddRange( address_as_bytes );
			buffer.AddRange( BitConverter.GetBytes( ( int ) port ) );

			return buffer.ToArray();
		}

		public static byte[] EncodeMessage( string sender, int message_number, string message )
		{
			List<byte> buffer = new List<byte>();

			byte[] sender_as_bytes = Encoding.UTF8.GetBytes( sender );
			byte[] message_as_bytes = Encoding.UTF8.GetBytes( message );

			buffer.AddRange( BitConverter.GetBytes( ( int ) DataType.Message ) );
			buffer.AddRange( BitConverter.GetBytes( ( int ) sender_as_bytes.Length ) );
			buffer.AddRange( sender_as_bytes );
			buffer.AddRange( BitConverter.GetBytes( ( int ) message_number ) );
			buffer.AddRange( BitConverter.GetBytes( ( int ) message_as_bytes.Length ) );
			buffer.AddRange( message_as_bytes );

			return buffer.ToArray();
		}

		public static byte[] EncodeMessageAck( string sender, int message_number )
		{
			List<byte> buffer = new List<byte>();

			byte[] sender_as_bytes = Encoding.UTF8.GetBytes( sender );

			buffer.AddRange( BitConverter.GetBytes( ( int ) DataType.MessageAck ) );
			buffer.AddRange( BitConverter.GetBytes( ( int ) sender_as_bytes.Length ) );
			buffer.AddRange( sender_as_bytes );
			buffer.AddRange( BitConverter.GetBytes( ( int ) message_number ) );

			return buffer.ToArray();
		}

		public static byte[] EncodeNone()
		{
			List<byte> buffer = new List<byte>();
			buffer.AddRange( BitConverter.GetBytes( ( int ) DataType.None ) );
			return buffer.ToArray();
		}

		public static byte[] EncodePeerRequest( string requester_name, string peer_name )
		{
			List<byte> buffer = new List<byte>();

			byte[] requester_as_bytes = Encoding.UTF8.GetBytes( requester_name );
			byte[] peer_as_bytes = Encoding.UTF8.GetBytes( peer_name );

			buffer.AddRange( BitConverter.GetBytes( ( int ) DataType.PeerRequest ) );
			buffer.AddRange( BitConverter.GetBytes( ( int ) requester_as_bytes.Length ) );
			buffer.AddRange( requester_as_bytes );
			buffer.AddRange( BitConverter.GetBytes( ( int ) peer_as_bytes.Length ) );
			buffer.AddRange( peer_as_bytes );

			return buffer.ToArray();
		}

		public static byte[] EncodePeerResponse( string peer_name, IPAddress address, int port )
		{
			List<byte> buffer = new List<byte>();

			byte[] peer_as_bytes = Encoding.UTF8.GetBytes( peer_name );
			byte[] address_as_bytes = Encoding.UTF8.GetBytes( address.ToString() );

			buffer.AddRange( BitConverter.GetBytes( ( int ) DataType.Ident ) );
			buffer.AddRange( BitConverter.GetBytes( ( int ) peer_as_bytes.Length ) );
			buffer.AddRange( peer_as_bytes );
			buffer.AddRange( BitConverter.GetBytes( ( int ) address_as_bytes.Length ) );
			buffer.AddRange( address_as_bytes );
			buffer.AddRange( BitConverter.GetBytes( ( int ) port ) );

			return buffer.ToArray();
		}
	}
}
