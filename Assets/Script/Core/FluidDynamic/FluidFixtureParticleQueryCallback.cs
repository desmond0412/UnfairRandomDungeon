using UnityEngine;
using System.Collections;

namespace Artoncode.Core.Fluid {
	public abstract class FluidFixtureParticleQueryCallback {
		public FluidSystem fluidSystem;

		public abstract void reportFixtureAndParticle (FluidFixtureShape fixture, FluidParticle fp);
	}
}
