using Godot;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

using game;
using items;
using System.Linq;
using packets;

public partial class ServerManager : Node
{
	private static TcpListener listener;

	public static UdpClient udpClient;

	private static int playerCount = 0;

	private static Dictionary<int, Client> clients = new();
	private static Dictionary<int, Game> games = new();

	private static List<Packet> gameQueue = new();

	private static ServerManager tree;

	//private static bool checkingQueue = false;
	private static bool checkingGame = false;

	public override void _Ready()
	{
		base._Ready();

		tree = this;

		PacketManager.CompileAll();

		listener = new(IPAddress.Any, 6666);

		udpClient = new(6666);
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
			IPEndPoint _clientEnd = new(IPAddress.Any, 0);
			byte[] data = udpClient.EndReceive(result, ref _clientEnd);
			udpClient.BeginReceive(ReceiveCallback, null);

			Packet _p = PacketManager.CreatePacket(data);

			if (clients[_p.playerId].udp.end == null)
			{
				clients[_p.playerId].udp.Connect(_clientEnd);
				gameQueue.Add(_p);
				return;
			}

			Game _tempG = GetGame(clients[_p.playerId].gameId);
			_tempG?.AddToQueue(_p);
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
			AddClient(_tempTcpClient);
		}
		catch (Exception)
		{
			Print("Failure");
		}
		AcceptTcpClient();
	}

	public override void _Process(double delta)
	{
		base._Process(delta);

		if (!checkingGame && gameQueue.Count > 0) ReadGameQueue();
	}

	private static void ReadGameQueue()
	{
		while (gameQueue.Count > 0)
		{
			if (gameQueue[0] == null) continue;
			AddToGame(gameQueue[0]);
			gameQueue.RemoveAt(0);
		}
	}

	private static void AddClient(TcpClient _client)
	{
		int _id = 0;

		while (clients.ContainsKey(_id))
			_id++;

		Client _tempClient = new(_client, _id);
		clients.Add(_id, _tempClient);

		playerCount++;
	}

	public static Client GetClient(int _cId)
	{
		if (!clients.ContainsKey(_cId)) return null;

		return clients[_cId];
	}

	private static void AddToGame(Packet _packet)
	{
		int _pId = _packet.playerId;

		if (_packet.data != -1 && games.ContainsKey(_packet.data))
		{
			games[_packet.data].createQ.Add(GetClient(_pId));
			GetClient(_pId).gameId = _packet.data;
			return;
		}

		int _gameId = 0;
		Client _tempClient = clients[_pId];

		while (games.Keys.Contains(_gameId))
			_gameId++;

		Node _tempGameScene = ResourceLoader.Load<PackedScene>("res://Scenes/GameRoom.tscn").Instantiate().Duplicate();
		Node curScene = tree.GetTree().Root;

		Window win = new();

		curScene.CallDeferred(Node.MethodName.AddChild, win);
		//win.SetDeferred(win.Name, _gameId.ToString());
		win.Show();
		Game _tempGame = _tempGameScene as Game;

		win.CallDeferred(Node.MethodName.AddChild, _tempGameScene);

		games.Add(_gameId, _tempGame);

		_tempClient.gameId = _gameId;

		_tempGame.Instantiate(_gameId, [_tempClient]);
	}

	public static void AddPacket(packets.Packet _packet, int _gId)
	{
		games[_gId].AddToQueue(_packet);
	}

	public static Game GetGame(int _gId)
	{
		if (!games.ContainsKey(_gId)) return null;

		return games[_gId];
	}

	public static void DisconnectClient(int _id)
	{
		playerCount--;

		games[clients[_id].gameId].Destroy(_id);
		clients.Remove(_id);
	}

	public static void RemoveGame(int _gId)
	{
		games.Remove(_gId);
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