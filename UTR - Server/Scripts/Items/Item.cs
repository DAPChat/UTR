using System.Collections.Generic;
using System.Linq;

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

		public override string ToString()
		{
			return item.name;
		}

		public bool IsEqual(Item other)
		{
			bool equal = true;

			if (other == null) return false;
			if (other.instanceAttrValues.Length != instanceAttrValues.Length) return false;
			if (other.item.name != item.name) return false;
			
			for (int i = 0; i < other.instanceAttrValues.Length; i++)
			{
				if (other.instanceAttrValues[i] != instanceAttrValues[i]) return false;
			}

			if (!equal) ServerManager.Print("Not");

			return equal;
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

		public static int FindStat(int[] _objArr, int _obj)
		{
			int index = -1;

			for (int i = 0; i < _objArr.Length; i++)
			{
				if (_objArr[i] == _obj)
				{
					index = i;
					break;
				}
			}

			return index;
		}
	}
}