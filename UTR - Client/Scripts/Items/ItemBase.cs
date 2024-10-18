using Godot;
using System.Collections.Generic;
using System.Linq;

namespace items
{
	[GlobalClass]
	public partial class ItemBase : Resource
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

		public virtual byte[] Serialize()
		{
			return packets.Packet.Serialize([int.Parse(ResourcePath.Where(char.IsDigit).ToArray())]);
		}
	}
}