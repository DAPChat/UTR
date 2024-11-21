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

	bool noItem = true;

	int maxHealth = 100;

	public int activeSlot = 0;
	public int health;
	public int mana;

	public int curRoom;

	//public bool dir; // Left false, Right true

	public SlotPacket[] inventory = new SlotPacket[8];
	public SlotPacket[] hotbar = new SlotPacket[5];

	public Vector2 inMove = new();

	// Temp Solution (Replace the following w/ spawning a new shape
	//Area2D area;
	List<Enemy> enemies = new();

	public void Instantiate(int _cId, int _gId)
	{
		cId = _cId;
		gId = _gId;

		health = 50;

		Area2D _ar = GetNode<Area2D>("WeaponArea");

		_ar.AreaEntered += (body) =>
		{
			if (body.GetParentOrNull<Enemy>() != null) enemies.Add(body.GetParent<Enemy>());
		};

		_ar.AreaExited += (body) =>
		{
			if (body.GetParentOrNull<Enemy>() != null) enemies.Remove(body.GetParent<Enemy>());
		};

		hotbar[0] = new(cId, ItemManager.GetItem(0), 0, 1, 1);
		hotbar[1] = new(cId, ItemManager.GetItem(1), 1, 2, 1);
		noItem = false;
		SlotPacket _sp = new(cId, hotbar[activeSlot].item, activeSlot, hotbar[activeSlot].count, 2);

		ServerManager.GetGame(gId).SendTo(cId, hotbar[0].Serialize());
		ServerManager.GetGame(gId).SendTo(cId, hotbar[1].Serialize());
		ServerManager.GetGame(gId).SendAll(_sp.Serialize());

		ServerManager.GetGame(gId).SendTo(cId, new StatsPacket(cId, health, mana).Serialize());
	}

	public void SetActiveSlot(int _slot)
	{
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
		}

		ServerManager.GetGame(gId).SendAll(_sp.Serialize());
	}

	public void UseItem()
	{
		if (hotbar[activeSlot] == null || noItem) return;
		
		Item item = hotbar[activeSlot].item;

		if (item.type == 1)
		{
			Tool tool = (Tool)item.item;

			if (tool.type == 0)
			{
				SetCollider(4);
				if (enemies.Count == 0) { return; }
				enemies.FirstOrDefault().Damage(tool.baseDmg);
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

			ServerManager.GetGame(gId).SendTo(cId, new StatsPacket(cId, health, mana).Serialize());
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
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector2 prevPos = Position;

		Velocity = Velocity.MoveToward(inMove.Normalized() * 100, 1500 * (float)GetPhysicsProcessDeltaTime());
		MoveAndSlide();

		if (ServerManager.GetGame(gId) == null) return;

		ServerManager.GetGame(gId).SendAll(new MovePacket(cId, Position.X, Position.Y, prevPos != Position ? (int)inMove.X : 0).Serialize());
	}

	public void SetCollider(int size)
	{
		Area2D _ar = GetNode<Area2D>("WeaponArea");
		CollisionShape2D _sh = _ar.GetNode<CollisionShape2D>("Shape");
		((CircleShape2D)_sh.Shape).Radius = size;
	}
}
