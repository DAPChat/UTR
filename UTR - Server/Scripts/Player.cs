using Godot;
using System;

using packets;
using items;

public partial class Player : CharacterBody2D
{
	public int cId;
	int gId;

	bool noItem = true;

	int maxHealth = 100;

	public int activeSlot = 0;
	public int health;
	public int mana;

	public SlotPacket[] inventory = new SlotPacket[8];
	public SlotPacket[] hotbar = new SlotPacket[5];

	public void Instantiate(int _cId, int _gId)
	{
		cId = _cId;
		gId = _gId;

		health = 50;

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
		if (hotbar[activeSlot] == null) return;
		
		Item item = hotbar[activeSlot].item;

		if (item.type == 1)
		{

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
}
