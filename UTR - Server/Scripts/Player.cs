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
	public int gId;

	public int outOrder;
	public int inOrder;
	public int statOrder;
	public int slotOrder;

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
	public Vector2 mouse = new();

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

		statOrder = 0;
		slotOrder = 0;

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

		ServerManager.GetGame(gId).SendAll(new StatePacket(cId, 0, 1).Serialize());

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
				UseWeapon(item);
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

	public void UseWeapon(Item _item)
	{
		Tool tool = (Tool)_item.item;

		SetCollider(4);

		ServerManager.GetGame(gId).SendAll(new StatePacket(cId, 0, 0).Serialize());

		cooled = false;
		cooldownTimer.Start(tool.cooldown);

		if (enemies.Count == 0) { return; }
		foreach (Enemy e in enemies)
		{
			if (e.roomId != curRoom)
			{
				continue;
			}

			float damage = tool.baseDmg;

			if (_item.instanceAttrType.Length > 0 && Item.FindStat(_item.instanceAttrType, 0) != -1)
				damage += tool.lvlScale * _item.instanceAttrValues[Item.FindStat(_item.instanceAttrType, 0)];
			if (_item.instanceAttrType.Length > 0 && Item.FindStat(_item.instanceAttrType, 1) != -1)
			{
				damage *= new RandomNumberGenerator().RandiRange(1, 100) <= _item.instanceAttrValues[Item.FindStat(_item.instanceAttrType, 1)] ? 2 : 1;
			}

			e.Damage((int)damage, GlobalPosition, cId);
		}
	}

	public bool AddItem(Item _item, bool send)
	{
		int nullIndex = -1;
		int ind = -1;

		for (int i = inventory.Length-1; i >= 0; i--)
		{
			SlotPacket slot = inventory[i];

			if (slot == null)
			{
				nullIndex = i;
				continue;
			}

			if (slot.item.IsEqual(_item) && slot.count < slot.item.item.maxStack)
			{
				slot.count++;
				ind = i;
				nullIndex = -1;
				break;
			}
		}

		if (nullIndex != -1)
		{
			ind = nullIndex;
			inventory[nullIndex] = new SlotPacket(cId, _item, nullIndex, 1, 0);
		}

		if (ind != -1)
		{
			if (send) ServerManager.GetGame(gId).SendTo(cId, inventory[ind].Serialize());

			return true;
		}

		return false;
	}

	public void MoveItem(int _to, int _from, int _loc)
	{
		if (_to < 6)
		{
			_to--;
			// Item from inventory to hotbar or hotbar to hotbar
			if (_loc == 2)
			{
				if (hotbar[_from] == null)
				{
					return;
				}

				// hot to hot
				if (_from == _to) return;

				(hotbar[_to], hotbar[_from]) = (hotbar[_from], hotbar[_to]);

				if (hotbar[_from] != null)
				{
					(hotbar[_to].slot, hotbar[_from].slot) = (hotbar[_from].slot, hotbar[_to].slot);
				}
				else hotbar[_to].slot = _from;
			}
			else
			{
				if (inventory[_from] == null) return;

				if (hotbar[_to] != null && inventory[_from].item.IsEqual(hotbar[_to].item) && hotbar[_to].count < hotbar[_to].item.item.maxStack)
				{
					if (hotbar[_to].count + inventory[_from].count > hotbar[_to].item.item.maxStack)
					{
						inventory[_from].count -= (hotbar[_to].item.item.maxStack - hotbar[_to].count);
						hotbar[_to].count = hotbar[_to].item.item.maxStack;
					}
					else
					{
						hotbar[_to].count += inventory[_from].count;
						inventory[_from].count = 0;
					}
				}
				else
				{
					// inv to hot
					(hotbar[_to], inventory[_from]) = (inventory[_from], hotbar[_to]);

					if (inventory[_from] != null)
					{
						(hotbar[_to].slot, inventory[_from].slot) = (inventory[_from].slot, hotbar[_to].slot);
						(hotbar[_to].data, inventory[_from].data) = (inventory[_from].data, hotbar[_to].data);
					}
					else
					{
						hotbar[_to].slot = _from;
						hotbar[_to].data = 1;
					}
				}
			}

			if (_to == activeSlot || (_loc == 2 && _from == activeSlot)) { noItem = true; SetActiveSlot(activeSlot + 1); }
		} 
		else if (_to == 6 && _loc == 2)
		{
			if (hotbar[_from] == null) return;

			int toRemove = 0;

			for (int i = 0; i < hotbar[_from].count; i++)
			{
				if (!AddItem(hotbar[_from].item, false))
				{
					break;
				}
				else
				{
					toRemove++;
				}
			}

			hotbar[_from].count -= toRemove;

			if (hotbar[_from].count <= 0)
			{
				hotbar[_from] = null;
			}
		}

		string inv = "Hotbar: ";

		for (int i = 0; i < hotbar.Length; i++)
		{
			SlotPacket item = hotbar[i];

			SlotPacket sp;

			if (item == null) sp = new SlotPacket(cId, ItemManager.GetItem(-1), i, 0, 1);
			else sp = item;

			sp.slot = i;

			hotbar[i] = item == null ? null : sp;

			inv += sp;

			ServerManager.GetGame(gId).SendTo(cId, sp.Serialize());
		}

		//GD.Print(inv);

		inv = "Inventory: ";

		for (int i = 0; i < inventory.Length; i++)
		{
			SlotPacket item = inventory[i];

			SlotPacket sp;

			if (item == null) sp = new SlotPacket(cId, ItemManager.GetItem(-1), i, 0, 0);
			else sp = item;

			sp.slot = i;

			inv += sp;

			inventory[i] = item == null ? null : sp;

			ServerManager.GetGame(gId).SendTo(cId, sp.Serialize());
		}

		//GD.Print(inv);
	}

	public void Move(MovePacket move)
	{
		if (!active) return;
		if (inOrder >= move.order) { return; }

		GetNode<Area2D>("WeaponArea").LookAt(new Vector2(move.cX, move.cY));
		GetNode<Area2D>("WeaponArea").RotationDegrees -= 180;

		mouse = new Vector2(move.cX, move.cY);

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

		ServerManager.GetGame(gId).SendAll(new MovePacket(cId, outOrder, Position.X, Position.Y, mouse.X, mouse.Y, prevPos != Position ? (int)inMove.X : 0).Serialize());
	}

	public void EnemyKill()
	{
		points++;
		ServerManager.GetGame(gId).SendAll(new StatsPacket(cId, health, mana, points).Serialize());
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
