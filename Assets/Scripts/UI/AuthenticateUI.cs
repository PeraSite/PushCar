using TMPro;
using UnityEngine;

namespace PushCar.UI {
	public class AuthenticateUI : MonoBehaviour {
		[SerializeField] private TMP_InputField _idInput;
		[SerializeField] private TMP_InputField _passwordInput;

		public void Login() {
			var id = _idInput.text;
			var password = _passwordInput.text;

			if (string.IsNullOrEmpty(id)) {
				Toast.Instance.Error("아이디를 입력해주세요!");
				return;
			}

			if (string.IsNullOrEmpty(password)) {
				Toast.Instance.Error("비밀번호를 입력해주세요!");
				return;
			}

			NetworkManager.Instance.Login(id, password).Forget();
		}

		public void Register() {
			var id = _idInput.text;
			var password = _passwordInput.text;

			if (string.IsNullOrEmpty(id)) {
				Toast.Instance.Error("아이디를 입력해주세요!");
				return;
			}

			if (string.IsNullOrEmpty(password)) {
				Toast.Instance.Error("비밀번호를 입력해주세요!");
				return;
			}

			NetworkManager.Instance.Register(id, password);
		}
	}
}
