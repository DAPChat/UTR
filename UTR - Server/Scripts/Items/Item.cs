﻿using System.Collections.Generic;
using System.Linq;

namespace items
{
	public class Item
	{
		public ItemBase item;
		public int[] instanceAttrType;
		public int[] instanceAttrValues;

		public Item(ItemBase _item)
		{
			item = _item;
			instanceAttrType = [];
			instanceAttrValues = [];
		}

		public Item(Buffer _buff)
		{
			Deserialize(_buff);
		}

		public byte[] Serialize()
		{
			List<object> list = [];

			list.Add(instanceAttrType.Length);
			for (int i = 0; i < instanceAttrType.Length; i++)
			{
				list.Add(instanceAttrType[i]);
				list.Add(instanceAttrValues[i]);
			}

			return item.Serialize().Concat(packets.Packet.Serialize(list.ToArray())).ToArray();
		}

		public void Deserialize(Buffer _buff)
		{
			item = ItemManager.GetItemBase(_buff.ReadInt());

			int _len = _buff.ReadInt();
			instanceAttrType = new int[_len];
			instanceAttrValues = new int[_len];

			for (int i = 0; i < _len; i++)
			{
				instanceAttrType[i] = _buff.ReadInt();
				instanceAttrValues[i] = _buff.ReadInt();
			}
		}
	}
}