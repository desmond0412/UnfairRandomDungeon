using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Artoncode.Core.Fluid {
	[RequireComponent(typeof(FluidFixtureShape))]
	public class FluidRelocator : MonoBehaviour {

		public FluidSystem fluidSystem;
		public Transform target;
		public Vector2 velocity;
		private FluidFixtureShape shape;

		void Start () {
			shape = GetComponent<FluidFixtureShape> ();
		}
	
		void FixedUpdate () {
			List<FluidParticle> fps = fluidSystem.getParticlesInShape (shape);
			if (fps.Count > 0) {
				fps[0].position = target.position;
				fps[0].velocity = velocity;
			}
//			foreach (FluidParticle fp in fps) {
//				fp.position = target.position;
//				fp.velocity = velocity;
//			}
		}
	}
}