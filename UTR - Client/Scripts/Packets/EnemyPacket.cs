namespace packets
{
	public class EnemyPacket : Packet
	{
		public int order;
		public int enemyId;
		public float x;
		public float y;
		public int health;

		public EnemyPacket(Buffer _buff) : base(_buff)
		{
		}

		public EnemyPacket(int _id,/*, enemy.Enemy _en, */int _data = -1) : base(_id, _data)
		{
			//enemyId = _en.enemyId;
			//x = _en.Position.X;
			//y = _en.Position.Y;
		}

		public override void Run()
		{
			if (data == 0)
				ClientManager.RemoveEntity(enemyId);
			else
				ClientManager.MoveEntity(this);
		}

		public override byte[] Serialize()
		{
			return Serialize([6, playerId, data, order, enemyId, x, y, health]);
		}

		public override void Deserialize(Buffer buff)
		{
			base.Deserialize(buff);
			order = buff.ReadInt();
			enemyId = buff.ReadInt();
			x = buff.ReadFloat();
			y = buff.ReadFloat();
			health = buff.ReadInt();
		}
	}
}