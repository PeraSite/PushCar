using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PushCar.UI {
	public class PlayUI : MonoBehaviour {
		[SerializeField] private CarController _car;
		[SerializeField] private Transform _flag;
		[SerializeField] private GameObject _retryButton;

		[SerializeField] private TextMeshProUGUI _distanceText;

		private void Start() {
			_retryButton.SetActive(false);
		}

		private void Update() {
			UpdateUI();
		}

		private void UpdateUI() {
			if (_car.CanMove) {
				_distanceText.text = "Swipe to move!";
				return;
			}

			var distance = _flag.position.x - _car.transform.position.x;
			if (distance <= 0f) {
				_retryButton.SetActive(true);
				_distanceText.text = "Game Over!";
				return;
			}

			if (_car.Stopped) {
				_retryButton.SetActive(true);
				_distanceText.text = $"Result: {_flag.position.x - _car.TargetX:F2}m";
				return;
			}

			_distanceText.text = $"Distance: {distance:F2}m";
		}

		public void Retry() {
			SceneManager.LoadScene("Play");
		}
	}
}
