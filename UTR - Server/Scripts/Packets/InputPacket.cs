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

		public override void Run(int _gId)
		{
			Player _p = ServerManager.GetClient(playerId).player;
			_p.Velocity = inVect * 100;
			_p.MoveAndSlide();

			ServerManager.GetGame(_gId).SendAll(new MovePacket(playerId, _p.Position.X, _p.Position.Y).Serialize());
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