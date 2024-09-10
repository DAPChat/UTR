using System;

public class Buffer(byte[] _buff)
{
	byte[] data = _buff;
	int pos = 0;

	public int ReadInt()
	{
		int val = BitConverter.ToInt32(data, pos);
		pos += 4;

		return val;
	}

	public float ReadFloat()
	{
		float val = BitConverter.ToSingle(data, pos);
		pos += 4;

		return val;
	}
}