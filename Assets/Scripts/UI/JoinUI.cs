using System.Net;
using TMPro;
using UnityEngine;

namespace PushCar.UI {
	public class JoinUI : MonoBehaviour {
		[SerializeField] private TMP_InputField _ipInput;
		[SerializeField] private TMP_InputField _portInput;

		public void Join() {
			if (!IPAddress.TryParse(_ipInput.text, out var ip)) {
				Toast.Instance.Error("올바르지 않은 IP입니다!");
				return;
			}

			if (!int.TryParse(_portInput.text, out var port)) {
				Toast.Instance.Error("올바르지 않은 포트입니다!");
				return;
			}

			var endpoint = new IPEndPoint(ip, port);
			NetworkManager.Instance.Join(endpoint).Forget();
		}
	}
}
