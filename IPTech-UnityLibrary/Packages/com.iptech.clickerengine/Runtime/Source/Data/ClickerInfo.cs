using UnityEngine;
using System.Collections;

namespace IPTech.ClickerLibrary {
	[CreateAssetMenu(fileName = "ClickerInfo", menuName = "IPTech/ClickerEngine/Create New Engine")]
	public class ClickerInfo : ScriptableObject {
		
		public string ClickerName = "";
		public string Group = null;

		public bool ManualClicker = false;

		public float CPSA = 0;
		public float CPSB = 1;
		public float CPSC = 0;

		public float GetClicksPerSecond(int ForCount) {
			Debug.Assert(ForCount >= 0);
			ForCount -= 1;
			float y = (CPSA * ForCount) * (CPSA * ForCount) + (CPSB * ForCount) + CPSC;
			return y;
		}
		
		public float A = 1;
		public float B = 0;
		public float C = 0;
		
		virtual public int GetCost(int ForCount) {
			Debug.Assert(ForCount>=0);
			ForCount -= 1;
			float y = (A*ForCount)*(A*ForCount) + (B*ForCount) + C;
			return (int)Mathf.Floor(y);
		}
	}
}