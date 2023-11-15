using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IPTech.MathUtils;

namespace IPTech.GameUtils {
	public interface IValueInterpolator {
		float TargetValue { get; set; }
		float CurrentValue { get; set; }
		float Update(float deltaTime);
		bool NeedsUpdate();
	}

	public class ValueIterpolator : IValueInterpolator {
		float _duration;
		float _currentTime;
		float _targetValue;
		float _startValue;
		float _currentValue;

		public ValueIterpolator(float duration) {
			_duration = duration;
		}

		public float TargetValue {
			get {
				return _targetValue;
			}
			set {
				if(_targetValue!=value) {
					_targetValue = value;
					_currentTime = 0F;
					_startValue = _currentValue;
				}
			}
		}

		public float CurrentValue {
			get {
				return _currentValue;
			}
			set {
				_currentValue = value;
				_targetValue = value;
				_startValue = value;
				_currentTime = 1F;
			}
		}

		public float Update(float deltaTime) {
			if(!NeedsUpdate()) return _currentValue;

			_currentTime = Mathf.Clamp(_currentTime + (deltaTime / _duration), 0, 1);
			_currentValue = Tween.Linear(_startValue, _targetValue, _currentTime);
			return _currentValue;
		}

		public bool NeedsUpdate() {
			return _currentValue != _targetValue;
		}
	}
}
