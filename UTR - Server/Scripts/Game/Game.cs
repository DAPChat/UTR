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

		public List<int> exploredRooms = new();

		private bool readingQueue = false;
		private bool createQC = false;

		public List<int> enemyIds = new();

		public void Instantiate(int _gameId, Client[] _clients)
		{
			gameId = _gameId;
			dun = new(10, 10, 10, GetNode<TileMapLayer>("DungeonT"), gameId);
			exploredRooms.Add(dun.startRoom.playerId);

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

			while (dun.startRoom == null) { }

			_tempPlayer.Position = new (dun.startRoom.x*256+32, dun.startRoom.y*256+32);
			_c.player = _tempPlayer as Player;
			_c.player.Instantiate(_c.id, gameId);

			SendAll(new MovePacket(_c.id, _c.player.outOrder, _tempPlayer.Position.X, _tempPlayer.Position.Y, 1).Serialize());
			
			SendTo(_c.id, dun.startRoom.Serialize());
			
			foreach (int i in exploredRooms)
			{
				SendTo(_c.id, dun.rooms[i].Serialize());
			}

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

		public void ChangeRoom(int _cId, RoomPacket _rp)
		{
			clients[_cId].player.curRoom = _rp.playerId;

			foreach(Enemy e in GetNode("Enemies").GetChildren())
			{
				if (!GetActiveRooms().Contains(e.roomId))
					e.ProcessMode = (ProcessModeEnum)4;
				else e.ProcessMode = 0;
			}

			if (exploredRooms.Contains(_rp.playerId))
			{
				return;
			}

			PrepareRoom(_rp);

			SendAll(_rp.Serialize());
			exploredRooms.Add(_rp.playerId);
		}

		public void PrepareRoom(RoomPacket _rp)
		{
			if (!(_rp.h == 1 && _rp.w == 1)) return;

			for (int i = 0; i < 5; i++)
			{
				Enemy enemy = (Enemy)ResourceLoader.Load<PackedScene>("res://Scenes/enemy.tscn").Instantiate<Enemy>().Duplicate();
				enemy.Position = new Vector2(GD.RandRange(_rp.x * 16 * 16 + 32, _rp.x * 16 * 16 + 16 * 15), GD.RandRange(_rp.y * 16 * 16 + 32, _rp.y * 16 * 16 + 16 * 15));

				int id = 0;

				while (enemyIds.Contains(id)) id++;

				enemyIds.Add(id);

				enemy.Instantiate(gameId, id, _rp.playerId);

				GetNode<Node>("Enemies").CallDeferred(Node.MethodName.AddChild, enemy);
			}
		}

		public void Destroy(int _cId)
		{
			clients[_cId].player.Exit();
			clients.Remove(_cId);

			if (clients.Count != 0) return;

			ServerManager.RemoveGame(gameId);
			GetParent().QueueFree();
		}

		public List<int> GetActiveRooms()
		{
			List<int> aR = new();

			foreach (var room in exploredRooms)
			{
				if (GetPlayersInRoom(room) > 0) aR.Add(room);
			}

			return aR;
		}

		public int GetPlayersInRoom(int _rId)
		{
			int count = 0;

			foreach (var player in clients)
			{
				if (player.Value.player.curRoom == _rId) count++;
			}

			return count;
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