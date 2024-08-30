using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Godot;

public partial class ServerManager : Node
{
	private static TcpListener listener;

	private static int playerCount = 0;

	private static List<int> ids = new();
	private static Dictionary<int, Client> clients = new();

	private static List<TcpClient> tpcClientQueue = new();
}
