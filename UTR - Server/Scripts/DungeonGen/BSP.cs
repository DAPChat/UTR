using Godot;
using System.Collections.Generic;
using DelaunatorSharp;
using System;
using System.Linq;

public class BSP
{
	Dungen gen;
	Panel panelC;

	public List<Room> rooms = [];

	public int main = 4;

	public BSP (Dungen _dn)
	{
		gen = _dn;
		panelC = new();
	}

	public List<Room> CreateDungeon(int maxX, int maxY)
	{
		Room _r = new(400, 0, maxX, maxY, 0);
		_r.Partition(150, 10, panelC, gen);

		return AddRooms(maxX, maxY);
	}

	public List<Room> AddRooms(int _mX, int _mY)
	{
		RandomNumberGenerator rand = new();

		int rSize = 300;

		Vector2I pos = new()
		{
			X = _mX+400,
			Y = rand.RandiRange(0, _mY - rSize)
		};

		Room br = new(pos.X, pos.Y, rSize, rSize, 0);
		br.Create(gen, panelC);

		pos = new()
		{
			X = 400-rSize,
			Y = rand.RandiRange(0, _mY - rSize)
		};

		Room sr = new(pos.X, pos.Y, rSize, rSize, 0);
		sr.Create(gen, panelC);

		return MainRooms();
	}

	public List<Room> MainRooms()
	{
		List<Room> _tRs = [];

		RandomNumberGenerator rand = new();

		_tRs.Add(rooms[^1]);
		_tRs.Add(rooms[^2]);

        for (int i = 0; i < 6; i++)
        {
			int e = rand.RandiRange(0, rooms.Count-1);

			while (_tRs.Contains(rooms[e]))
			{
				e = rand.RandiRange(0, rooms.Count-1);
			}

			_tRs.Add(rooms[e]);
        }

		IPoint[] points = new IPoint[_tRs.Count];

		Dictionary<IPoint, int> _pS = [];

		for (int i = 0; i < _tRs.Count; i++)
		{
			Room room = _tRs[i];

			StyleBoxFlat _style = room.panel.GetThemeStylebox("panel").Duplicate() as StyleBoxFlat;

			_style.BgColor = Colors.DarkRed;

			//if (room.skipped) _style.BgColor = Colors.DarkGreen;

			Vector2 v = room.panel.Position + (room.panel.Size / 2);

			points[i] = new Point(v.X, v.Y);
			_pS.Add(points[i], i);

			room.panel.AddThemeStyleboxOverride("panel", _style);
		}

		Delaunator dl = new(points);

		Line2D l = new();
		l.Points = [Vector2.Zero, Vector2.Zero];

		List<Line2D> ls = [];
		List<MST.Edge> es = [];

		foreach (IEdge edge in dl.GetEdges())
		{
			int st = _pS[edge.Q];
			int en = _pS[edge.P];

			double lx = edge.P.X-edge.Q.X;
			double ly = edge.P.Y-edge.Q.Y;

			es.Add(new(st, en, (int)Math.Pow(lx, 2)+(int)Math.Pow(ly, 2)));
		}

		var _es = MST.Main(es, points.Length);

		foreach (var e in es)
		{
			if (_es.Contains(e))
			{
				continue;
			}
			if (rand.RandfRange(0, 1) > 0.75)
			{
				_es.Add(e);
			}
		}

		foreach (var e in _es)
		{
			Line2D _l = (Line2D)l.Duplicate();
			//Vector2 sP = _tRs[edge.Index].panel.Position + (_tRs[edge.Index].panel.Size/2);
			_l.Points = [GetKey(_pS, e.Source), GetKey(_pS, e.Destination)];

			ls.Add(_l);
			//gen.AddChild(_l);
		}

		return GenLines(ls);
	}

	public List<Room> GenLines(List<Line2D> ls)
	{
		Line2D _l = new();
		_l.Points = [Vector2.Zero, Vector2.Zero];

		List<Line2D> lines = [];
		List<Room> roomL = [];

		foreach (var l in ls)
		{
			Vector2 e = l.Points[0];
			Vector2 s = l.Points[1];

			float x = s.X - e.X;
			float y = s.Y - e.Y;

			if (l.Points[0] == rooms[^1].panel.Position + rooms[^1].panel.Size/2) e.X = e.X + 150;
			else if (l.Points[0] == rooms[^2].panel.Position + rooms[^2].panel.Size / 2) e.X = e.X - 150;
			else if (l.Points[1] == rooms[^1].panel.Position + rooms[^1].panel.Size / 2) e.X = e.X + 150;
			else if (l.Points[1] == rooms[^2].panel.Position + rooms[^2].panel.Size / 2) e.X = e.X - 150;

			Line2D la = (Line2D)_l.Duplicate();
			la.Points = [new(e.X, e.Y), new(e.X+x, e.Y)];

			Line2D lb = (Line2D)_l.Duplicate();
			lb.Points = [new(e.X+x, e.Y), new(e.X+x, e.Y+y)];

			lines.Add(la);
			lines.Add(lb);

			foreach (var r in rooms)
			{
				if (LineIntersectsRect(la.Points[0], la.Points[1], r.panel) || LineIntersectsRect(lb.Points[0], lb.Points[1], r.panel)) roomL.Add(r);
			}
		}

		foreach (var r in rooms)
		{
			if (roomL.Contains(r)) r.Display(gen);
		}

		return roomL;
	}

	public static bool LineIntersectsRect(Vector2 p1, Vector2 p2, Panel r)
	{
		return LineIntersectsLine(p1, p2, new Vector2(r.Position.X, r.Position.Y), new Vector2(r.Position.X + r.Size.X, r.Position.Y)) ||
			   LineIntersectsLine(p1, p2, new Vector2(r.Position.X + r.Size.X, r.Position.Y), new Vector2(r.Position.X + r.Size.X, r.Position.Y + r.Size.Y)) ||
			   LineIntersectsLine(p1, p2, new Vector2(r.Position.X + r.Size.X, r.Position.Y + r.Size.Y), new Vector2(r.Position.X, r.Position.Y + r.Size.Y)) ||
			   LineIntersectsLine(p1, p2, new Vector2(r.Position.X, r.Position.Y + r.Size.Y), new Vector2(r.Position.X, r.Position.Y)); //||
			   //(r.Position(p1) && r.Contains(p2));
	}

	private static bool LineIntersectsLine(Vector2 l1p1, Vector2 l1p2, Vector2 l2p1, Vector2 l2p2)
	{
		float q = (l1p1.Y - l2p1.Y) * (l2p2.X - l2p1.X) - (l1p1.X - l2p1.X) * (l2p2.Y - l2p1.Y);
		float d = (l1p2.X - l1p1.X) * (l2p2.Y - l2p1.Y) - (l1p2.Y - l1p1.Y) * (l2p2.X - l2p1.X);

		if (d == 0)
		{
			return false;
		}

		float r = q / d;

		q = (l1p1.Y - l2p1.Y) * (l1p2.X - l1p1.X) - (l1p1.X - l2p1.X) * (l1p2.Y - l1p1.Y);
		float s = q / d;

		if (r < 0 || r > 1 || s < 0 || s > 1)
		{
			return false;
		}

		return true;
	}

	public static Vector2 GetKey(Dictionary<IPoint, int> d, int val)
	{
		for (int i = 0; i < d.Count; i++)
		{
			if (d.ElementAt(i).Value == val)
			{
				return new((float)d.ElementAt(i).Key.X, (float)d.ElementAt(i).Key.Y);
			}
		}

		return Vector2.Zero;
	}
}