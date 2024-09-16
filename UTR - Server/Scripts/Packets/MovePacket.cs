﻿using System.IO;

namespace packets
{
	public class MovePacket : Packet
	{
		public float x, y;

		public MovePacket(Buffer _buff) : base(_buff) { }

		public MovePacket(int _id, float _x, float _y) : base(_id)
		{
			x = _x;
			y = _y;
		}

		public override byte[] Serialize()
		{
			using (MemoryStream m = new())
			{
				using (BinaryWriter writer = new(m))
				{
					writer.Write(0);
					writer.Write(playerId);
					writer.Write(x);
					writer.Write(y);
				}
				return m.ToArray();
			}
		}

		public override void Deserialize(Buffer buff)
		{
			playerId = (int)buff.ReadInt();
			x = (float)buff.ReadFloat();
			y = (float)buff.ReadFloat();
		}
	}
}