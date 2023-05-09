using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using Cysharp.Threading.Tasks;
using PushCar.Common;
using PushCar.Common.Extensions;
using PushCar.Common.Packets.Client;
using PushCar.Common.Packets.Server;
using PushCar.Common.Utils;
using PushCar.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

namespace PushCar {
	public sealed class NetworkManager : MonoBehaviour {
		public static NetworkManager Instance { get; private set; }

		public event Action<IPacket> OnPacketReceived;

		private TcpClient _client;
		private NetworkStream _stream;
		private SslStream _sslStream;
		private BinaryReader _reader;
		private BinaryWriter _writer;
		private ConcurrentQueue<IPacket> _packetQueue;

		private Guid _token;
		private string _salt;

		private void Awake() {
			_client = new TcpClient();
			_packetQueue = new ConcurrentQueue<IPacket>();

			if (Instance != null && Instance != this) {
				Destroy(gameObject);
				return;
			}

			Instance = this;
			DontDestroyOnLoad(gameObject);

			// 빈 salt 초기화
			_salt = string.Empty;
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
			OnPacketReceived?.Invoke(incomingPacket);
			switch (incomingPacket) {
				case ServerPongPacket: {
					Toast.Instance.Info("서버에 접속했습니다!");
					SceneManager.LoadScene("Authenticate");
					break;
				}
				case ServerAuthenticateSuccessPacket packet: {
					_token = packet.Token;
					SceneManager.LoadScene("Play");
					break;
				}
				case ServerResponseSaltPacket packet: {
					// 자신의 salt를 받았다면 저장
					_salt = packet.Salt;
					break;
				}
				case ServerAuthenticateFailPacket packet: {
					Toast.Instance.Error(packet.Reason);
					break;
				}
			}
		}

		public async UniTaskVoid Join(IPEndPoint endpoint) {
			if (_client.Connected) {
				Debug.Log("Can't join twice!");
				return;
			}

			try {
				await _client.ConnectAsync(endpoint.Address, endpoint.Port);
			}
			catch (Exception e) {
				Debug.Log($"Error : {e.Message}");
				Toast.Instance.Error("해당 서버에 연결하지 못했습니다!");
				return;
			}

			_stream = _client.GetStream();
			_sslStream = new SslStream(_stream, false, (_, _, _, _) => true);
			await _sslStream.AuthenticateAsClientAsync(SystemInfo.deviceName);

			_writer = new BinaryWriter(_sslStream);
			_reader = new BinaryReader(_sslStream);

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

					try {
						var packetID = _reader.BaseStream.ReadByte();

						// 읽을 수 없다면(데이터가 끝났다면 리턴)
						if (packetID == -1) break;

						var packetType = (PacketType)packetID;

						// 타입에 맞는 패킷 객체 생성 후 큐에 추가
						var basePacket = packetType.CreatePacket(_reader);
						_packetQueue.Enqueue(basePacket);
					}
					catch (Exception) {
						break;
					}
				}
			}, cancellationToken: this.GetCancellationTokenOnDestroy());
		}

		public void Register(string id, string password) {
			var salt = CryptoUtil.GetUniqueString(32);
			var encryptedPassword = CryptoUtil.SHA256(password + salt);
			SendPacket(new ClientRegisterPacket(id, encryptedPassword, salt));
		}

		public async UniTaskVoid Login(string id, string password) {
			SendPacket(new ClientRequestSaltPacket(id));
			await UniTask.WaitUntil(() => !string.IsNullOrEmpty(_salt), PlayerLoopTiming.LastUpdate, cancellationToken: this.GetCancellationTokenOnDestroy());
			var encryptedPassword = CryptoUtil.SHA256(password + _salt);
			SendPacket(new ClientLoginPacket(id, encryptedPassword));
		}

		public void AddRecord(float swipeDistance) {
			SendPacket(new ClientRecordPacket(_token, swipeDistance));
		}

		public void RequestRank(int page = 0, int recordsPerPage = 6) {
			SendPacket(new ClientRequestRankPacket(page, recordsPerPage));
		}

		private void SendPacket(IPacket packet) {
			if (!_client.Connected) {
				Toast.Instance.Error("서버에 연결되지 않았습니다!");
				return;
			}

			Debug.Log($"[C -> S] {packet}");
			_writer.Write(packet);
		}

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		private static void ResetStatic() {
			Instance = null;
		}
	}
}
