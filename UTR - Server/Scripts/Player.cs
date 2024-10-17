using Godot;
using System;

using packets;
using items;

public partial class Player : CharacterBody2D
{
	int cId;
	int gId;

	public Slot[] inventory = new Slot[8];
	public Slot[] hotbar = new Slot[5];

	public void Instantiate(int _cId, int _gId)
	{
		cId = _cId;
		gId = _gId;

		hotbar[0] = new(cId, ItemManager.GetItem(0), 0, 1);

		ServerManager.GetGame(_gId).SendTo(cId, hotbar[0].Serialize());
	}
}
