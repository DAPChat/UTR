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

		public CharacterBody2D player;

		private bool readingQueue = false;

		public void Instantiate(int _gameId, Client[] _clients)
		{
			gameId = _gameId;
			clients.AddRange(_clients);

			player = GetNode<CharacterBody2D>("Player");

			dun = new(10);
		}

		public void AddToQueue(Packet _packet)
		{
			packetQueue.Add(_packet);
		}

		public override void _PhysicsProcess(double delta)
		{
			base._PhysicsProcess(delta);

			if (packetQueue.Count > 0 && !readingQueue)
			{
				readingQueue = true;
				ReadQueue();
			}
		}

		private void ReadQueue()
		{
			while (packetQueue.Count > 0)
			{
				packetQueue[0].Run(this);
			}
		}
	}
}