using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

public class Client
{
	private TCP tcp;
	private UDP udp;
	private int id;
	//private Player player;
	//private Account account;

	public Client(TcpClient _tcpClient, int _id)
	{
		tcp = new(_tcpClient, this);
		udp = new(_tcpClient.Client, this);
		id = _id;
	}

	private class TCP
	{
		Client instance;
		TcpClient tcpClient;
		NetworkStream stream;

		byte[] buffer;

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
				int _readLength = await stream.ReadAsync(buffer, 0, buffer.Length);

				if (_readLength <= 0)
				{
					// Disconnect
					return;
				}

				StringBuilder sb = new(Encoding.ASCII.GetString(buffer));

				while (stream.DataAvailable)
				{
					_readLength = stream.Read(buffer, 0, buffer.Length);
					sb.Append(Encoding.ASCII.GetString(buffer, 0, _readLength));
				}
			}
			catch (Exception)
			{
				// Disconnect
				return;
			}
		}
	}

	private class UDP
	{
		Client instance;
		UdpClient udpClient;
		IPEndPoint end;
		IPEndPoint local;

		public UDP(Socket _client, Client _instance)
		{
			instance = _instance;
			end = /*new(IPAddress.Parse("127.1.1.0"), 6664);*/_client.RemoteEndPoint as IPEndPoint;
			local = _client.LocalEndPoint as IPEndPoint;

			ServerManager.Print(local.ToString() + ": " + end.ToString());

			udpClient = new(local);
			udpClient.Connect(end);

			udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

			udpClient.BeginReceive(ReceiveCallback, null);
		}

		private void ReceiveCallback(IAsyncResult result)
		{
			byte[] data = udpClient.EndReceive(result, ref end);
			udpClient.BeginReceive(ReceiveCallback, null);

			ServerManager.Print(Encoding.ASCII.GetString(data));
		}
	}
}