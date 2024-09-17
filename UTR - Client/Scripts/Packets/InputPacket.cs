using System.IO;
using Godot;

namespace packets
{
	public class InputPacket : Packet
	{
		public Vector2I inVect;

		public InputPacket(Buffer _buff) : base(_buff) { }
		public InputPacket(Vector2I _inVect) 
		{
			inVect = _inVect;
		}

		public override void Deserialize(Buffer buff)
		{
			throw new System.NotImplementedException();
		}

		public override byte[] Serialize()
		{
			using (MemoryStream m = new())
			{
				using (BinaryWriter writer = new(m))
				{
					writer.Write(0);
					writer.Write(playerId);
					writer.Write(inVect.X);
					writer.Write(inVect.Y);
				}
				return m.ToArray();
			}
		}
	}
}