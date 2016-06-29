using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Artoncode.Core.Fluid {
	public class FluidParticleGenerator : MonoBehaviour {

		public FluidSystem fluidSystem;
		public List<FluidFixtureShape> shapes;
		public FluidParticleDef particleDef;

		void Start () {
			foreach (FluidFixtureShape shape in shapes) {
				if (shape.gameObject.activeSelf && shape.enabled) {
					fluidSystem.createParticlesFillShapeForGroup (shape, particleDef);
				}
			}
		}
	}
}