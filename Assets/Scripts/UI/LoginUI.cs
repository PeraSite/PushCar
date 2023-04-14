using TMPro;
using UnityEngine;

namespace PushCar.UI {
	public class LoginUI : MonoBehaviour {
		[SerializeField] private TMP_InputField _idInput;
		[SerializeField] private TMP_InputField _passwordInput;

		public void Join() {
			var id = _idInput.text;
			var password = _passwordInput.text;

			if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(password)) {
				Debug.LogError("Please provide enough data");
				return;
			}

			NetworkManager.Instance.Authenticate(id, password);
		}
	}
}
