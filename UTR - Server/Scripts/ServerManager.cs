using Godot;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

using game;

public partial class ServerManager : Node
{
	private static TcpListener listener;

	private static int playerCount = 0;

	private static List<int> ids = new();
	private static List<int> gameIds = new();
	private static Dictionary<int, Client> clients = new();
	private static Dictionary<int, Game> games = new();

	private static List<TcpClient> tcpClientQueue = new();

	private static ServerManager tree;

	private static bool checkingQueue = false;

	public override void _Ready()
	{
		base._Ready();

		tree = this;

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
			Print("Failure");
			AcceptTcpClient();
			return;
		}

		tcpClientQueue.Add(_tempTcpClient);

		AcceptTcpClient();
	}

	public override void _Process(double delta)
	{
		base._Process(delta);
		if (checkingQueue) Print("Processes");
		if (tcpClientQueue.Count > 0 && !checkingQueue)
		{
			Print("Processing");
			checkingQueue = true;
			ClientQueueManage();
		}
	}

	private static void ClientQueueManage()
	{
		while (tcpClientQueue.Count != 0)
		{
			TcpClient _curClient = tcpClientQueue[0];

			int _id = 0;

			while (ids.Contains(_id))
				_id++;

			Client _tempClient = new(_curClient, _id);
			clients.Add(_id, _tempClient);
			ids.Add(_id);

			int _gameId = 0;

			tcpClientQueue.Remove(tcpClientQueue[0]);

			while (gameIds.Contains(_gameId))
				_gameId++;

			Node _tempGameScene = ResourceLoader.Load<PackedScene>("res://Scenes/GameRoom.tscn").Instantiate().Duplicate();
			Node curScene = tree.GetNode("/root/MainScene/Games");//tree.GetTree().Root;

			Window win = new();

			curScene.AddChild(win.Duplicate());
			win.Name = _gameId.ToString();
			win.Show();
			Game _tempGame = _tempGameScene as Game;

			_tempGame.Instantiate(_gameId, [_tempClient]);

			win.AddChild(_tempGameScene);

			games.Add(_gameId, _tempGame);
			gameIds.Add(_gameId);

			_tempClient.SetGame(_gameId);

			playerCount++;
		}
		checkingQueue = false;
	}

	public static void AddPacket(packets.Packet _packet, int _gId)
	{
		games[_gId].AddToQueue(_packet);
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