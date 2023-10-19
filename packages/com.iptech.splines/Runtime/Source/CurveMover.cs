using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace IPTech.Splines {
	public class CurveMover : MonoBehaviour {
		public BezierCurve Curve;
		public Transform ObjectToMove;
		public float SecondsToTraverse = 1F;
		public bool PlayOnAwake;

		public UnityEvent EndReached;
		

		float _currentTime;
		float _speed;
		bool _playing;

		Transform _transformToMove {
			get { return ObjectToMove == null ? transform : ObjectToMove; }
		}

		private void Awake() {
			_speed = 1F / SecondsToTraverse;
			_playing = PlayOnAwake;
		}

		public void Reset() {
			_currentTime = 0F;
			_playing = PlayOnAwake;
			SetTransformPosition();
		}

		public void Play() {
			Reset();
			_playing = true;
		}

		private void Update() {
			if(!_playing) return;

			_currentTime += Time.deltaTime * _speed;
			_currentTime = Mathf.Clamp(_currentTime, 0F, 1F);
			SetTransformPosition();

			if(_currentTime >= 1F) {
				EndReached.Invoke();
				_playing = false;
			}
		}

		private void SetTransformPosition() {
			_transformToMove.position = Curve.CalculatePointOnCurveAt(_currentTime);
		}
	}
}
