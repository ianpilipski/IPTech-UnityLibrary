using UnityEngine;

namespace IPTech.Unity.Particles
{
	public class AtrractorFunction_SpringForce : ParticleAttractor
	{
		public float SpringConstant_K = 9.8f;
		public float Damping = 0.01f;

		protected override void UpdateParticleSystem() {
			int count = this.ParticleSystemToAffect.GetParticles(particles);
			for (int i = 0; i < count; i++) {
				ParticleSystem.Particle p = this.particles[i];
				Vector3 vectorTowardAffector = (this.transform.position - (p.position + this.ParticleSystemToAffect.transform.position));
				Vector3 unitDirection = vectorTowardAffector.normalized;
				Vector3 force = this.SpringConstant_K * vectorTowardAffector;
				p.velocity += (force * Time.deltaTime) - (p.velocity * this.Damping);
				this.particles[i] = p;
			}
			this.ParticleSystemToAffect.SetParticles(particles, count);

		}
	}
}