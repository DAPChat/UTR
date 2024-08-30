using Godot;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

public partial class ServerManager : Node
{
	private static TcpListener listener;

	private static int playerCount = 0;

	private static List<int> ids = new();
	private static Dictionary<int, Client> clients = new();

	private static List<TcpClient> tpcClientQueue = new();

	public override void _Ready()
	{
		base._Ready();

		listener = new(IPAddress.Any, 6666);

		listener.Start();
		listener.BeginAcceptTcpClient(AcceptClientCallback, this);
	}

	private static void AcceptClientCallback(IAsyncResult result)
	{

	}
}