using PushCar.Common.Models;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace PushCar.UI.Rank {
	public class RankElement : MonoBehaviour {
		[SerializeField] private TextMeshProUGUI _rank;
		[SerializeField] private TextMeshProUGUI _distance;
		[SerializeField] private TextMeshProUGUI _date;
		[SerializeField] private TextMeshProUGUI _id;
		[SerializeField] private Button _replayButton;
		[SerializeField] private ReplayCache _replayCache;

		public void Init(int rank, Record record) {
			_rank.text = $"{rank}";
			_distance.text = $"{record.Distance:F2}m";
			_date.text = $"{record.Time:yyyy-MM-dd HH:mm:ss}";
			_id.text = record.Id;
			_replayButton.onClick.AddListener(() => {
				_replayCache.Record = record;
				SceneManager.LoadScene("Replay");
			});
		}
	}
}
