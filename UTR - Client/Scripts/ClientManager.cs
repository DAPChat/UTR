using Godot;
using System;

public partial class ClientManager : Node
{
	Client client;

	public override void _Ready()
	{
		base._Ready();

		client = new();
	}

	public static void Print(string message)
	{
		GD.Print(message);
	}
}
