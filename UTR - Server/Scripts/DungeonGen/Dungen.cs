using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class Dungen : Node
{
	public BSP bsp;

	public List<Room> rooms = [];

	PackedScene scene;
	
	public void CreateDungeon()
	{
		scene = ResourceLoader.Load<PackedScene>("res://Scenes/wall_tile.tscn");
		bsp = new(this);

		bsp.CreateDungeon(1000, 1000);
	}
}
