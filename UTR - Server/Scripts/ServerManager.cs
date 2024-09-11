using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

public partial class ServerManager : Node
{
	private static TcpListener listener;

	private static int playerCount = 0;

	private static List<int> ids = new();
	private static Dictionary<int, Client> clients = new();

	private static List<TcpClient> tcpClientQueue = new();

	public override void _Ready()
	{
		PacketManager.CompileAll();

		PacketManager.CreatePacket(new packets.MovePacket(new Buffer(new byte[10])) { x = 5.5f, y = 3.75f, name = "Hello"}.Serialize());

		return;

		base._Ready();

		listener = new(IPAddress.Any, 6666);

		listener.Start();
		AcceptTcpClient();

		Print("Started");
	}

	private static async Task AcceptTcpClient()
	{
		TcpClient _tempTcpClient;

		try
		{
			_tempTcpClient = await listener.AcceptTcpClientAsync();
		}
		catch (Exception)
		{
			AcceptTcpClient();
			return;
		}

		tcpClientQueue.Add(_tempTcpClient);

		AcceptTcpClient();
	}

	public override void _Process(double delta)
	{
		if (tcpClientQueue.Count > 0)
		{
			ClientQueueManage();
		}
	}

	private static void ClientQueueManage()
	{
		while (tcpClientQueue.Count > 0)
		{
			TcpClient _curClient = tcpClientQueue[0];

			int _id = 0;

			while (ids.Contains(_id))
			{
				_id++;
			}

			Client _tempClient = new(_curClient, _id);
			clients.Add(_id, _tempClient);
			
			playerCount++;

			tcpClientQueue.RemoveAt(0);
		}
	}

	public static void DisconnectClient(int _id)
	{
		playerCount--;

		clients.Remove(_id);
		ids.Remove(_id);
	}

	public static void Print(string message)
	{
		GD.Print(message);
	}
}