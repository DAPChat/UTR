using Godot;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

public partial class ServerManager : Node
{
	private static TcpListener listener;

	private static int playerCount = 0;

	private static List<int> ids = new();
	private static List<int> gameIds = new();
	private static Dictionary<int, Client> clients = new();
	private static Dictionary<int, Game> games = new();

	private static List<TcpClient> tcpClientQueue = new();

	public override void _Ready()
	{
		base._Ready();

		PacketManager.CompileAll();

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
				_id++;

			Client _tempClient = new(_curClient, _id);
			clients.Add(_id, _tempClient);
			ids.Add(_id);

			int _gameId = 0;

			while (gameIds.Contains(_gameId))
				_gameId++;

			Game _tempGame = new(_gameId, [clients[_id]]);
			games.Add(_gameId, _tempGame);
			gameIds.Add(_gameId);

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