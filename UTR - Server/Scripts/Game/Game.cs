using System.Collections.Generic;

using packets;

namespace game
{
	public class Game
	{
		private readonly int gameId;

		private Dungeon dun;
		private List<Client> clients;
		private List<Packet> packetQueue = new();

		public Game(int _gameId, Client[] _clients)
		{
			gameId = _gameId;
			clients.AddRange(_clients);

			dun = new(10);
		}
	}
}