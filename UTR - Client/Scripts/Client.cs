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

	public IPEndPoint end = new(IPAddress.Parse("127.1.1.0"), 6666);

	public Client()
	{
		tcp = new(this);
	}

	private class TCP
	{
		Client instance;
		TcpClient tcpClient;
		NetworkStream stream;

		byte[] buffer;

		public TCP(Client _instance)
		{
			instance = _instance;
			
			tcpClient = new();

			Connect();
		}

		private async Task Connect()
		{
			try
			{
				await tcpClient.ConnectAsync(instance.end);

				instance.udp = new(instance);

				ClientManager.Print("Connected");

				buffer = new byte[512];

				stream = tcpClient.GetStream();

				ReadStreamAsync();
			}
			catch (Exception)
			{
				Connect();
			}
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

		public UDP(Client _instance)
		{
			instance = _instance;

			udpClient = new();
			udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

			udpClient.Connect(instance.end);

			ClientManager.Print(instance.end.ToString());

			udpClient.SendAsync(Encoding.ASCII.GetBytes("Hello There"), "Hello There".Length);

			Read();
		}

		private async Task Read()
		{
			try
			{
				byte[] buffer = (await udpClient.ReceiveAsync()).Buffer;

				ClientManager.Print("There");

				ClientManager.Print(Encoding.ASCII.GetString(buffer));
			}
			catch (Exception)
			{
				// Disconnect

				return;
			}

			Read();
		}
	}
}