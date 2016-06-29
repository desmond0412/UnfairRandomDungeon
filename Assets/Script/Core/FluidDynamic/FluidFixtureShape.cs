using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Artoncode.Core.Fluid {
	public abstract class FluidFixtureShape : MonoBehaviour {
		protected List<FluidSystem> fluidSystems;
		public List<GameObject> listeners;

		public abstract void computeDistance (FluidParticle fp, out float distance, out Vector2 normal);

		public abstract FluidAABB getAABB ();

		public abstract bool testPoint (Vector2 p);

		public abstract bool isTrigger ();

		public void addTo (FluidSystem fs) {
			if (fluidSystems == null) {
				fluidSystems = new List<FluidSystem> ();
			}
			fluidSystems.Add (fs);
		}

		public void removeFrom (FluidSystem fs) {
			if (fluidSystems == null) {
				return;
			}
			fluidSystems.Remove (fs);
		}

		public void notifyOnTrigger (FluidParticleBodyContact bodyContact) {
			foreach (GameObject receiver in listeners.ToArray()) {
				MonoBehaviour[] components = receiver.GetComponents<MonoBehaviour> ();
				foreach (MonoBehaviour component in components) {
					if (component is IFluidFixtureTriggerListener) {
						if (component.enabled) {
							IFluidFixtureTriggerListener l = component as IFluidFixtureTriggerListener;
							l.fluidOnTrigger (bodyContact);
						}
					}
				}
			}
		}
	}
}