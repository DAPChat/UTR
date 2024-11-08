using System.IO;
using Godot;

namespace packets
{
	public class InputPacket : Packet
	{
		public int input;

		public InputPacket(Buffer _buff) : base(_buff) { }
		public InputPacket(int id, int _input, int _data = -1) : base(id, _data)
		{
			input = _input;
		}

		public override void Deserialize(Buffer buff)
		{
			base.Deserialize(buff);
			input = buff.ReadInt();
		}

		public override void Run()
		{
			throw new System.NotImplementedException();
		}

		public override byte[] Serialize()
		{
			return Serialize([2, playerId, data, input]);
		}
	}
}