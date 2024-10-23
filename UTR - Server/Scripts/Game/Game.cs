using System;
using System.Collections.Generic;
using System.Linq;
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

			foreach (Client _c in _clients)
			{
				CreateClient(_c);
			}
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
		}

		public void Move(InputPacket _pa)
		{
			Player _p = ServerManager.GetClient(_pa.playerId).player;

			float _acceleration = -1;

			_p.Velocity = _p.Velocity.MoveToward(((Vector2)_pa.inVect).Normalized() * 100, 1500 * (float)GetPhysicsProcessDeltaTime());
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

			foreach (Client _c in clients.Values)
			{
				Player _p = _c.player;

				Vector2 _prev = _p.Position;
				try
				{
					_p.MoveAndSlide();
				}
				catch (Exception) { }
				
				SendAll(new MovePacket(_c.id, _p.Position.X, _p.Position.Y, _prev != _p.Position ? 1 : -1).Serialize());
			}
		}

		public void Destroy(int _cId)
		{
			clients.Remove(_cId);

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