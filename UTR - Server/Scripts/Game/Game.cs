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

		private bool readingQueue = false;

		public void Instantiate(int _gameId, Client[] _clients)
		{
			gameId = _gameId;
			clients.AddRange(_clients);

			foreach (Client _c in clients)
			{
				CreateClient(_c);
			}

			dun = new(10);
		}

		private void CreateClient(Client _c)
		{
			CharacterBody2D _tempPlayer = (CharacterBody2D)ResourceLoader.Load<PackedScene>("res://Scenes/player.tscn").Instantiate().Duplicate();
			GetNode<Node>("Players").AddChild(_tempPlayer);
			_tempPlayer.Position = new(50, 500);
			_c.player = _tempPlayer as Player;

			SendAll(new MovePacket(_c.id, _tempPlayer.Position.X, _tempPlayer.Position.Y).Serialize());
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

			}
			readingQueue = false;
		}

		public void SendAll(byte[] _msg)
		{
			foreach (Client _client in clients)
			{
				_client.udp.Send(_msg);
			}
		}
	}
}