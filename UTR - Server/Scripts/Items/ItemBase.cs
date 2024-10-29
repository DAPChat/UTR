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
		public int maxStack;
		[Export]
		public int[] attributeType;
		[Export]
		public int[] attributeValues;

		public virtual byte[] Serialize()
		{
			string _rp = ResourcePath;
			_rp = _rp.Substring(_rp.IndexOf("ms/") + 3, -(_rp.IndexOf("ms/") + 3) + _rp.IndexOf(".tres"));

			return packets.Packet.Serialize([int.Parse(_rp)]);
		}
	}
}