using Godot;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class Client
{
	private readonly TCP tcp;

	private UDP udp;
	public bool active;

	public readonly int id;
	//private Player player;
	//private Account account;

	public IPEndPoint end = new(IPAddress.Parse("127.1.1.0"), 6666);

	public Client()
	{
		tcp = new(this);
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

				instance.udp = new(tcpClient.Client, instance);

				ClientManager.Print("Connected");

				buffer = new byte[512];

				instance.active = true;

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

			local.Address = local.Address.MapToIPv4();
			end.Address = end.Address.MapToIPv4();

			try
			{
				udpClient = new(local);

				udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

				uint IOC_IN = 0x80000000;
				uint IOC_VENDOR = 0x18000000;
				uint SIO_UDP_CONNRESET = IOC_IN | IOC_VENDOR | 12;
				udpClient.Client.IOControl((int)SIO_UDP_CONNRESET, [Convert.ToByte(false)], null);

				udpClient.Connect(end);
			}
			catch (Exception e)
			{
				ClientManager.Print(e.ToString());
				return;
			}

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
			}catch (Exception e)
			{
				ClientManager.Print(e.ToString());
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