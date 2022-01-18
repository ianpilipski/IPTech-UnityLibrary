using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IPTech.Unity {
	public interface IUETime {
		float time { get; }
		float timeScale { get; set; }
		float unscaledTime { get; }
	}

	public class UETime : IUETime {
		public float time => Time.time;
		public float timeScale {
			get => Time.timeScale; 
			set => Time.timeScale = value; 
		}

		public float unscaledTime {
			get => Time.unscaledTime;
		}
	}
}
