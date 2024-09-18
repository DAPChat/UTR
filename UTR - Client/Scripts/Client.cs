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
	public int id;

	//private Player player;
	//private Account account;

	public IPEndPoint end = new(IPAddress.Parse("127.1.1.0"), 6666);

	public Client()
	{
		tcp = new(this);
		udp = new(this);
	}

	public void SendTCP(byte[] msg)
	{
		if (!active) return;
		tcp.Send(msg);
	}

	public void SendUDP(byte[] msg)
	{
		if (!active) return;
		udp.Send(msg);
	}

	public void Disconnect()
	{
		active = false;
		ClientManager.active = false;

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
				instance.active = true;
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

				PacketManager.CreatePacket(buffer);
			}
			catch (Exception e)
			{
				ClientManager.Print(e.ToString());
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

		private IPEndPoint end;

		public UDP(Client _instance)
		{
			instance = _instance;

			end = instance.end;

			end.Address = end.Address.MapToIPv4();

			try
			{
				udpClient = new();

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

				ClientManager.Print("That");

				PacketManager.CreatePacket(data).Run();
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