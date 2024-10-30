namespace packets
{
	public class StatsPacket : Packet
	{
		public int pos;

		public int health;
		public int mana;

		public StatsPacket(Buffer _buff) : base(_buff)
		{
		}

		public StatsPacket(int _id, int _health, int _mana, int _data = -1) : base(_id, _data)
		{
			health = _health;
			mana = _mana;
		}

		public override byte[] Serialize()
		{
			pos = PacketManager.statPacketOrder;
			PacketManager.statPacketOrder++;

			return Serialize([5, playerId, data, pos, health, mana]);
		}

		public override void Deserialize(Buffer buff)
		{
			base.Deserialize(buff);
			pos = buff.ReadInt();
			health = buff.ReadInt();
			mana = buff.ReadInt();
		}
	}
}