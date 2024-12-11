using Godot;
using System;

using packets;
using items;
using enemy;
using System.Collections.Generic;
using System.Linq;

public partial class Player : CharacterBody2D
{
	public int cId;
	int gId;

	public int outOrder;
	public int inOrder;

	bool noItem = true;
	bool active = false;

	bool cooled = true;
	bool invincible = false;
	bool knocked = false;
	Vector2 knockedDir = new();

	int maxHealth = 100;

	public int activeSlot = 0;
	public int health;
	public int mana;
	public int points;

	public int curRoom;

	//public bool dir; // Left false, Right true

	public SlotPacket[] inventory = new SlotPacket[8];
	public SlotPacket[] hotbar = new SlotPacket[5];

	public Vector2 inMove = new();

	// Temp Solution (Replace the following w/ spawning a new shape
	//Area2D area;
	List<Enemy> enemies = new();

	Timer cooldownTimer;
	Timer invincibilityTimer;
	Timer knockbackTimer;

	public void Instantiate(int _cId, int _gId)
	{
		cId = _cId;
		gId = _gId;

		outOrder = 0;
		inOrder = -1;

		health = maxHealth;

		active = true;

		Area2D _ar = GetNode<Area2D>("WeaponArea");

		_ar.AreaEntered += (body) =>
		{
			if (body.GetParentOrNull<Enemy>() != null) enemies.Add(body.GetParent<Enemy>());
		};

		_ar.AreaExited += (body) =>
		{
			if (body.GetParentOrNull<Enemy>() != null) enemies.Remove(body.GetParent<Enemy>());
		};

		cooldownTimer = GetNode<Timer>("Cooldown");

		cooldownTimer.Timeout += () =>
		{
			cooled = true;
		};

		invincibilityTimer = GetNode<Timer>("Invincibility");

		invincibilityTimer.Timeout += () =>
		{
			invincible = false;
		};

		knockbackTimer = GetNode<Timer>("Knockback");

		knockbackTimer.Timeout += () =>
		{
			knocked = false;
		};

		hotbar[0] = new(cId, ItemManager.GetItem(0), 0, 1, 1);
		hotbar[1] = new(cId, ItemManager.GetItem(1), 1, 2, 1);
		noItem = false;
		SlotPacket _sp = new(cId, hotbar[activeSlot].item, activeSlot, hotbar[activeSlot].count, 2);

		ServerManager.GetGame(gId).SendTo(cId, hotbar[0].Serialize());
		ServerManager.GetGame(gId).SendTo(cId, hotbar[1].Serialize());
		ServerManager.GetGame(gId).SendAll(_sp.Serialize());

		ServerManager.GetGame(gId).SendAll(new StatsPacket(cId, health, mana, points).Serialize());
	}

	public void Damage(Enemy damager, int _dmg)
	{
		if (invincible) return;

		health -= _dmg;

		// Player death
		if (health <= 0)
		{
			if (Die()) return;
		}

		invincible = true;
		invincibilityTimer.Start(.25);

		knocked = true;
		knockbackTimer.Start();
		knockedDir = (GlobalPosition - damager.GlobalPosition).Normalized();

		ServerManager.GetGame(gId).SendAll(new StatsPacket(cId, health, mana, points).Serialize());
	}

	public bool Die()
	{
		ServerManager.GetGame(gId).SendTo(cId, new StatePacket(cId, 0, -1).Serialize());
		ServerManager.GetGame(gId).Destroy(cId);
		return true;
	}

	public void SetActiveSlot(int _slot)
	{
		if (!active) return;

		int _tempIn = activeSlot;

		activeSlot = _slot - 1;

		SlotPacket _sp;

		if ((_tempIn == _slot - 1 && !noItem) || hotbar[_slot - 1] == null)
		{
			_sp = new(cId, ItemManager.GetItem(-1), activeSlot, 0, 2);
			noItem = true;
		}
		else
		{
			noItem = false;
			_sp = new(cId, hotbar[activeSlot].item, activeSlot, hotbar[activeSlot].count, 2);

			if (hotbar[activeSlot].item.type == 1) {
				cooled = false;
				cooldownTimer.Start(((Tool)hotbar[activeSlot].item.item).cooldown);
			}
		}

		ServerManager.GetGame(gId).SendAll(_sp.Serialize());
	}

	public void UseItem()
	{
		if (!active) return;
		if (hotbar[activeSlot] == null || noItem) return;
		
		Item item = hotbar[activeSlot].item;

		if (item.type == 1)
		{
			Tool tool = (Tool)item.item;

			if (!cooled) return;

			if (tool.type == 0)
			{
				SetCollider(4);

				ServerManager.GetGame(gId).SendAll(new StatePacket(cId, 0, 0).Serialize());

				cooled = false;
				cooldownTimer.Start(tool.cooldown);

				if (enemies.Count == 0) { return; }
				if (enemies[0].roomId != curRoom)
				{
					Enemy _temp = enemies[0];
					enemies.Remove(_temp);
					enemies.Add(_temp);
					return;
				}
				enemies[0].Damage(tool.baseDmg, GlobalPosition, cId);
			}
		}
		else if (item.type == 2)
		{
			Consumable _cons = (Consumable)item.item;

			switch (_cons.value)
			{
				case 0:
					health = Math.Clamp(_cons.recover + health, 0, maxHealth);
					break;
			}

			hotbar[activeSlot].count -= 1;

			ServerManager.GetGame(gId).SendAll(new StatsPacket(cId, health, mana, points).Serialize());
			ServerManager.GetGame(gId).SendTo(cId, hotbar[activeSlot].Serialize());
		}

		if (hotbar[activeSlot].count <= 0)
		{
			hotbar[activeSlot] = null;
			noItem = true;
			ServerManager.GetGame(gId).SendAll(new SlotPacket(cId, ItemManager.GetItem(-1), activeSlot, 0, 2).Serialize());
		}
	}

	public void Move(MovePacket move)
	{
		if (!active) return;
		if (inOrder >= move.order) { return; }
		if (inMove == new Vector2(move.x, move.y)) return;

		inMove = new (move.x, move.y);
		if (inMove.X != 0)
			if (inMove.X == 1)
			{
				Scale = new (1, -1);
				RotationDegrees = 180f;
			}else
			{
				Scale = new (1, 1);
				RotationDegrees = 0f;
			}

		inOrder = move.order;
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector2 prevPos = Position;

		Velocity = Velocity.MoveToward(inMove.Normalized() * 100, 1500 * (float)GetPhysicsProcessDeltaTime());

		if (knocked) Velocity += (new Vector2(100, 100) * knockedDir);

		MoveAndSlide();

		if (ServerManager.GetGame(gId) == null) return;

		if (!active) return;

		ServerManager.GetGame(gId).SendAll(new MovePacket(cId, outOrder, Position.X, Position.Y, prevPos != Position ? (int)inMove.X : 0).Serialize());
	}

	public void Exit()
	{
		active = false;
		ServerManager.GetGame(gId).ChangeRoom(cId, null);
		ServerManager.GetGame(gId).SendAll(new Packet(cId, -2).Serialize());
		QueueFree();
	}

	public void SetCollider(int size)
	{
		Area2D _ar = GetNode<Area2D>("WeaponArea");
		CollisionShape2D _sh = _ar.GetNode<CollisionShape2D>("Shape");
		//((CapsuleShape2D)_sh.Shape).Radius = size;
	}
}
