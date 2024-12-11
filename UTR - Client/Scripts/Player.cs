using Godot;
using System;

using packets;

public partial class Player : CharacterBody2D
{
	public int outOrder;
	public int inOrder;
	public int health;

	public SlotPacket curItem;

	AnimatedSprite2D item;

	public void Instantiate()
	{
		outOrder = 0;
		inOrder = -1;

		item = GetNode<AnimatedSprite2D>("Item");

		item.AnimationFinished += () =>
		{
			item.Animation = curItem.item.item.simplename.ToLower() + "_use";
			item.Frame = 0;
		};
	}

	public void SetActiveItem(SlotPacket _slot)
	{
		if (_slot.count == 0)
		{
			item.Animation = "empty";
			return;
		}
		curItem = _slot;
		item.Animation = _slot.item.item.simplename.ToLower() + "_use";
	}

	// Update States
	/*
	-1 -> dead
	0 -> use item (anim)
	1 -> damage (take -> kb)
	 */

	public void StateUpdate(int _s, int _data)
	{
		if (_s == -1)
		{
			ClientManager.active = false;
			ClientManager.sceneTree.GetNode<ColorRect>("UI/Death").Show();
			ClientManager.sceneTree.ProcessMode = (ProcessModeEnum)4;
		}
		else if (_s == 0)
		{
			item.Play(curItem.item.item.simplename.ToLower() + "_use");
		}
		else if (_s == 1)
		{

		}
	}
}
