using System.Collections.Generic;
using Godot;
using packets;

namespace game
{
	public partial class Game : Node2D
	{
		private int gameId;

		private Dungeon dun;
		private List<Client> clients = new();
		private List<Packet> packetQueue = new();

		public void Instantiate(int _gameId, Client[] _clients)
		{
			gameId = _gameId;
			clients.AddRange(_clients);

			dun = new(10);
		}

		public void AddToQueue(Packet _packet)
		{
			packetQueue.Add(_packet);
		}

		public override void _Process(double delta)
		{
			base._Process(delta);

			GD.Print("Hello");
		}
	}
}