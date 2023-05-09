using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace PushCar {
	public class ReplayCarController : MonoBehaviour {
		[SerializeField] private AudioClip _carSfx;
		[SerializeField] private Transform _car;
		[SerializeField] private Transform _flag;
		[SerializeField] private ReplayCache _replayCache;

		public bool Stopped;
		public float TargetX;

		private void Start() {
			// Play audio
			AudioSource.PlayClipAtPoint(_carSfx, this.transform.position);

			// 목표 X축 위치 설정
			TargetX = _flag.transform.position.x - _replayCache.Record.Distance;
		}

		private void Update() {
			// 목표 X 값까지 이동시키기
			var position = transform.position;
			var newX = Mathf.Lerp(position.x, TargetX, Time.deltaTime);
			transform.position = new Vector3(newX, position.y, position.z);

			// 이동 후 멈췄으면 점수 전송
			if (IsStopped()) {
				Stopped = true;
			}
		}

		private bool IsStopped() => Math.Abs(this.transform.position.x - TargetX) < 0.01f;
	}
}
