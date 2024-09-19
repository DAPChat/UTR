using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

public class Client
{
	private readonly TCP tcp;
	private readonly UDP udp;
	public readonly int id;

	private int gameId;

	public bool active;
	private Player player;
	//private Account account;

	public Client(TcpClient _tcpClient, int _id)
	{
		active = true;

		tcp = new(_tcpClient, this);
		udp = new(_tcpClient.Client, this);
		id = _id;
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
	}

	public void SetGame(int _gId)
	{
		gameId = _gId;
	}

	public void SetPlayer(Player _player)
	{
		player = _player;
	}

	public int GetGameId() { return gameId; }

	public Player GetPlayer() { return player; }

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

		private IPEndPoint end;

		public UDP(Socket _client, Client _instance)
		{
			instance = _instance;
			end = _client.RemoteEndPoint as IPEndPoint;

			Send(new packets.Packet(instance.id).Serialize());
		}

		public void Send(byte[] msg)
		{
			if (!instance.active) return;

			ServerManager.SendUDP(msg, end);
		}
	}
}