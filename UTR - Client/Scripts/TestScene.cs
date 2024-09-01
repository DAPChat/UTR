using Godot;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

public partial class TestScene : Node2D
{
	IPEndPoint end = new(IPAddress.Parse("127.1.1.0"), 6666);

	TcpClient client = new();

	public override void _Ready()
	{
		base._Ready();

		client.BeginConnect(end.Address, end.Port, Connected, null);
	}

	void Connected (IAsyncResult res)
	{
		client.GetStream().Write(Encoding.ASCII.GetBytes("This is longer than 5 bytes"));
		client.GetStream().Write(Encoding.ASCII.GetBytes("This is longer than 10 bytes"));
		client.GetStream().Write(Encoding.ASCII.GetBytes("This is longer than 15 bytes"));
	}
}
