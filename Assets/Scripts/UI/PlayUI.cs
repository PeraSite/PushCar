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
			if (_car.Stopped) {
				_retryButton.SetActive(true);
				_distanceText.text = distance <= 0f ? "Game Over!" : $"Result: {distance:F2}m";
				return;
			}

			_distanceText.text = $"Distance: {distance:F2}m";
		}

		public void Retry() {
			SceneManager.LoadScene("Play");
		}
	}
}
