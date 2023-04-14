using System;
using UnityEngine;

namespace PushCar {
	public class CarController : MonoBehaviour {
		[SerializeField] private float _swipeMultiplier = 1f;
		[SerializeField] private AudioClip _carSfx;
		[SerializeField] private Transform _car;
		[SerializeField] private Transform _flag;

		public bool CanMove;
		public bool Stopped;
		public float TargetX;

		private Vector2 _lastClickedPosition;
		private float _swipeLength;

		private void Awake() {
			CanMove = true;
		}

		private void Update() {
			if (Input.GetMouseButtonDown(0)) {
				_lastClickedPosition = Input.mousePosition;
			}

			if (Input.GetMouseButtonUp(0) && CanMove) {
				Vector2 endPos = Input.mousePosition;
				_swipeLength = (endPos.x - _lastClickedPosition.x) / Screen.width * _swipeMultiplier;
				TargetX = CalculateFinalPosition(_swipeLength);

				// Play audio
				AudioSource.PlayClipAtPoint(_carSfx, this.transform.position);

				// Make player can't move
				CanMove = false;
			}

			if (!CanMove) {
				var position = transform.position;
				var newX = Mathf.Lerp(position.x, TargetX, Time.deltaTime);
				transform.position = new Vector3(newX, position.y, position.z);
			}

			if (IsStopped() && !CanMove && !Stopped) {
				Stopped = true;

				NetworkManager.Instance.AddRecord(_swipeLength);
			}
		}

		private float CalculateFinalPosition(float swipeDistance) {
			var from = transform.position.x;
			var to = from + swipeDistance;
			return to;
		}

		private bool IsStopped() => Math.Abs(this.transform.position.x - TargetX) < 0.01f;

		private float CalculateScore() => _flag.position.x - _car.position.x;
	}
}
