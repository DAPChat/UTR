using System;

public class Buffer
{
	byte[] data;
	int pos;


	public Buffer(byte[] _buff)
	{
		data = _buff;
		pos = 0;
	}

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