﻿using System.IO;
using Godot;

namespace packets
{
	public class InputPacket : Packet
	{
		public Vector2I inVect;

		public InputPacket(Buffer _buff) : base(_buff) { }
		public InputPacket(int id, Vector2I _inVect, int _data = 0) : base(id, _data)
		{
			inVect = _inVect;
		}

		public override void Deserialize(Buffer buff)
		{
			inVect = new();

			base.Deserialize(buff);
			inVect.X = buff.ReadInt();
			inVect.Y = buff.ReadInt();
		}

		public override void Run()
		{
			throw new System.NotImplementedException();
		}

		public override byte[] Serialize()
		{
			using (MemoryStream m = new())
			{
				using (BinaryWriter writer = new(m))
				{
					writer.Write(2);
					writer.Write(playerId);
					writer.Write(data);
					writer.Write(inVect.X);
					writer.Write(inVect.Y);
				}
				return m.ToArray();
			}
		}
	}
}