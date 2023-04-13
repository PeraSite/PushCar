using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Sockets;
using Cysharp.Threading.Tasks;
using PushCar.Common;
using PushCar.Common.Packets.Client;
using PushCar.Common.Extensions;
using PushCar.Common.Packets.Server;
using UnityEngine;

public sealed class NetworkManager : MonoBehaviour {
	public static NetworkManager Instance { get; private set; }

	private TcpClient _client;
	private NetworkStream _stream;
	private BinaryReader _reader;
	private BinaryWriter _writer;
	private ConcurrentQueue<IPacket> _packetQueue;

	private void Awake() {
		_client = new TcpClient();
		Instance = this;
		_packetQueue = new ConcurrentQueue<IPacket>();
	}

	private void OnDestroy() {
		if (!_client.Connected) return;
		_client?.Dispose();
		_stream?.Dispose();
		_reader?.Dispose();
		_writer?.Dispose();
	}

	private void Update() {
		if (!_packetQueue.TryDequeue(out var incomingPacket)) return;

		Debug.Log($"Received packet: {incomingPacket}");
		switch (incomingPacket) {
			case ServerPongPacket packet: {
				Debug.Log("Pong!");
				break;
			}
			default: {
				throw new ArgumentOutOfRangeException();
			}
		}
	}

	public void SendPacket(IPacket packet) {
		_writer.Write(packet);
	}

	public async UniTask<bool> Join(IPEndPoint endpoint) {
		if (_client.Connected) {
			Debug.Log("Can't join twice!");
			return false;
		}

		Debug.Log($"Trying to join {endpoint}");
		try {
			await _client.ConnectAsync(endpoint.Address, endpoint.Port);
		}
		catch (Exception e) {
			Debug.Log($"Error : {e.Message}");
			return false;
		}

		_stream = _client.GetStream();
		_writer = new BinaryWriter(_stream);
		_reader = new BinaryReader(_stream);
		Debug.Log("Connected!");

		// Ping after connected
		_writer.Write(new ClientPingPacket());

		// Run packet listen thread
		await UniTask.RunOnThreadPool(() => {
			while (_client.Connected) {
				if (!_stream.CanRead) continue;
				if (!_stream.CanWrite) continue;
				if (!_client.Connected) {
					Debug.Log("Disconnected!");
					break;
				}

				var packetID = _reader.BaseStream.ReadByte();

				// 읽을 수 없다면(데이터가 끝났다면 리턴)
				if (packetID == -1) break;

				var packetType = (PacketType)packetID;

				// 타입에 맞는 패킷 객체 생성 후 큐에 추가
				var basePacket = packetType.CreatePacket(_reader);
				_packetQueue.Enqueue(basePacket);
			}
		}, cancellationToken: this.GetCancellationTokenOnDestroy());
		return true;
	}
}
