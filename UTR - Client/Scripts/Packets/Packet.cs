﻿using System.IO;

namespace packets
{
	public class Packet
	{
		public int playerId;
		public int data = -1;

		public Packet(Buffer _buff)
		{
			Deserialize(_buff);
		}

		public Packet(int _id, int _data = -1)
		{
			playerId = _id;
			data = _data;
		}

		public virtual void Run()
		{
			ClientManager.SetClient(this);
		}

		public virtual byte[] Serialize()
		{
			using (MemoryStream m = new())
			{
				using (BinaryWriter writer = new(m))
				{
					writer.Write(0);
					writer.Write(playerId);
					writer.Write(data);
				}
				return m.ToArray();
			}
		}

		public virtual void Deserialize(Buffer buff)
		{
			playerId = buff.ReadInt();
			data = buff.ReadInt();
		}
	}
}