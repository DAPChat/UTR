namespace packets
{
	public class StatsPacket : Packet
	{
		public int pos;

		public int health;
		public int mana;

		public int points;

		public StatsPacket(Buffer _buff) : base(_buff)
		{
		}

		public StatsPacket(int _id, int _health, int _mana, int _pts, int _data = -1) : base(_id, _data)
		{
			health = _health;
			mana = _mana;
			points = _pts;
		}

		public override void Run()
		{
			//base.Run(_gId);

			if (pos < PacketManager.statPacketOrder) return;

			PacketManager.statPacketOrder = pos;

			ClientManager.UpdateStats(this);
		}

		public override byte[] Serialize()
		{
			return Serialize([5, playerId, data, pos, health, mana, points]);
		}

		public override void Deserialize(Buffer buff)
		{
			base.Deserialize(buff);
			pos = buff.ReadInt();
			health = buff.ReadInt();
			mana = buff.ReadInt();
			points = buff.ReadInt();
		}
	}
}