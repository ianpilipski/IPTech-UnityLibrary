using UnityEngine;

namespace IPTech.Unity.Particles
{
	public class AttractorFunction_ConstantForce : ParticleAttractor
	{

		public float Force = 9.8f;

		protected override void UpdateParticleSystem() {
			int count = this.ParticleSystemToAffect.GetParticles(particles);
			for (int i = 0; i < count; i++) {
				ParticleSystem.Particle p = this.particles[i];
				Vector3 direction = (this.transform.position - (p.position + this.ParticleSystemToAffect.transform.position)).normalized;
				p.velocity += direction * (this.Force * Time.deltaTime);
				this.particles[i] = p;
			}
			this.ParticleSystemToAffect.SetParticles(particles, count);
		}
	}
}