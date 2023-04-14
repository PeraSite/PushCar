using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using Cysharp.Threading.Tasks;
using PushCar.Common;
using PushCar.Common.Extensions;
using PushCar.Common.Packets.Client;
using PushCar.Common.Packets.Server;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PushCar {
	public sealed class NetworkManager : MonoBehaviour {
		public static NetworkManager Instance { get; private set; }

		private TcpClient _client;
		private NetworkStream _stream;
		private BinaryReader _reader;
		private BinaryWriter _writer;
		private ConcurrentQueue<IPacket> _packetQueue;

		private string _id;

		private void Awake() {
			_client = new TcpClient();
			_packetQueue = new ConcurrentQueue<IPacket>();

			if (Instance != null && Instance != this) {
				Destroy(gameObject);
				return;
			}

			Instance = this;
			DontDestroyOnLoad(gameObject);
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

			Debug.Log($"[S -> C] {incomingPacket}");
			HandlePacket(incomingPacket);
		}

		private void HandlePacket(IPacket incomingPacket) {
			switch (incomingPacket) {
				case ServerPongPacket: {
					SceneManager.LoadScene("Authenticate");
					break;
				}
				case ServerAuthenticatePacket packet: {
					var success = packet.Success;
					if (success) {
						SceneManager.LoadScene("Play");
					} else {
						Debug.LogError("Can't authenticate to the server!");
					}
					break;
				}
				default: {
					throw new ArgumentOutOfRangeException();
				}
			}
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
			SendPacket(new ClientPingPacket());

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

		public void Authenticate(string id, string password) {
			_id = id;
			var encryptedPassword = SHA256(password);
			SendPacket(new ClientAuthenticatePacket(id, encryptedPassword));
		}

		public void AddRecord(float swipeDistance) {
			SendPacket(new ClientRecordPacket(_id, swipeDistance));
		}

		private void SendPacket(IPacket packet) {
			Debug.Log($"[C -> S] {packet}");
			if (!_client.Connected) {
				Debug.LogError("Can't send packet when not connected!");
				return;
			}

			_writer.Write(packet);
		}

		private static string SHA256(string data) {
			var sha = new SHA256Managed();
			var hash = sha.ComputeHash(Encoding.ASCII.GetBytes(data));
			var sb = new StringBuilder();
			foreach (var b in hash) {
				sb.AppendFormat("{0:x2}", b);
			}
			return sb.ToString();
		}

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		private static void ResetStatic() {
			Instance = null;
		}
	}
}
