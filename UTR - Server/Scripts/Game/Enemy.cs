using Godot;
using System.Collections.Generic;
using System.Linq;

namespace enemy
{
	public partial class Enemy : CharacterBody2D
	{
		int gId;

		public int order;

		List<int> collidingPlayers = new();
		List<int> attackPlayers = new();

		public int health = 100;
		public int enemyId;
		public int roomId;
		public int damage = 5;
		public int speed = 1000;

		public int trackingId = -1;
		public int attackingId = -1;

		public bool active = false;

		bool attackReady = false;
		bool stateReady = true;
		Timer cooldown;
		Timer stateCd;

		public void Instantiate(int _gId, int _id, int _rId)
		{
			gId = _gId;
			enemyId = _id;
			roomId = _rId;
			order = 0;

			cooldown = GetNode<Timer>("Cooldown");
			stateCd = GetNode<Timer>("StateCooldown");

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
				if (body.GetParent().GetType() == typeof(Player))
				{
					Player _p = (Player)body.GetParent();

					if (attackingId == -1)
					{
						attackingId = _p.cId;
						trackingId = _p.cId;

						cooldown.Start(0.2);
						attackReady = false;
					}

					attackPlayers.Add(_p.cId);
				}
			};

			GetNode<Area2D>("AttackArea").AreaExited += (body) =>
			{
				if (body.GetParent().GetType() == typeof(Player))
				{
					Player _p = (Player)body.GetParent();

					attackPlayers.Remove(_p.cId);

					if (attackingId == _p.cId)
						if (attackPlayers.Count != 0)
						{
							attackingId = attackPlayers.FirstOrDefault();
							trackingId = attackingId;
						}
						else
						{
							attackingId = -1;
							cooldown.Stop();
							attackReady = false;
						}
				}
			};

			cooldown.Timeout += () =>
			{
				attackReady = true;
			};

			stateCd.Timeout += () =>
			{
				speed = 1000;
				stateCd.Start();
				stateReady = true;
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
				TrackPlayer(delta);
			}

			if (attackingId != -1 && ServerManager.GetClient(attackingId) != null)
			{
				AttackPlayer(delta);
			}
		}

		private void AttackPlayer(double delta)
		{
			if (ServerManager.GetClient(attackingId).player.curRoom != roomId)
			{
				if (attackPlayers.Count == 1) return;

				attackPlayers.Remove(attackingId);
				attackPlayers.Add(attackingId);

				attackingId = attackPlayers.FirstOrDefault();
				trackingId = attackingId;
				return;
			}

			if (!attackReady) return;

			ServerManager.GetClient(attackingId).player.Damage(this, damage);

			attackReady = false;

			cooldown.Start(0.15);
		}

		private void TrackPlayer(double delta)
		{
			RandomNumberGenerator rand = new();

			if (ServerManager.GetClient(trackingId).player.curRoom != roomId)
			{
				if (collidingPlayers.Count == 1) return;

				collidingPlayers.Remove(trackingId);
				collidingPlayers.Add(trackingId);
				trackingId = collidingPlayers.FirstOrDefault();
				return;
			}

			Vector2 pos = (ServerManager.GetClient(trackingId).player.Position - Position).Normalized();

			if (stateReady && rand.RandiRange(1,5) == 1)
			{
				speed = 4500;
				stateReady = false;
			}
			else
			{
				stateReady = false;
			}

			Velocity = pos * (float)delta * speed;
			MoveAndSlide();
			//MoveAndCollide(pos * (float)delta * 10);

			ServerManager.GetGame(gId).SendAll(new packets.EnemyPacket(enemyId, this).Serialize());
		}

		public void Damage(int amt)
		{
			if (!active) return;
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