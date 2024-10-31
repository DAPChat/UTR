﻿using Godot;
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

		public void Instantiate(int _gId)
		{
			gId = _gId;

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
		}

		public override void _PhysicsProcess(double delta)
		{
			base._PhysicsProcess(delta);

			if (trackingId != -1)
			{
				if (ServerManager.GetClient(trackingId) == null)
				{
					GetNode<Area2D>("TrackerArea").EmitSignal(Area2D.SignalName.AreaExited);
					return;
				}
				Vector2 pos = (ServerManager.GetClient(trackingId).player.Position - Position).Normalized();
				Velocity = pos*(float)delta*500;
				MoveAndSlide();

				ServerManager.GetGame(gId).SendAll(new packets.EnemyPacket(-1, this).Serialize());
			}
		}
	}
}