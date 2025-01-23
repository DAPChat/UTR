using Godot;
using items;
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

		public bool prevTrack = false;

		public bool active = false;

		Vector2 knockedDir;

		bool attackReady = false;
		bool stateReady = true;
		bool knocked = false;
		Timer cooldown;
		Timer stateCd;
		Timer knockbackTimer;

		public void Instantiate(int _gId, int _id, int _rId)
		{
			gId = _gId;
			enemyId = _id;
			roomId = _rId;
			order = 0;

			cooldown = GetNode<Timer>("Cooldown");
			stateCd = GetNode<Timer>("StateCooldown");
			knockbackTimer = GetNode<Timer>("Knockback");

			ServerManager.GetGame(gId).SendAll(new packets.EnemyPacket(enemyId, this).Serialize());

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

						cooldown.Start(0.35);
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

			knockbackTimer.Timeout += () =>
			{
				knocked = false;
			};

			active = true;
		}

		public override void _PhysicsProcess(double delta)
		{
			base._PhysicsProcess(delta);

			if (!active) return;

			if (ServerManager.GetGame(gId) == null) return;

			if (trackingId != -1 && ServerManager.GetClient(trackingId) != null)
			{
				TrackPlayer(delta);
				if (!prevTrack)
					ServerManager.GetGame(gId).SendAll(new packets.StatePacket(enemyId, 1, 1).Serialize());
			}
			else if (prevTrack)
			{
				ServerManager.GetGame(gId).SendAll(new packets.StatePacket(enemyId, 1, 0).Serialize());
			}

			prevTrack = trackingId != -1;

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

			cooldown.Start(0.4);
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
				speed = 5500;
				stateReady = false;
			}
			else
			{
				stateReady = false;
			}

			Velocity = pos * (float)delta * speed;

			if (knocked) Velocity += (new Vector2(250, 250) * knockedDir);

			MoveAndSlide();
			//MoveAndCollide(pos * (float)delta * 10);

			ServerManager.GetGame(gId).SendAll(new packets.EnemyPacket(enemyId, this).Serialize());
		}

		public void Pause(bool _p)
		{
			if (_p)
			{
				ServerManager.GetGame(gId).SendAll(new packets.StatePacket(enemyId, 1, 0).Serialize());
				ProcessMode = (ProcessModeEnum)4;
			}
			else ProcessMode = 0;
		}

		public void Damage(int amt, Vector2 dir, int atkId)
		{
			if (!active) return;
			health -= amt;

			knocked = true;
			knockbackTimer.Start();
			knockedDir = (GlobalPosition - dir).Normalized();

			cooldown.Start();

			if (health <= 0)
			{
				active = false;
				ServerManager.GetClient(atkId).player.EnemyKill();
				ServerManager.GetGame(gId).SendAll(new packets.EnemyPacket(enemyId, this, 0).Serialize());

				ItemDrop drop = ResourceLoader.Load<PackedScene>("res://Scenes/item_drop.tscn").Instantiate<ItemDrop>();
				drop.Instantiate(items.ItemManager.GetItem(1));
				drop.Position = Position;


				if (new RandomNumberGenerator().RandiRange(0, 20) == 20)
				{
					drop = ResourceLoader.Load<PackedScene>("res://Scenes/item_drop.tscn").Instantiate<ItemDrop>();
					drop.Instantiate(ItemManager.GetItem(0));
					drop.Position = Position;

					drop.item.instanceAttrType = [0];
					drop.item.instanceAttrValues = [5];
				}

				ServerManager.GetGame(gId).EntityDrop(drop);

				QueueFree();
			}
		}
	}
}