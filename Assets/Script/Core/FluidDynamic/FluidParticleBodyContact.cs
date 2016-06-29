using UnityEngine;
using System.Collections;

namespace Artoncode.Core.Fluid {
	public class FluidParticleBodyContact {
		public FluidParticle fp;
		public Rigidbody2D body;
		public FluidFixtureShape fixture;
		public float weight;
		public Vector2 normal;
		public float mass;
	}
}