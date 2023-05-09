using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PushCar.UI {
	public class ReplayUI : MonoBehaviour {
		[SerializeField] private ReplayCarController _replayCar;
		[SerializeField] private Transform _flag;
		[SerializeField] private GameObject _replayButton;
		[SerializeField] private TextMeshProUGUI _distanceText;

		private void Start() {
			_replayButton.SetActive(false);
		}

		private void Update() {
			UpdateDistanceUI();
		}

		private void UpdateDistanceUI() {
			var distance = _flag.position.x - _replayCar.transform.position.x;

			if (_replayCar.Stopped) {
				_replayButton.SetActive(true);
				_distanceText.text = $"Result: {_flag.position.x - _replayCar.TargetX:F2}m";
				return;
			}

			_distanceText.text = $"Distance: {distance:F2}m";
		}

		public void Replay() {
			SceneManager.LoadScene("Replay");
		}

		public void Return() {
			SceneManager.LoadScene("Play");
		}
	}
}
