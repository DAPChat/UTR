using System;
using System.Collections.Generic;
using System.Linq;
using enemy;
using Godot;
using packets;

namespace game
{
	public partial class Game : Node2D
	{
		private int gameId;

		private Dungeon dun;
		private Dictionary<int, Client> clients = new();
		private List<Packet> packetQueue = new();

		public List<Client> createQ = new();

		private bool readingQueue = false;
		private bool createQC = false;

		public void Instantiate(int _gameId, Client[] _clients)
		{
			gameId = _gameId;
			dun = new(10, GetNode<TileMapLayer>("DungeonT"));

			for (int i = 0; i < 5; i++)
			{
				Enemy enemy = (Enemy)ResourceLoader.Load<PackedScene>("res://Scenes/enemy.tscn").Instantiate<Enemy>().Duplicate();
				enemy.Position = new Vector2(GD.RandRange(20,10*15), GD.RandRange(20, 10*15));

				enemy.enemyId = i;

				enemy.Instantiate(gameId);

				GetNode<Node>("Enemies").AddChild(enemy);
			}

			foreach (Client _c in _clients)
			{
				CreateClient(_c);
			}
		}

		public void Move(MovePacket _pa)
		{
			clients[_pa.playerId].player.Move(_pa);
		}

		public void CreateClient(Client _c)
		{
			clients.Add(_c.id, _c);
			CharacterBody2D _tempPlayer = (CharacterBody2D)ResourceLoader.Load<PackedScene>("res://Scenes/player.tscn").Instantiate().Duplicate();
			GetNode<Node>("Players").AddChild(_tempPlayer);
			_tempPlayer.Position = new (32, 32);
			_c.player = _tempPlayer as Player;
			_c.player.Instantiate(_c.id, gameId);

			SendAll(new MovePacket(_c.id, _tempPlayer.Position.X, _tempPlayer.Position.Y, 1).Serialize());
			SendTo(_c.id, dun.rooms[0].Serialize());

			foreach (Enemy enemy in GetNode<Node>("Enemies").GetChildren())
			{
				SendTo(_c.id, new EnemyPacket(_c.id, enemy).Serialize());
			}

			foreach (Client c in clients.Values)
			{
				SendTo(_c.id, new SlotPacket(c.id, c.player.hotbar[c.player.activeSlot].item, c.player.activeSlot, c.player.hotbar[c.player.activeSlot].count, 2).Serialize());
			}
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

			if (createQ.Count > 0 && !createQC)
			{
				createQC = true;
				CreateQueue();
			}
		}

		public void Destroy(int _cId)
		{
			clients[_cId].player.QueueFree();
			clients.Remove(_cId);

			SendAll(new Packet(_cId, -2).Serialize());

			if (clients.Count != 0) return;

			ServerManager.RemoveGame(gameId);
			GetParent().QueueFree();
		}

		private void CreateQueue()
		{
			while (createQ.Count != 0)
			{
				if (createQ[0] != null)
				{
					CreateClient(createQ[0]);
					createQ.RemoveAt(0);
				}
				else createQ.RemoveAt(0);
			}

			createQC = false;
		}

		private void ReadQueue()
		{
			while (packetQueue.Count > 0)
			{
				if (packetQueue[0] != null)
				{
					packetQueue[0].Run(gameId);
					packetQueue.RemoveAt(0);
				}
				else packetQueue.RemoveAt(0);
			}
			readingQueue = false;
		}

		public void SendTo(int _cId, byte[] _msg)
		{
			clients[_cId].udp.Send(_msg);
		}

		public void SendAll(byte[] _msg)
		{
			foreach (Client _client in clients.Values)
			{
				_client.udp.Send(_msg);
			}
		}
	}
}