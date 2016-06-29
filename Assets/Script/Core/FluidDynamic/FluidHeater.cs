using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Artoncode.Core.Fluid {
	[RequireComponent(typeof(FluidFixtureShape))]
	public class FluidHeater : MonoBehaviour {
		
		public FluidSystem fluidSystem;
		public float degreePerSecond;
		private FluidFixtureShape shape;
		
		void Start () {
			shape = GetComponent<FluidFixtureShape> ();
		}
		
		void FixedUpdate () {
			List<FluidParticle> fps = fluidSystem.getParticlesInShape (shape);
			foreach (FluidParticle fp in fps) {
				fp.temperature += degreePerSecond * Time.fixedDeltaTime;
			}
		}
	}
}