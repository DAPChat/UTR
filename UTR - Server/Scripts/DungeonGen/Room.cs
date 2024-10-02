using Godot;
using System;
public class Room
{
	int x;
	int y;
	public int sX;
	public int sY;

	public bool skipped = false;

	int depth;

	Room[] rooms;

	public Panel panel;

	public Room(int _x, int _y, int _sX, int _sY, int _d)
	{
		x = _x; 
		y = _y;
		sX = _sX;
		sY = _sY;
		depth = _d;

		rooms = new Room[2];
	}

	public void Partition(int minSize, int maxDepth, Panel _p, Dungen _d)
	{
		RandomNumberGenerator rand = new();

		short dir = 0;

		if (sX > minSize * 2 && sY > minSize * 2) {
			if (sX < (sY * 1.25)) dir = 2;
			else if (sY < (sX * 1.25)) dir = 1;
			else dir = (short)rand.RandiRange(1, 2);
		}
		else if (sY > minSize * 2) dir = 1;
		else if (sX > minSize * 2) dir = 2;

		//if (depth > 4 && rand.RandfRange(0,1) > .9) dir = 0;

		if (dir == 0 || depth == maxDepth)
		{
			Create(_d, _p);
			return;
		}

		int size;

		if (dir == 1)
		{
			size = rand.RandiRange(minSize, sY - minSize);

			rooms[0] = new(x, y, sX, size, depth+1);
			rooms[1] = new(x, y+size, sX, sY-size, depth+1);
		}
		else if (dir == 2)
		{
			size = rand.RandiRange(minSize, sX - minSize);

			rooms[0] = new(x, y, size, sY, depth + 1);
			rooms[1] = new(x + size, y, sX - size, sY, depth + 1);
		}

		foreach (Room room in rooms)
		{
			
			room.Partition(minSize, maxDepth, _p, _d);
		}
	}
	/*
	 if (depth == 3 && !skipped && room.sX * room.sY >= 250 * 250)
			{
				if (_d.bsp.main > 0)
				{
					skipped = true;
					room.skipped = true;
					room.depth = maxDepth - 1;

					int min = Math.Min(room.sX, room.sY);

					if ((250 * 250 / min) > minSize)
					{
						int _s = (int)Math.Ceiling(250.0 * 250 / min);

						GD.Print(room.sX + ": " + room.sY);
						GD.Print(_s);

						room.Partition(_s, maxDepth, _p, _d);
					}
					else room.Display(_d, _p);

					_d.bsp.main--;
					continue;
				}
			}
	 */

	public void Create (Dungen _d, Panel _p)
	{
		_d.bsp.rooms.Add(this);
		Panel _tp = (Panel) _p.Duplicate();
		_tp.Size = new(sX, sY);
		_tp.Position = new(x, y);
		//_tp.GetNode<Label>("Label").Text = depth.ToString();
		panel = _tp;
		//_d.AddChild(panel);
	}

	public void Display(Dungen _d)
	{
		_d.AddChild(panel);
	}
}