using Godot;
using System;

public partial class TitleScene : Node2D
{
	Button btn;

	public override void _Ready()
	{
		base._Ready();

		PacketManager.CompileAll();

		btn = (Button)GetNode("CanvasLayer/StartBtn");

		btn.Pressed += () => {
			GetTree().ChangeSceneToFile("res://Scenes/GameScene.tscn");
		};
	}
}
