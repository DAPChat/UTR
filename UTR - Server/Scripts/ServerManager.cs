using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;

public partial class ServerManager : Node
{
	private static TcpListener listener;

	private static int playerCount = 0;

	private static List<int> ids = new();
	private static Dictionary<int, Client> clients = new();

	private static List<TcpClient> tcpClientQueue = new();

	public override void _Ready()
	{
		base._Ready();

		listener = new(IPAddress.Any, 6666);

		listener.Start();
		listener.BeginAcceptTcpClient(AcceptClientCallback, null);

		Print("Started");
	}

	private static void AcceptClientCallback(IAsyncResult _result)
	{
		TcpClient _tempTcpClient;

		try
		{
			_tempTcpClient = listener.EndAcceptTcpClient(_result);
		}
		catch (Exception)
		{
			listener.BeginAcceptTcpClient(AcceptClientCallback, null);
			return;
		}

		tcpClientQueue.Add(_tempTcpClient);

		listener.BeginAcceptTcpClient(AcceptClientCallback, null);
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

	public static void Print(string message)
	{
		GD.Print(message);
	}
}