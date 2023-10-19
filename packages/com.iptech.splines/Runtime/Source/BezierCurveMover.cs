using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace IPTech.Splines
{
	public class BezierCurveMover : MonoBehaviour
	{
		private float startTime = 0.0f;
		private bool hasGoalBeenReached = false;
		
		public BezierCurve curveToMoveAlong = null;
		public float TimeToTraverseEntireCurveInSeconds = 3.0f;

		public UnityEvent GoalReached = null;

		// Use this for initialization
		void Start() {
			Reset();
		}

		// Update is called once per frame
		void Update() {
			ConditionalUpdateStartTime();
			UpdatePosition();
			ConditionalFireGoalReached();
		}

#if UNITY_EDITOR
		public bool PreviewMovementUpdate() {
			Update();
			return this.hasGoalBeenReached;
		}
#endif

		private void ConditionalUpdateStartTime() {
			if(this.startTime==0.0f) {
				this.startTime = GetTime();
			}
		}

		private float GetTime() {
			return Application.isPlaying ? Time.time : Time.realtimeSinceStartup;
		}

		private void UpdatePosition() {
			if(this.TimeToTraverseEntireCurveInSeconds==0.0f) {
				return;
			}
			float t = CalculateT();
			Vector3 positionOnCurve = this.curveToMoveAlong.CalculatePointOnCurveAt(t);
			this.transform.position = positionOnCurve;
		}

		private void ConditionalFireGoalReached() {
			if (!this.hasGoalBeenReached) {
				if(CalculatePercentComplete() == 1F) { 
					this.hasGoalBeenReached = true;
					if (this.GoalReached != null) {
						this.GoalReached.Invoke();
					}
				}
			}
		}

		private float CalculateT() {
			float pctComplete = CalculatePercentComplete();
			if (this.TimeToTraverseEntireCurveInSeconds<0.0f) {
				return 1.0f - pctComplete;
			}
			return pctComplete;
		}

		private float CalculatePercentComplete() {
			float deltaT = this.startTime == 0.0f ? 0.0f : (GetTime() - this.startTime);
			return Mathf.Clamp( Mathf.Abs(deltaT / this.TimeToTraverseEntireCurveInSeconds), 0F, 1F);
		}

		public void Reset() {
			this.startTime = 0.0f;
			this.hasGoalBeenReached = false;
			UpdatePosition();
		}
	}
}