using Godot;
using System.Collections.Generic;
using System.Linq;

[GlobalClass]
public partial class Item : Resource
{
	[Export]
	public string name;
	[Export]
	public string description;
	[Export]
	public string icon;
	[Export]
	public int[] attributeType;
	[Export]
	public int[] attributeValues;

	public int[] instanceAttrType;
	public int[] instanceAttrValues;

	public virtual byte[] GetBytes()
	{
		List<object> list = [];

		int _id = int.Parse(ResourcePath.Where(char.IsDigit).ToArray());

		list.Add(_id);
		list.Add(instanceAttrType.Length);
		for (int i = 0; i < instanceAttrType.Length; i++)
		{
			list.Add(instanceAttrType[i]);
			list.Add(instanceAttrValues[i]);
		}

		string s = "";

		foreach (object obj in list)
		{
			s += obj;
		}

		GD.Print(s);

		return packets.Packet.Serialize(list.ToArray());
	}
}