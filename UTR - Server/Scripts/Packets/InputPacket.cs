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

		public override void Run(game.Game _game)
		{
			_game.player.Velocity = inVect;
		}

		public override void Deserialize(Buffer buff)
		{
			Vector2I _tempVect = new();

			playerId = buff.ReadInt();
			_tempVect.X = buff.ReadInt();
			_tempVect.Y = buff.ReadInt();
		}

		public override byte[] Serialize()
		{
			throw new System.NotImplementedException();
		}
	}
}