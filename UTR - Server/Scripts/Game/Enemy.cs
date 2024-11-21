using Godot;
using System.Collections.Generic;
using System.Linq;

namespace enemy
{
	public partial class Enemy : CharacterBody2D
	{
		int gId;

		List<int> collidingPlayers = new();

		public int health = 100;
		public int enemyId;
		public int damage = 10;

		public int trackingId = -1;

		public bool active = false;

		public void Instantiate(int _gId, int _id)
		{
			gId = _gId;
			enemyId = _id;

			GetNode<Area2D>("TrackerArea").AreaEntered += (body) =>
			{
				if (body.GetParent().GetType() == typeof(Player))
				{
					Player _p = (Player)body.GetParent();

					if (trackingId == -1)
					{
						trackingId = _p.cId;
					}

					collidingPlayers.Add(_p.cId);
				}
			};

			GetNode<Area2D>("TrackerArea").AreaExited += (body) =>
			{
				if (body.GetParent().GetType() == typeof(Player))
				{
					Player _p = (Player)body.GetParent();
					
					collidingPlayers.Remove(_p.cId);

					if (trackingId == _p.cId)
						if (collidingPlayers.Count != 0) trackingId = collidingPlayers.FirstOrDefault();
						else trackingId = -1;
				}
			};

			GetNode<Area2D>("AttackArea").AreaEntered += (body) =>
			{
				//if (body.GetParent().GetType() == typeof (Player))
					//GD.Print("Hello");
			};

			ServerManager.GetGame(gId).SendAll(new packets.EnemyPacket(enemyId, this).Serialize());

			active = true;
		}

		public override void _PhysicsProcess(double delta)
		{
			base._PhysicsProcess(delta);

			if (!active) return;

			if (trackingId != -1 && ServerManager.GetClient(trackingId) != null)
			{
				Vector2 pos = (ServerManager.GetClient(trackingId).player.Position - Position).Normalized();

				Velocity = pos * (float)delta * 750;
				MoveAndSlide();
				//MoveAndCollide(pos * (float)delta * 10);

				ServerManager.GetGame(gId).SendAll(new packets.EnemyPacket(enemyId, this).Serialize());
			}
		}

		public void Damage(int amt)
		{
			health -= amt;
			if (health <= 0)
			{
				active = false;
				ServerManager.GetGame(gId).SendAll(new packets.EnemyPacket(enemyId, this, 0).Serialize());
				QueueFree();
			}
		}
	}
}