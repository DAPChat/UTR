using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

public class Client
{
	private readonly TCP tcp;
	private readonly UDP udp;
	private readonly int id;

	public bool active;
	//private Player player;
	//private Account account;

	public Client(TcpClient _tcpClient, int _id)
	{
		tcp = new(_tcpClient, this);
		udp = new(_tcpClient.Client, this);
		id = _id;

		active = true;
	}

	public void SendTCP(byte[] msg)
	{
		tcp.Send(msg);
	}

	public void SendUDP(byte[] msg)
	{
		udp.Send(msg);
	}

	public void Disconnect()
	{
		ServerManager.DisconnectClient(id);

		active = false;

		tcp.Disconnect();
		udp.Disconnect();
	}

	private class TCP
	{
		private readonly Client instance;
		private readonly TcpClient tcpClient;

		private NetworkStream stream;

		private byte[] buffer;

		public TCP(TcpClient _tcpClient, Client _instance)
		{
			instance = _instance;
			tcpClient = _tcpClient;

			buffer = new byte[512];

			stream = tcpClient.GetStream();
			
			ReadStreamAsync();
		}

		private async Task ReadStreamAsync()
		{
			try
			{
				int _readLength = await stream.ReadAsync(buffer, 0, buffer.Length, new (instance.active));

				if (!instance.active) return;

				if (_readLength <= 0)
				{
					instance.Disconnect();
					return;
				}

				StringBuilder sb = new(Encoding.ASCII.GetString(buffer));

				while (stream.DataAvailable)
				{
					_readLength = stream.Read(buffer, 0, buffer.Length);
					sb.Append(Encoding.ASCII.GetString(buffer, 0, _readLength));
				}

				// Handle Data
			}
			catch (Exception)
			{
				instance.Disconnect();
				return;
			}
		}

		public void Send(byte[] msg)
		{
			if (!instance.active) return;

			stream.WriteAsync(msg, 0, msg.Length);
		}

		public void Disconnect()
		{
			stream.Close();
			tcpClient.Close();
			tcpClient.Dispose();
		}
	}

	private class UDP
	{
		private readonly Client instance;
		private readonly UdpClient udpClient;

		private readonly IPEndPoint local;
		private IPEndPoint end;

		public UDP(Socket _client, Client _instance)
		{
			instance = _instance;
			end = _client.RemoteEndPoint as IPEndPoint;
			local = _client.LocalEndPoint as IPEndPoint;

			udpClient = new(local);

			uint IOC_IN = 0x80000000;
			uint IOC_VENDOR = 0x18000000;
			uint SIO_UDP_CONNRESET = IOC_IN | IOC_VENDOR | 12;
			udpClient.Client.IOControl((int)SIO_UDP_CONNRESET, [Convert.ToByte(false)], null);

			udpClient.Connect(end);

			udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

			udpClient.BeginReceive(ReceiveCallback, null);
		}

		private void ReceiveCallback(IAsyncResult result)
		{
			try
			{
				if (!instance.active) return;

				byte[] data = udpClient.EndReceive(result, ref end);
				udpClient.BeginReceive(ReceiveCallback, null);

				// Handle Data
			}
			catch (Exception e)
			{
				ServerManager.Print(e.ToString());
			}
		}

		public void Send(byte[] msg)
		{
			if (!instance.active) return;

			udpClient.BeginSend(msg, msg.Length, null, null);
		}

		public void Disconnect()
		{
			udpClient.Close();
			udpClient.Dispose();
		}
	}
}