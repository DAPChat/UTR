using System;
using System.Text;

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

	public string ReadString()
	{
		int len = data[pos];
		pos += 1;

		string val = Encoding.UTF8.GetString(data[pos..(pos+len)]);
		pos += len;

		return val;
	}

	public char ReadChar()
	{
		char val = BitConverter.ToChar(data, pos);
		pos += 2;
		return val;
	}
}