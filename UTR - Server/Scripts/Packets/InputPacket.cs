using System.IO;
using Godot;

namespace packets
{
	public class InputPacket : Packet
	{
		public char input;

		public InputPacket(Buffer _buff) : base(_buff) { }
		public InputPacket(int id, char _input, int _data = 0) : base(id, _data)
		{
			input = _input;
		}

		public override void Deserialize(Buffer buff)
		{
			base.Deserialize(buff);
			input = buff.ReadChar();
		}

		public override void Run(int _gId)
		{
			throw new System.NotImplementedException();
		}

		public override byte[] Serialize()
		{
			return Serialize([2, playerId, data, input]);
		}
	}
}