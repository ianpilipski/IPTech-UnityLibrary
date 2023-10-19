using System;
using UnityEngine;

namespace IPTech.GameUtils {
	public class AutoOscillate : MonoBehaviour {
		float _initialRealTimeSinceStartup;
		Vector3 _currentOffsetPos;
		Vector3 _currentOffsetRot;

		public Vector3andSpace moveUnitsPerSecond;
		public Vector3andSpace rotateDegreesPerSecond;
		public bool ignoreTimescale;

		private void Start()
		{
			_initialRealTimeSinceStartup = Time.realtimeSinceStartup;
			_currentOffsetPos = Vector3.zero;
			_currentOffsetRot = Vector3.zero;
		}

		void Update()
		{
			float offsetStrength = CalculateOffsetStrength();

			UpdateTranslation();
			UpdateRotation();
   
			float CalculateOffsetStrength() {
				float retVal = Time.time;
				if(ignoreTimescale) {
					retVal = (Time.realtimeSinceStartup - _initialRealTimeSinceStartup);
				}
				return Mathf.Sin(retVal * 2F);
			}

			void UpdateTranslation() {
				if(moveUnitsPerSecond.space == Space.Self) {
					Vector3 pos = transform.localPosition - _currentOffsetPos;
					_currentOffsetPos = (moveUnitsPerSecond.value * offsetStrength);
					transform.localPosition = pos + _currentOffsetPos;
				} else {
					Vector3 pos = transform.position - _currentOffsetPos;
					_currentOffsetPos = (moveUnitsPerSecond.value * offsetStrength);
					transform.position = pos + _currentOffsetPos;
				}
			}

			void UpdateRotation() {
				if(rotateDegreesPerSecond.space == Space.Self) {
					Vector3 rot = transform.localRotation.eulerAngles - _currentOffsetRot;
					_currentOffsetRot = (rotateDegreesPerSecond.value * offsetStrength);
					transform.localRotation = Quaternion.Euler(rot + _currentOffsetRot);
				} else {
					Vector3 rot = transform.rotation.eulerAngles - _currentOffsetRot;
					_currentOffsetRot = (rotateDegreesPerSecond.value * offsetStrength);
					transform.rotation = Quaternion.Euler(rot + _currentOffsetRot);
				}
			}
		}


		[Serializable]
		public class Vector3andSpace
		{
			public Vector3 value;
			public Space space = Space.Self;
		}
	}
}