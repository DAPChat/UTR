using Godot;

namespace items
{
	[GlobalClass]
	public partial class Consumable : ItemBase
	{
		[Export]
		public int recover;
		[Export]
		public int value;
	}
}