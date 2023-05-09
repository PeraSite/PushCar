using PushCar.Common.Models;
using UnityEngine;

namespace PushCar {
	[CreateAssetMenu(fileName = "ReplayCache", menuName = "ReplayCache", order = 0)]
	public class ReplayCache : ScriptableObject {
		public Record Record;
	}
}
