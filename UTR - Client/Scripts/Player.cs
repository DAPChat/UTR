using Godot;
using System;

using packets;
using items;

public partial class Player : CharacterBody2D
{
	public int outOrder;
	public int inOrder;
	public int health;

	public int statOrder;
	public int slotOrder;

	public SlotPacket curItem;

	AnimatedSprite2D item;
	AnimatedSprite2D overlay;
	AudioStreamPlayer2D audio;

	public void Instantiate()
	{
		outOrder = 0;
		inOrder = -1;
		statOrder = -1;
		slotOrder = -1;

		audio = GetNode<AudioStreamPlayer2D>("Audio");
		overlay = GetNode<AnimatedSprite2D>("Overlay");
		item = GetNode<AnimatedSprite2D>("Item");

		item.AnimationFinished += () =>
		{
			item.Animation = curItem.item.item.simplename.ToLower() + "_use";
			item.Frame = 0;
		};

		overlay.AnimationFinished += () =>
		{
			overlay.Hide();
			overlay.Animation = "empty";
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

	private void PlayAudio(string path)
	{
		if (audio == null) return;

		audio.Stream = ResourceLoader.Load<AudioStream>(path);
		audio.Play();
	}

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

			if (curItem.item.type == 1)
			{
				//overlay.Show();
				overlay.Play("sword_swing");
			}

			PlayAudio(curItem.item.item.audio);
		}
		else if (_s == 1)
		{

		}
	}
}
