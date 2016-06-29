using UnityEngine;
using System.Collections;

namespace Artoncode.Core.Fluid {
	public class FluidParticleContact {
		public FluidParticle a;
		public FluidParticle b;
		public float weight;
		public Vector2 normal;
		public FluidParticleType flags;
	}
}