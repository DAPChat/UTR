using System.IO;
using Godot;

namespace packets
{
	public class InputPacket : Packet
	{
		public Vector2I inVect;

		public InputPacket(Buffer _buff) : base(_buff) { }
		public InputPacket(int id, Vector2I _inVect) : base(id)
		{
			inVect = _inVect;
		}

		public override void Deserialize(Buffer buff)
		{
			inVect = new();

			playerId = buff.ReadInt();
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
					writer.Write(1);
					writer.Write(playerId);
					writer.Write(inVect.X);
					writer.Write(inVect.Y);
				}
				return m.ToArray();
			}
		}
	}
}