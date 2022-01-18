using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IPTech.MathUtils {
	public static class Tween {

		public static float Linear(float fromValue, float toValue, float t) {
			float deltaValue = toValue - fromValue;
			return deltaValue * t + fromValue;
		}

		public static float EaseInQuadratic(float fromValue, float toValue, float t) {
			float deltaValue = toValue - fromValue;
			return deltaValue * t * t + fromValue;
		}

		public static float EaseOutQuadratic(float fromValue, float toValue, float t) {
			float delta = toValue - fromValue;
			return -delta * t * (t - 2) + fromValue;
		}

		public static float EaseInOutQuadratic(float fromValue, float toValue, float t) {
			float midValue = fromValue + ((toValue - fromValue) * 0.5f);
			t *= 2F;
			if(t < 1) return EaseInQuadratic(fromValue, midValue, t);
			return EaseOutQuadratic(midValue, toValue, --t);
		}

		public static float EaseInCubic(float fromValue, float toValue, float t) {
			float delta = toValue - fromValue;
			return delta * t * t * t + fromValue;
		}

		public static float EaseOutCubic(float fromValue, float toValue, float t) {
			float delta = toValue - fromValue;
			t--;
			return delta * (t * t * t + 1) + fromValue;
		}

		public static float EaseInOutCubic(float fromValue, float toValue, float t) {
			float midValue = fromValue + ((toValue - fromValue) * 0.5F);
			t *= 2F;
			if(t < 1) return EaseInCubic(fromValue, midValue, t);
			return EaseOutCubic(midValue, toValue, --t);
		}

		public static float EaseInQuartic(float fromValue, float toValue, float t) {
			float delta = toValue - fromValue;
			return delta * t * t * t * t + fromValue;
		}

		public static float EaseOutQuartic(float fromValue, float toValue, float t) {
			float delta = toValue - fromValue;
			t--;
			return -delta * (t * t * t * t - 1) + fromValue;
		}

		public static float EaseInOutQuartic(float fromValue, float toValue, float t) {
			float midValue = fromValue + ((toValue - fromValue) * 0.5f);
			t *= 2F;
			if(t < 1F) return EaseInQuartic(fromValue, midValue, t);
			return EaseOutQuartic(midValue, toValue, --t);
		}

		public static float EaseInQuintic(float fromValue, float toValue, float t) {
			float delta = toValue - fromValue;
			return delta * t * t * t * t * t + fromValue;
		}

		public static float EaseOutQuintic(float fromValue, float toValue, float t) {
			float delta = toValue - fromValue;
			t--;
			return delta * (t * t * t * t * t + 1) + fromValue;
		}

		public static float EaseInOutQuintic(float fromValue, float toValue, float t) {
			float midValue = fromValue + ((toValue - fromValue) * 0.5f);
			t *= 2F;
			if(t < 1) return EaseInQuintic(fromValue, midValue, t);
			return EaseInOutQuintic(midValue, toValue, --t);
		}

		public static float EaseInSin(float fromValue, float toValue, float t) {
			float delta = toValue - fromValue;
			return -delta * Mathf.Cos(t * (Mathf.PI * 0.5f)) + delta + fromValue;
		}

		public static float EaseOutSin(float fromValue, float toValue, float t) {
			float delta = toValue - fromValue;
			return delta * Mathf.Sin(t * (Mathf.PI * 0.5f)) + fromValue;
		}

		public static float EaseInOutSin(float fromValue, float toValue, float t) {
			float delta = toValue - fromValue;
			return -delta * 0.5f * (Mathf.Cos(Mathf.PI * t) - 1F) + fromValue;
		}

		public static float EaseInExponential(float fromValue, float toValue, float t) {
			float delta = toValue - fromValue;
			return delta * Mathf.Pow(2F, 10F * (t - 1F)) + fromValue;
		}

		public static float EaseOutExponential(float fromValue, float toValue, float t) {
			float delta = toValue - fromValue;
			return delta * (-Mathf.Pow(2F, -10F * t) + 1F) + fromValue;
		}

		public static float EaseInOutExponential(float fromValue, float toValue, float t) {
			float midValue = fromValue + ((toValue - fromValue) * 0.5f);
			t *= 2F;
			if(t < 1) return EaseInExponential(fromValue, midValue, t);
			return EaseOutExponential(midValue, fromValue, --t);
		}

		public static float EaseInCircular(float fromValue, float toValue, float t) {
			float delta = toValue - fromValue;
			return -delta * (Mathf.Sqrt(1F - t * t) - 1F) + fromValue;
		}

		public static float EaseOutCircular(float fromValue, float toValue, float t) {
			float delta = toValue - fromValue;
			t--;
			return delta * Mathf.Sqrt(1F - t * t) + fromValue;
		}

		public static float EaseInOutCircular(float fromValue, float toValue, float t) {
			float midValue = fromValue + ((toValue - fromValue) * 0.5f);
			t *= 2F;
			if(t < 1) return EaseInCircular(fromValue, midValue, t);
			return EaseOutCircular(midValue, fromValue, --t);
		}
	}
}