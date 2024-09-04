using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

public class Client
{
	private TCP tcp;
	private int id;
	//private Player player;
	//private Account account;

	public Client()
	{
		tcp = new(this);
	}

	private class TCP
	{
		Client instance;
		TcpClient tcpClient;
		NetworkStream stream;

		IPEndPoint end = new(IPAddress.Parse("127.1.1.0"), 6666);

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
				await tcpClient.ConnectAsync(end);

				ClientManager.Print("Connected");

				buffer = new byte[4096];

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
}