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

		public void Move(InputPacket _pa)
		{
			Player _p = ServerManager.GetClient(_pa.playerId).player;
			_p.Velocity = ((Vector2)_pa.inVect).Normalized() * 100;
			_p.MoveAndSlide();

			SendAll(new MovePacket(_pa.playerId, _p.Position.X, _p.Position.Y).Serialize());
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

		public void Destroy()
		{
			ServerManager.RemoveGame(gameId);
			GetParent().QueueFree();
		}

		private void ReadQueue()
		{
			while (packetQueue.Count > 0)
			{
				if (packetQueue[0] == null)
				{
					packetQueue.RemoveAt(0);
					continue;
				}
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