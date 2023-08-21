using UnityEngine;
using System.Collections.Generic;

namespace IPTech.ClickerLibrary {
	[CreateAssetMenu(fileName = "ClickerEngineInfo", menuName = "IPTech/ClickerEngine/ClickerEngineInfo")]
	public class ClickerEngineInfo : ScriptableObject {
		public List<ClickerInfo> Clickers = new List<ClickerInfo>();
		public Vector3 LevelProgression = new Vector3(2,0,10);
		
		virtual public int GetLevelCost(int ForLevel) {
			int level = ForLevel - 1;
			float y = (LevelProgression.x*level)*(LevelProgression.x*level) + (LevelProgression.y*level) + LevelProgression.z;
			return (int)Mathf.Floor(y);
		}
	}
}
