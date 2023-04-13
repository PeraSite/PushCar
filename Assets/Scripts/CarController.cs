using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

namespace PushCar {
	public class CarController : MonoBehaviour {
		const int PORT = 8080;

		[SerializeField] private float _movePower = 1f;
		[SerializeField] private AudioClip _carSfx;
		[SerializeField] private Transform _car;
		[SerializeField] private Transform _flag;

		public bool CanMove;
		public bool Stopped;

		private Rigidbody2D _rigidbody;
		private Vector2 _lastClickedPosition;

		private void Awake() {
			_rigidbody = GetComponent<Rigidbody2D>();
			CanMove = true;
		}

		private void Update() {
			if (Input.GetMouseButtonDown(0)) {
				_lastClickedPosition = Input.mousePosition;
			}

			if (Input.GetMouseButtonUp(0) && CanMove) {
				// Add force to the car
				var delta = (Vector2)Input.mousePosition - _lastClickedPosition;
				delta.y = 0f;
				if (delta.x <= 0) delta.x = 0f;
				_rigidbody.AddForce(delta * _movePower, ForceMode2D.Impulse);

				// Play audio
				AudioSource.PlayClipAtPoint(_carSfx, this.transform.position);

				// Make player can't move
				CanMove = false;
			}

		#if UNITY_64

  #endif

			if (IsStopped() && !CanMove && !Stopped) {
				var distance = CalculateDistance();
				Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
				if (distance <= 0f) distance = -999;

				var buffer = Encoding.UTF8.GetBytes(distance.ToString("F2"));
				socket.SendTo(buffer, new IPEndPoint(IPAddress.Loopback, PORT));


				var receiveBuffer = new byte[1024];
				EndPoint receivedEndpoint = new IPEndPoint(IPAddress.Any, 0);
				int receivedSize = socket.ReceiveFrom(receiveBuffer, ref receivedEndpoint);
				var received = Encoding.UTF8.GetString(receiveBuffer, 0, receivedSize);
				Debug.Log(received);
				Stopped = true;
			}
		}

		private bool IsStopped() => _rigidbody.velocity.x <= 0.0001;

		private float CalculateDistance() => _flag.position.x - _car.position.x;
	}
}
