using System.Net;
using TMPro;
using UnityEngine;

public class JoinUI : MonoBehaviour {
	[SerializeField] private TMP_InputField _ipInput;
	[SerializeField] private TMP_InputField _portInput;

	public void Join() {
		if (!IPAddress.TryParse(_ipInput.text, out var ip)) {
			Debug.LogError("IP is invalid");
			return;
		}

		if (!int.TryParse(_portInput.text, out var port)) {
			Debug.LogError("Port is invalid integer");
			return;
		}

		var endpoint = new IPEndPoint(ip, port);
		NetworkManager.Instance.Join(endpoint);
	}
}
