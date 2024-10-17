using Godot;

[GlobalClass]
public partial class Tool : Item
{
	[Export]
	public int type;
	[Export]
	public int baseDmg;
	[Export]
	public int lvlScale;
	[Export]
	public int range;
	[Export]
	public int cooldown;

	public override byte[] GetBytes()
	{
		return base.GetBytes();
	}
}