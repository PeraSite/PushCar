using DG.Tweening;
using TMPro;
using UnityEngine;

namespace PushCar.UI {
	public class Toast : MonoBehaviour {
		[SerializeField] private RectTransform _toast;
		[SerializeField] private TextMeshProUGUI _toastText;

		[SerializeField] private float _showTime = 1f;
		[SerializeField] private float _animationTime = 0.5f;

		[SerializeField] private Color _infoColor;
		[SerializeField] private Color _errorColor;

		public static Toast Instance { get; private set; }

		private void Awake() {
			if (Instance != null && Instance != this) {
				Destroy(gameObject);
				return;
			}

			Instance = this;
			DontDestroyOnLoad(gameObject);
		}

		private void Show(string message) {
			_toastText.text = message;
			_toast.DOKill();
			DOTween.Sequence(_toast)
				.OnStart(() => _toast.gameObject.SetActive(true))
				.Join(_toast.DOAnchorPosY(-30f, _animationTime))
				.AppendInterval(_showTime)
				.Append(_toast.DOAnchorPosY(100f, _animationTime))
				.OnComplete(() => _toast.gameObject.SetActive(false));
		}

		public void Info(string message) {
			_toastText.color = _infoColor;
			Show(message);
		}

		public void Error(string message) {
			_toastText.color = _errorColor;
			Show(message);
		}

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		private static void ResetInstance() {
			Instance = null;
		}
	}
}
