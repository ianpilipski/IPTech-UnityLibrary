using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using IPTech.Unity.Core;
using System.Runtime.Serialization;

namespace IPTech.Unity.Particles
{
	[ExecuteInEditMode]
	public abstract class ParticleAttractor : MonoBehaviour
	{
		public ParticleSystem ParticleSystemToAffect = null;
		protected ParticleSystem.Particle[] particles = null;

		private void Update() {
			if (this.ParticleSystemToAffect != null) {
				Initialize();
				UpdateParticleSystem();
			}
		}

		public void SetParticleSystemToAffect(ParticleSystem particleSystem) {
			this.ParticleSystemToAffect = particleSystem;
		}

		protected abstract void UpdateParticleSystem();

		private void Initialize() {
			if (this.particles == null || this.particles.Length != this.ParticleSystemToAffect.main.maxParticles) {
				this.particles = new ParticleSystem.Particle[this.ParticleSystemToAffect.main.maxParticles];
			}
		}
	}
}
