using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

public class Client
{
	public readonly TCP tcp;
	public readonly UDP udp;
	public readonly int id;

	public int gameId;

	public bool active;
	public Player player;
	//private Account account;

	public Client(TcpClient _tcpClient, int _id)
	{
		active = true;

		id = _id;

		tcp = new(_tcpClient, this);
		udp = new(this);
	}

	public void Disconnect()
	{
		ServerManager.DisconnectClient(id);

		active = false;

		tcp.Disconnect();
	}

	public class TCP
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

			Send(new packets.Packet(instance.id).Serialize());
			
			ReadStreamAsync();
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

				PacketManager.CreatePacket(buffer).Run(instance.gameId);

				ReadStreamAsync();
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

	public class UDP
	{
		private readonly Client instance;

		public IPEndPoint end;

		public UDP(Client _instance)
		{
			instance = _instance;
		}

		public void Connect(IPEndPoint _end)
		{
			end = _end;
		}

		public void Send(byte[] msg)
		{
			if (!instance.active) return;

			ServerManager.SendUDP(msg, end);
		}
	}
}