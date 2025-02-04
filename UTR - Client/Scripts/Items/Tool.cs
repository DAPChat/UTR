﻿using Godot;
using System.Linq;

namespace items
{
	[GlobalClass]
	public partial class Tool : ItemBase
	{
		[Export]
		public int type;
		[Export]
		public int baseDmg;
		[Export]
		public float lvlScale;
		[Export]
		public int range;
		[Export]
		public float cooldown;
	}
}