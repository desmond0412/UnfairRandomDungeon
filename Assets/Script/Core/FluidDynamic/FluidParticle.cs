using System;
using UnityEngine;
using System.Collections;

namespace Artoncode.Core.Fluid {
    [Flags]
	public enum FluidParticleType {
		/// Water particle.
		WaterParticle = 0,
		/// Removed after next simulation step.
		ZombieParticle = 1 << 1,
		/// Zero velocity.
		WallParticle = 1 << 2,
		/// With restitution from stretching.
		SpringParticle = 1 << 3,
		/// With restitution from deformation.
		ElasticParticle = 1 << 4,
		/// With viscosity.
		ViscousParticle = 1 << 5,
		/// Without isotropic pressure.
		PowderParticle = 1 << 6,
		/// With surface tension.
		TensileParticle = 1 << 7,
		/// Mix color between contacting particles.
		ColorMixingParticle = 1 << 8,
		/// Call b2DestructionListener on destruction.
		DestructionListenerParticle = 1 << 9,
		/// Prevents other particles from leaking.
		BarrierParticle = 1 << 10,
		/// Less compressibility.
		StaticPressureParticle = 1 << 11,
		/// Makes pairs or triads with other particles.
		ReactiveParticle = 1 << 12,
		/// With high repulsive force.
		RepulsiveParticle = 1 << 13,
		/// Call b2ContactListener when this particle is about to interact with
		/// a rigid body or stops interacting with a rigid body.
		/// This results in an expensive operation compared to using
		/// b2_fixtureContactFilterParticle to detect collisions between
		/// particles.
		FixtureContactListenerParticle = 1 << 14,
		/// Call b2ContactListener when this particle is about to interact with
		/// another particle or stops interacting with another particle.
		/// This results in an expensive operation compared to using
		/// b2_particleContactFilterParticle to detect collisions between
		/// particles.
		ParticleContactListenerParticle = 1 << 15,
		/// Call b2ContactFilter when this particle interacts with rigid bodies.
		FixtureContactFilterParticle = 1 << 16,
		/// Call b2ContactFilter when this particle interacts with other
		/// particles.
		ParticleContactFilterParticle = 1 << 17,
	};

	public class FluidParticle {
		public FluidParticleType flags;
		public Vector2 position;
		public Vector2 prevPosition;
		public Vector2 velocity;
		public Color color;
		public Color foamColor;
		public float foamWeightThreshold;
		public float lifetime;
		public float mass;
		public Vector2 force;
		public float weight;
		public float staticPressure;
		public float accumulation;
		public Vector2 accumulation2;
		public float temperature;
		public float freezingTemperature;
		public float boilingTemperature;
		public float depth;
		public int lastBodyContactStep;
		public int bodyContactCount;
		public int consecutiveContactSteps;
		public float expirationTime;

//	void* userData;
//	b2ParticleGroup* group;
	}
}
