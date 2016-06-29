using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Artoncode.Core.Fluid {
	public class FluidParticleDestroyer : MonoBehaviour {
		
		public FluidSystem fluidSystem;
		public List<FluidFixtureShape> shapes;
		
		void Start () {
			foreach (FluidFixtureShape shape in shapes) {
				fluidSystem.destroyParticlesFillShape (shape);
			}
		}
	}
}