using UnityEngine;
using System.Collections;

namespace Artoncode.Core.Fluid {
	public class FluidEmitter : MonoBehaviour {
		
		public FluidSystem fluidSystem;
		public FluidParticleDef particleDef;
		public Vector2 velocity;
		public float interval = 0.1f;
		private FluidFixtureShape shape;
		private float timeSinceLastEmit;

		void Start () {
			shape = GetComponent<FluidFixtureShape> ();
		}

		public void startEmitting () {
			StartCoroutine ("emit");
		}

		void Update() {
			timeSinceLastEmit += Time.deltaTime;
		}

		public void burst(int count) {
			for (int i=0;i<count;i++) {
				particleDef.velocity = velocity;
				particleDef.position = transform.position;
				fluidSystem.createParticle (particleDef);
			}
			timeSinceLastEmit = 0;
		}

		IEnumerator emit () {
			while (true) {
				particleDef.velocity = velocity;
				if (shape) {
					fluidSystem.createParticlesFillShapeForGroup (shape, particleDef);
				}
				else {
					particleDef.position = transform.position;
					fluidSystem.createParticle (particleDef);
				}
				timeSinceLastEmit = 0;
				yield return new WaitForSeconds (interval);
			}
		}

		public void stopEmitting () {
			StopCoroutine ("emit");
		}

		public float getTimeSinceLastEmit() {
			return timeSinceLastEmit;
		}
	}
}