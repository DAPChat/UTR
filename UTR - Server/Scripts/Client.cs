﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

public class Client
{
	private TCP tcp;
	private UDP udp;
	private int id;
	//private Player player;
	//private Account account;

	public Client(TcpClient _tcpClient, int _id)
	{
		tcp = new(_tcpClient, this);
		udp = new(_tcpClient.Client.RemoteEndPoint as IPEndPoint, this);
		id = _id;
	}

	private class TCP
	{
		Client instance;
		TcpClient tcpClient;
		NetworkStream stream;

		byte[] buffer;

		public TCP(TcpClient _tcpClient, Client _instance)
		{
			instance = _instance;
			tcpClient = _tcpClient;

			buffer = new byte[512];

			stream = tcpClient.GetStream();
			
			ReadStreamAsync();
		}

		private async Task ReadStreamAsync()
		{
			try
			{
				int _readLength = await stream.ReadAsync(buffer, 0, buffer.Length);

				if (_readLength <= 0)
				{
					// Disconnect
					return;
				}

				StringBuilder sb = new(Encoding.ASCII.GetString(buffer));

				while (stream.DataAvailable)
				{
					_readLength = stream.Read(buffer, 0, buffer.Length);
					sb.Append(Encoding.ASCII.GetString(buffer, 0, _readLength));
				}
			}
			catch (Exception)
			{
				// Disconnect
				return;
			}
		}
	}

	private class UDP
	{
		Client instance;
		UdpClient udpClient;
		IPEndPoint end;

		public UDP(IPEndPoint _end, Client _instance)
		{
			instance = _instance;
			end = _end;

			udpClient = new(end);
			udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

			udpClient.Connect(end);

			ServerManager.Print(end.ToString());

			udpClient.SendAsync(Encoding.ASCII.GetBytes("Hello From Server"), "Hello From Server".Length);

			Read();
		}

		private async Task Read()
		{
			try
			{
				byte[] buffer = udpClient.Receive(ref end);//(await udpClient.ReceiveAsync()).Buffer;

				ServerManager.Print(Encoding.ASCII.GetString(buffer));
			}
			catch (Exception)
			{
				// Disconnect

				return;
			}

			Read();
		}
	}
}