using System.Collections.Generic;
using PushCar.Common;
using PushCar.Common.Packets.Server;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PushCar.UI.Rank {
	public class RankDialog : MonoBehaviour {
		[SerializeField] private GameObject _rankDialog;
		[SerializeField] private RankElement _elementPrefab;
		[SerializeField] private RectTransform _elementParent;
		[SerializeField] private TextMeshProUGUI _pageText;

		[SerializeField] private int _recordsPerPage;

		[SerializeField] private Button _prevButton;
		[SerializeField] private Button _nextButton;

		private int _currentPage;
		private int _maxPage;
		private List<RankElement> _elements;

		private void Awake() {
			_elements = new List<RankElement>();
		}

		private void OnEnable() {
			NetworkManager.Instance.OnPacketReceived += OnPacketReceived;
		}

		private void OnDisable() {
			NetworkManager.Instance.OnPacketReceived -= OnPacketReceived;
		}

		private void OnPacketReceived(IPacket packet) {
			if (packet is ServerResponseRankPacket rankPacket) {
				_maxPage = rankPacket.MaxPage;

				_prevButton.interactable = _currentPage != 0;
				_nextButton.interactable = _currentPage < _maxPage;

				_elements.ForEach(x => Destroy(x.gameObject));
				_elements.Clear();

				_pageText.text = $"{_currentPage + 1} / {_maxPage + 1}";

				rankPacket.Records.ForEach(x => {
					var rank = rankPacket.Records.IndexOf(x) + 1 + rankPacket.CurrentPage * _recordsPerPage;
					var element = Instantiate(_elementPrefab, _elementParent);
					element.Init(rank, x);
					_elements.Add(element);
				});
			}
		}

		public void Open() {
			_rankDialog.SetActive(true);
			UpdateUI();
		}

		public void Close() {
			_rankDialog.SetActive(false);
		}

		public void OnClickNext() {
			if (_currentPage >= _maxPage)
				return;

			_currentPage++;
			UpdateUI();
		}

		public void OnClickPrev() {
			if (_currentPage <= 0)
				return;
			_currentPage--;
			UpdateUI();
		}

		private void UpdateUI() {
			NetworkManager.Instance.RequestRank(_currentPage, _recordsPerPage);
		}
	}
}
