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
			_game.player.Velocity = inVect*100;
			ServerManager.Print(_game.player.MoveAndSlide() ? ("Yes") : ("No"));
			_game.SendAll(new MovePacket(playerId, (float)_game.player.Position.X, (float)_game.player.Position.Y).Serialize());
		}

		public override void Deserialize(Buffer buff)
		{
			Vector2I _tempVect = new();

			playerId = buff.ReadInt();
			_tempVect.X = buff.ReadInt();
			_tempVect.Y = buff.ReadInt();

			inVect = _tempVect;
		}

		public override byte[] Serialize()
		{
			throw new System.NotImplementedException();
		}
	}
}