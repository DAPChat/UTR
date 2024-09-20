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
		private List<Client> clients = new();
		private List<Packet> packetQueue = new();

		public List<Client> createQ = new();

		private bool readingQueue = false;
		private bool createQC = false;

		public void Instantiate(int _gameId, Client[] _clients)
		{
			gameId = _gameId;

			foreach (Client _c in _clients)
			{
				CreateClient(_c);
			}

			dun = new(10);
		}

		public void CreateClient(Client _c)
		{
			CharacterBody2D _tempPlayer = (CharacterBody2D)ResourceLoader.Load<PackedScene>("res://Scenes/player.tscn").Instantiate().Duplicate();
			GetNode<Node>("Players").AddChild(_tempPlayer);
			_tempPlayer.Position = new (50, 500);
			_c.player = _tempPlayer as Player;

			clients.Add(_c);

			SendAll(new MovePacket(_c.id, _tempPlayer.Position.X, _tempPlayer.Position.Y).Serialize());
		}

		public void Move(InputPacket _pa)
		{
			Player _p = ServerManager.GetClient(_pa.playerId).player;
			_p.Velocity = ((Vector2)_pa.inVect).Normalized() * (float)GetPhysicsProcessDeltaTime() * 10000;
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

			foreach (Client _c in clients)
			{
				Player _p = _c.player;
				try
				{
					_p.MoveAndSlide();
				}
				catch (Exception) { }
				
				SendAll(new MovePacket(_c.id, _p.Position.X, _p.Position.Y).Serialize());
			}
		}

		public void Destroy()
		{
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

		public void SendAll(byte[] _msg)
		{
			foreach (Client _client in clients)
			{
				_client.udp.Send(_msg);
			}
		}
	}
}