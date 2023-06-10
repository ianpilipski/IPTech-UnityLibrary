using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace IPTech.UnityAbstraction {
	public interface IUETime {
		float captureDeltaTime { get; set; }
		int captureFramerate { get; set; }
		float deltaTime { get; }
		float fixedDeltaTime { get; }
		float fixedTime { get; }
		double fixedTimeAsDouble { get; }
		float fixedUnscaledDeltaTime { get; }
		float fixedUnscaledTime { get; }
		double fixedUnscaledTimeAsDouble { get; }
		int frameCount { get; }
		bool inFixedTimeStep { get; }
		float maximumDeltaTime { get; set; }
		float maximumParticleDeltaTime { get; set; }
		float realtimeSinceStartup { get; }
		double realtimeSinceStartupAsDouble { get; }
		int renderedFrameCount { get; }
		float smoothDeltaTime { get; }
		float time { get; }
		double timeAsDouble { get; }
		float timeScale { get; set; }
		float timeSinceLevelLoad { get; }
		double timeSinceLevelLoadAsDouble { get; }
		float unscaledDeltaTime { get; }
		float unscaledTime { get; }
		double unscaledTimeAsDouble { get; }
	}

	public class UETime : IUETime {
		public float captureDeltaTime { get => Time.captureDeltaTime; set => Time.captureDeltaTime = value; }
		public int captureFramerate { get => Time.captureFramerate; set => Time.captureFramerate = value; }
		public float deltaTime => Time.deltaTime;
		public float fixedDeltaTime => Time.fixedDeltaTime;
		public float fixedTime => Time.fixedTime;
		public double fixedTimeAsDouble => Time.fixedTimeAsDouble;
		public float fixedUnscaledDeltaTime => Time.fixedUnscaledDeltaTime;
		public float fixedUnscaledTime => Time.fixedUnscaledTime;
		public double fixedUnscaledTimeAsDouble => Time.fixedUnscaledTimeAsDouble;
		public int frameCount => Time.frameCount;
		public bool inFixedTimeStep => Time.inFixedTimeStep;
		public float maximumDeltaTime { get => Time.maximumDeltaTime; set => Time.maximumDeltaTime = value; }
		public float maximumParticleDeltaTime { get => Time.maximumParticleDeltaTime; set => Time.maximumParticleDeltaTime = value; }
		public float realtimeSinceStartup => Time.realtimeSinceStartup;
		public double realtimeSinceStartupAsDouble => Time.realtimeSinceStartupAsDouble;
		public int renderedFrameCount => Time.renderedFrameCount;
		public float smoothDeltaTime => Time.smoothDeltaTime;
		public float time => Time.time;
		public double timeAsDouble => Time.timeAsDouble;
		public float timeScale { get => Time.timeScale; set => Time.timeScale = value; }
		public float timeSinceLevelLoad => Time.timeSinceLevelLoad;
		public double timeSinceLevelLoadAsDouble => Time.timeSinceLevelLoadAsDouble;
		public float unscaledDeltaTime => Time.unscaledDeltaTime;
		public float unscaledTime => Time.unscaledTime;
		public double unscaledTimeAsDouble => Time.unscaledTimeAsDouble;
	}
}
