using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using PushCar.Common.Packets.Client;
using PushCar.Common.Utils;

public sealed class NetworkManager : IDisposable {
	private TcpClient _client;
	private NetworkStream _stream;
	private BinaryReader _reader;
	private BinaryWriter _writer;

	// 현재 서버와 연결되어있는가?
	private bool _connected;

	// Singleton 패턴
	private NetworkManager() {
		// TCP 클라이언트 초기화
		_client = new TcpClient();
	}
	private static NetworkManager _instance;
	public static NetworkManager Instance => _instance ??= new NetworkManager();

	public void Dispose() {
		if (!_connected) return;
		_client?.Dispose();
		_stream?.Dispose();
		_reader?.Dispose();
		_writer?.Dispose();
	}

	public void Join(IPEndPoint endpoint) {
		if (_connected) {

			Debug.Log("Can't join twice!");
			return;
		}

		Debug.Log($"Trying to join {endpoint}");
		_client.Connect(endpoint);

		_stream = _client.GetStream();
		_writer = new BinaryWriter(_stream);
		_reader = new BinaryReader(_stream);
		_connected = true;
		Debug.Log("Connected!");

		_writer.Write(new ClientPingPacket());
	}
}
