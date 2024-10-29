using System.Collections.Generic;
using packets;

namespace items
{
	public class Item
	{
		public ItemBase item;
		public int type;
		public int[] instanceAttrType;
		public int[] instanceAttrValues;

		public Item(ItemBase _item)
		{
			item = _item;
			type = GetItemType(item);
			instanceAttrType = [];
			instanceAttrValues = [];
		}

		public Item(Buffer _buff)
		{
			Deserialize(_buff);
		}

		public byte[] Serialize()
		{
			List<object> list = [instanceAttrType.Length];

			for (int i = 0; i < instanceAttrType.Length; i++)
			{
				list.Add(instanceAttrType[i]);
				list.Add(instanceAttrValues[i]);
			}

			return Packet.Concat(item.Serialize(), Packet.Serialize(list.ToArray()));
		}

		public void Deserialize(Buffer _buff)
		{
			item = ItemManager.GetItemBase(_buff.ReadInt());
			type = GetItemType(item);

			int _len = _buff.ReadInt();
			instanceAttrType = new int[_len];
			instanceAttrValues = new int[_len];

			for (int i = 0; i < _len; i++)
			{
				instanceAttrType[i] = _buff.ReadInt();
				instanceAttrValues[i] = _buff.ReadInt();
			}
		}

		public static int GetItemType(ItemBase _item)
		{
			int type = 0;

			if (_item.GetType() == typeof(Tool))
			{
				type = 1;
			}
			else if (_item.GetType() == typeof(Consumable))
			{
				type = 2;
			}

			return type;
		}

		public override string ToString()
		{
			string s = "";

			s += item.name + "\n";
			s += item.description + "\n";

			return s;
		}
	}
}