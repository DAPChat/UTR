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

	public static UdpClient udpClient;

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

		udpClient = new(listener.LocalEndpoint as IPEndPoint);

		udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

		uint IOC_IN = 0x80000000;
		uint IOC_VENDOR = 0x18000000;
		uint SIO_UDP_CONNRESET = IOC_IN | IOC_VENDOR | 12;
		udpClient.Client.IOControl((int)SIO_UDP_CONNRESET, [Convert.ToByte(false)], null);

		listener.Start();
		AcceptTcpClient();

		udpClient.BeginReceive(ReceiveCallback, null);

		Print("Started");
	}

	private static void ReceiveCallback(IAsyncResult result)
	{
		try
		{
			IPEndPoint _cliendEnd = new IPEndPoint(IPAddress.Any, 0);
			byte[] data = udpClient.EndReceive(result, ref _cliendEnd);
			udpClient.BeginReceive(ReceiveCallback, null);

			packets.Packet _p = PacketManager.CreatePacket(data);
			AddPacket(_p, clients[_p.playerId].GetGameId());
		}
		catch (Exception e)
		{
			Print(e.ToString());
		}
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

		if (tcpClientQueue.Count > 0 && !checkingQueue)
		{
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
			Node curScene = tree.GetTree().Root;

			Window win = new();

			curScene.AddChild(win);
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

	public static Game GetGame(int _gId)
	{
		return games[_gId];
	}

	public static void DisconnectClient(int _id)
	{
		playerCount--;

		clients.Remove(_id);
		ids.Remove(_id);
	}

	public static void SendUDP(byte[] _msg, IPEndPoint _refEnd)
	{
		udpClient.BeginSend(_msg, _msg.Length, _refEnd, null, null);
	}

	public static void Print(string message)
	{
		GD.Print(message);
	}
}