using Godot;
using System;

public partial class TitleScene : Node2D
{
	Button btn;
	LineEdit gId;

	public static int reqId = -1;

	public override void _Ready()
	{
		base._Ready();

		PacketManager.CompileAll();

		btn = (Button)GetNode("CanvasLayer/StartBtn");
		gId = (LineEdit)GetNode("CanvasLayer/GameId");

		btn.Pressed += () => {
			GetTree().ChangeSceneToFile("res://Scenes/GameScene.tscn");
		};

		gId.TextChanged += (string _t) =>
		{
			try
			{
				reqId = _t.ToInt();
			}
			catch (Exception) { }
		};
	}
}
