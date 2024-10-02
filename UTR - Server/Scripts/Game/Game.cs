using System.Collections.Generic;
using Godot;
using packets;

namespace game
{
	public partial class Game : Node2D
	{
		private int gameId;

<<<<<<< Updated upstream
		private Dungeon dun;
		private List<Client> clients = new();
=======
		private Dungen dun;
		private Dictionary<int, Client> clients = new();
>>>>>>> Stashed changes
		private List<Packet> packetQueue = new();

		private bool readingQueue = false;

		public void Instantiate(int _gameId, Client[] _clients)
		{
			gameId = _gameId;
			clients.AddRange(_clients);

<<<<<<< Updated upstream
			foreach (Client _c in clients)
=======
			dun = GetNode<Node>("Map") as Dungen;
			dun.CreateDungeon();

			foreach (Client _c in _clients)
>>>>>>> Stashed changes
			{
				CreateClient(_c);
			}
		}

		private void CreateClient(Client _c)
		{
			CharacterBody2D _tempPlayer = (CharacterBody2D)ResourceLoader.Load<PackedScene>("res://Scenes/player.tscn").Instantiate().Duplicate();
			GetNode<Node>("Players").AddChild(_tempPlayer);
<<<<<<< Updated upstream
			_tempPlayer.Position = new(50, 500);
=======
			_tempPlayer.Position = dun.bsp.rooms[^1].panel.Position + dun.bsp.rooms[^1].panel.Size/2;
>>>>>>> Stashed changes
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
				packetQueue[0].Run(gameId);
				packetQueue.RemoveAt(0);
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