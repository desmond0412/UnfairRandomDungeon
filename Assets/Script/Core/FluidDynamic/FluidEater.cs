using UnityEngine;
using System.Collections;

namespace Artoncode.Core.Fluid {
	[RequireComponent(typeof(FluidFixtureShape))]
	public class FluidEater : MonoBehaviour {
		
		public FluidSystem fluidSystem;
		private FluidFixtureShape shape;
		
		void Start () {
			shape = GetComponent<FluidFixtureShape> ();
		}

		void FixedUpdate () {
			fluidSystem.destroyParticlesFillShape (shape);
		}
	}
}