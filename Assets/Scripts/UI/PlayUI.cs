using System;
using System.Text;
using Cysharp.Threading.Tasks;
using PushCar.Common;
using PushCar.Common.Models;
using PushCar.Common.Packets.Server;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PushCar.UI {
	public class PlayUI : MonoBehaviour {
		[SerializeField] private CarController _car;
		[SerializeField] private Transform _flag;
		[SerializeField] private GameObject _retryButton;
		[SerializeField] private TextMeshProUGUI _distanceText;
		[SerializeField] private TextMeshProUGUI _rankText;


		private void Start() {
			_retryButton.SetActive(false);
			NetworkManager.Instance.RequestRank();
		}

		private void OnEnable() {
			NetworkManager.Instance.OnPacketReceived += OnPacketReceived;
		}

		private void OnDisable() {
			NetworkManager.Instance.OnPacketReceived -= OnPacketReceived;
		}

		private void OnPacketReceived(IPacket incoming) {
			if (incoming is ServerResponseRankPacket packet) {
				var records = packet.Records;
				var sb = new StringBuilder();
				for (var i = 0; i < records.Count; i++) {
					var record = records[i];
					sb.AppendLine($"{i + 1}. {record.Id} - {record.Distance:F2}m");
				}
				_rankText.text = sb.ToString();
			}
		}

		private void Update() {
			UpdateDistanceUI();
		}

		private void UpdateDistanceUI() {
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
